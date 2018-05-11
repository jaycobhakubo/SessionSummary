#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Messages;
using GameTech.Elite.Client.Modules.SessionSummary.Properties;
using GameTech.Elite.UI;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Represents control over session costs
    /// </summary>
    internal class SessionCostsController : ControllerBase, ISessionCostsController
    {
        #region Member Events
        /// <summary>
        /// Occurs when the controller retreives a list of availables session costs.
        /// </summary>
        public event AsyncCompletedEventHandler RequestCostsCompleted;

        /// <summary>
        /// Occurs when the controller completes a request to save a session cost.
        /// </summary>
        public event AsyncCompletedEventHandler RequestSaveCostCompleted;

        #endregion

        #region Member Variables
        private IEnumerable<SessionCost> m_sessionCosts;
        #endregion

        #region Contructors
        /// <summary>
        /// Intializes an instance of a SessionCostsController
        /// </summary>
        /// <param name="parent">The parent controller</param>
        public SessionCostsController(ISessionSummaryController parent)
        {
            if (parent == null)
                throw new ArgumentNullException();

            Parent = parent;

            SessionCosts = SendRequestCosts();
        }

        /// <summary>
        /// Closes the current view.
        /// </summary>
        public void CloseCurrentView()
        {
            Parent.CloseCurrentView();
        }
        #endregion

        #region Member Methods

        /// <summary>
        /// Requests a list of session costs.
        /// </summary>
        public void RequestCosts()
        {
            if (!IsBusy)
            {
                IsBusy = true;

                RunWorker(Resources.SavingCost,
                         DoRequestSaveCost,
                         null,
                         false,
                         OnRequestSaveCostCompleted);
            }
        }

        /// <summary>
        /// Gets session costs.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The work arguments.</param>
        private void DoRequestCosts(object sender, DoWorkEventArgs e)
        {
            e.Result = SendRequestCosts();
        }

        /// <summary>
        /// Sends the request to get the session costs
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SessionCost> SendRequestCosts()
        {
            GetSessionCosts msg = new GetSessionCosts();
            msg.Send();

            if (msg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msg.ReturnCode));

            return msg.Costs;
        }

        /// <summary>
        /// Handles when a request for session costs has completed.
        /// </summary>
        /// <param name="sender">The source of this event.</param>
        /// <param name="e">The arguments of the completed event.</param>
        private void OnRequestCostsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            if (!CheckForError(e.Error))
                SessionCosts = (IEnumerable<SessionCost>)e.Result;

            AsyncCompletedEventHandler handler = RequestCostsCompleted;
            if (handler != null)
                handler(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, null));
        }

        /// <summary>
        /// Request that a session cost be saved.
        /// </summary>
        /// <param name="cost">The cost to be saved.</param>
        public void RequestSaveCost(SessionCost cost)
        {
            if (!IsBusy)
            {
                IsBusy = true;

                RunWorker(Resources.SavingCost,
                         DoRequestSaveCost,
                         cost,
                         false,
                         OnRequestSaveCostCompleted);
            }
        }

        /// <summary>
        /// Saves a session cost
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The work arguments.</param>
        private void DoRequestSaveCost(object sender, DoWorkEventArgs e)
        {
            SetSessionCost msg = new SetSessionCost((SessionCost)e.Argument);
            msg.Send();

            if (msg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msg.ReturnCode));

            e.Result = SendRequestCosts();
        }

        /// <summary>
        /// Handles when a request save cost worker has completed.
        /// </summary>
        /// <param name="sender">The source of this event.</param>
        /// <param name="e">The arguments of the completed event.</param>
        private void OnRequestSaveCostCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            if (!CheckForError(e.Error))
                SessionCosts = (IEnumerable<SessionCost>)e.Result;

            AsyncCompletedEventHandler handler = RequestSaveCostCompleted;
            if (handler != null)
                handler(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, null));
        }

        /// <summary>
        /// Checks for an error after a message is called
        /// </summary>
        /// <param name="ex">The exception to check or null.</param>
        /// <returns>true if there is an error false otherwise</returns>
        protected override bool CheckForError(Exception ex)
        {
            if (ex == null)
                return false;
            else
            {
                if (ex is ServerCommException)
                    Parent.ServerCommFailure();
                else
                {
                    if (Parent.CurrentView != null)
                        MessageWindow.Show(Parent.CurrentView, ex.Message, Resources.SessionSummaryName, MessageWindowType.Close);
                    else
                        MessageWindow.Show(ex.Message, Resources.SessionSummaryName, MessageWindowType.Close);
                }

                return true;
            }
        }

        /// <summary>
        /// Shows a message asking if the user wants to save, not save, or
        /// cancel.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="canSave">Whether the user is allowed to save or
        /// not.</param>
        /// <returns>The result of the message window.</returns>
        public MessageBoxResult ShowObjectModifiedMessage(string message, bool canSave)
        {
            MessageWindowType type;
            string caption;

            if (canSave)
            {
                type = MessageWindowType.YesNoCancel;
                caption = Resources.SaveChanges;
            }
            else
            {
                type = MessageWindowType.OKCancel;
                caption = Resources.LoseChanges;
            }

            if (Parent.CurrentView != null)
                return MessageWindow.Show(Parent.CurrentView, message, caption, type);
            else
                return MessageWindow.Show(message, caption, type);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets this controllers parent controller.
        /// </summary>
        public ISessionSummaryController Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of session costs.
        /// </summary>
        public IEnumerable<SessionCost> SessionCosts
        {
            get
            {
                return m_sessionCosts;
            }
            private set
            {
                m_sessionCosts = value;
                RaisePropertyChanged("SessionCosts");
            }
        }

        /// <summary>
        /// Gets the OperatorId
        /// </summary>
        public int OperatorId
        {
            get
            {
                return Parent.OperatorId;
            }
        }

        #endregion
    }
}
