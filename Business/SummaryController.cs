#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Bingo;
using GameTech.Elite.Client.Modules.SessionSummary.Messages;
using GameTech.Elite.Client.Modules.SessionSummary.Properties;
using GameTech.Elite.UI;
using GameTech.Elite.Base.Messages;
using GameTech.Elite.Base.System;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Represents control over session summaries
    /// </summary>
    internal class SummaryController : ControllerBase, ISummaryController
    {
        #region Member Variables
        private Dictionary<DateTime, IList<short>> m_sessionList;
        #endregion

        #region Member Events
        /// <summary>
        /// Occurs when the controller returns a list of sessions based on a
        /// particular date.
        /// </summary>
        public event EventHandler<RequestSessionsEventArgs> RequestSessionsCompleted;
        /// <summary>
        /// Occurs when the controller returns a summary for a session base on
        /// a date and session number
        /// </summary>
        public event EventHandler<RequestSummaryEventArgs> RequestSummaryCompleted;
        /// <summary>
        /// Occurs when the controller return for a request get for a calculated summary
        /// </summary>
        public event EventHandler<RequestCalcdSummaryEventArgs> RequestCalculatedSummaryCompleted;
        /// <summary>
        /// Occurs when the controller return for a request to apply pending acc2 increases
        /// </summary>
        public event EventHandler<TriggerAcc2IncreasesEventArgs> TriggerAcc2IncreasesCompleted;
        /// <summary>
        /// Occurs when the controller returns from a save session summary event
        /// </summary>
        public event AsyncCompletedEventHandler RequestSetSummaryCompleted;
        /// <summary>
        /// Occurs when the controller returns from a request to get the session banks
        /// </summary>
        public event EventHandler<RequestSessionBanksEventArgs> RequestSessionBanksCompleted;
        #endregion

        #region Constructors
        /// <summary>
        /// Intializes the an instance of a SummaryController class.
        /// </summary>
        /// <param name="parent">The parent session summary controller.</param>
        public SummaryController(ISessionSummaryController parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            Parent = parent;
            IsBusy = false;

            // Get a default session list
            m_sessionList = new Dictionary<DateTime, IList<short>>();
            Parent.LoadingMessage(Resources.LoadingSessions);
            GamingDate = EliteModule.GetGamingDate();
            SendRequestSessions(GamingDate);
        }
        #endregion
        
        #region Member Methods
        /// <summary>
        /// A request of the controller to retreive a session list base on the DateTime
        /// </summary>
        /// <param name="date">The date of the session list wanted</param>
        public void RequestSessions(DateTime date)
        {
            if (!IsBusy)
            {
                IsBusy = true;

                RunWorker(Resources.LoadingSessions,
                         DoRequestSessions,
                         date,
                         false,
                         OnRequestSessionsCompleted);
            }
        }

        /// <summary>
        /// Retrieves a list of sessions
        /// </summary>
        /// <param name="sender">The source of the request</param>
        /// <param name="e">The argument passed in</param>
        private void DoRequestSessions(object sender, DoWorkEventArgs e)
        {
            EliteModule.SetThreadCulture(Parent.Settings);

            DateTime date = (DateTime)e.Argument;
            e.Result = SendRequestSessions(date);
        }

        /// <summary>
        /// Retrieves a list of session
        /// </summary>
        /// <param name="date">The date of the session list wanted</param>
        /// <returns>the list of session on the requested date</returns>
        private IList<short> SendRequestSessions(DateTime date)
        {
            if (!m_sessionList.ContainsKey(date))
            {
                GetSessionNumberListMessage getSessionsMsg = new GetSessionNumberListMessage(date, date);
                getSessionsMsg.Send();

                if (getSessionsMsg.ReturnCode == ServerReturnCode.Success)
                {
                    List<short> list = new List<short>(getSessionsMsg.Sessions);
                    m_sessionList.Add(date, list);
                }
                else
                    throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(getSessionsMsg.ReturnCode));
            }

            return m_sessionList[date];
        }

        /// <summary>
        /// Handles when the request sessions background work is completed.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">A RunWorkerCompletedEentArgs object that contains the even data.</param>
        private void OnRequestSessionsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            //ProgressText = null;

            IList<short> sessions = null;

            if (!CheckForError(e.Error))
                sessions = (IList<short>)e.Result;

            EventHandler<RequestSessionsEventArgs> handler = RequestSessionsCompleted;
            if (handler != null)
                handler(this, new RequestSessionsEventArgs(sessions));
        }

        /// <summary>
        /// Requests a summary for the session on the specified date
        /// </summary>
        /// <param name="date">date of the session</param>
        /// <param name="session">session number to retrieve the summary for</param>
        public void RequestSummary(DateTime date, short session)
        {
            if (!IsBusy)
            {
                IsBusy = true;

                // Box the arguments.
                ArrayList args = new ArrayList();
                args.Add(date);
                args.Add(session);

                RunWorker(Resources.LoadingSummary,
                         DoRequestSummary,
                         args,
                         false,
                         OnRequestSummaryCompleted);
            }
        }

        /// <summary>
        /// Retrieves a summary
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The work argument passed in</param>
        private void DoRequestSummary(object sender, DoWorkEventArgs e)
        {
            EliteModule.SetThreadCulture(Parent.Settings);

            // Unbox the arguments.
            ArrayList args = (ArrayList)e.Argument;
            DateTime date = (DateTime)args[0];
            short session = (short)args[1];

            GetCalculatedSessionSummary msgCalcd = new GetCalculatedSessionSummary(date, session);
            msgCalcd.Send();

            if (msgCalcd.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msgCalcd.ReturnCode));

            GetSavedSessionSummary msgSaved = new GetSavedSessionSummary(date, session, StaffMembers);
            msgSaved.Send();

            if (msgSaved.ReturnCode == ServerReturnCode.MissingTableEntry)
            {
                //swallow this return code there will be not data
            }
            else if (msgSaved.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msgSaved.ReturnCode));

            ArrayList results = new ArrayList();
            results.Add(msgCalcd.Summary);
            results.Add(msgSaved.Summary);
            e.Result = results;
        }

        /// <summary>
        /// Handles when the request summary background worker has completed.
        /// </summary>
        /// <param name="sender">The source of this event.</param>
        /// <param name="e">A RunWorkerCompletedEventArgs object that contains the event data.</param>
        private void OnRequestSummaryCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            Summary calcd = null;
            Summary saved = null;

            if (!CheckForError(e.Error))
            {
                ArrayList results = (ArrayList)e.Result;
                calcd = (Summary)results[0];
                saved = (Summary)results[1];

                if (calcd != null)  calcd.LoadAvailableCurrencies(Parent.Currencies);
                if (saved != null)  saved.LoadAvailableCurrencies(Parent.Currencies);
            }

            EventHandler<RequestSummaryEventArgs> handler = RequestSummaryCompleted;
            if (handler != null)
                handler(this, new RequestSummaryEventArgs(calcd, saved));
        }

        /// <summary>
        /// Requests a system calculated summary for the session on the specified date
        /// </summary>
        /// <param name="date">date of the session</param>
        /// <param name="session">session number to retrieve the summary for</param>
        public void RequestCalcdSummary(DateTime date, short session)
        {
            if (!IsBusy)
            {
                IsBusy = true;

                // Box the arguments.
                ArrayList args = new ArrayList();
                args.Add(date);
                args.Add(session);

                RunWorker(Resources.LoadingSummary,
                         DoRequestCalcdSummary,
                         args,
                         false,
                         OnRequestCalcdSummaryCompleted);
            }
        }

        /// <summary>
        /// Retrieves a calculated summary.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The work argument passed in.</param>
        private void DoRequestCalcdSummary(object sender, DoWorkEventArgs e)
        {
            EliteModule.SetThreadCulture(Parent.Settings);

            // Unbox the arguments.
            ArrayList args = (ArrayList)e.Argument;
            DateTime date = (DateTime)args[0];
            short session = (short)args[1];

            GetCalculatedSessionSummary msgCalcd = new GetCalculatedSessionSummary(date, session);
            msgCalcd.Send();

            if (msgCalcd.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msgCalcd.ReturnCode));

            e.Result = msgCalcd.Summary;
        }

        /// <summary>
        /// Handles when the request calculated summary background worker has completed.
        /// </summary>
        /// <param name="sender">The source of this event.</param>
        /// <param name="e">A RunWorkerCompletedEventArgs object that contains the event data.</param>
        private void OnRequestCalcdSummaryCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            Summary summary = null;

            if (!CheckForError(e.Error))
            {
                summary = (Summary)e.Result;
                summary.LoadAvailableCurrencies(Parent.Currencies);
            }

            EventHandler<RequestCalcdSummaryEventArgs> handler = RequestCalculatedSummaryCompleted;
            if (handler != null)
                handler(this, new RequestCalcdSummaryEventArgs(summary));
        }
        
        /// <summary>
        /// Requests any pending increases for the session on the specified date to be applied.
        /// </summary>
        /// <param name="date">date of the session</param>
        /// <param name="session">session number to retrieve the summary for</param>
        public void TriggerAcc2Increases(DateTime date, short session)
        {
            if(!IsBusy)
            {
                IsBusy = true;

                // Box the arguments.
                ArrayList args = new ArrayList();
                args.Add(date);
                args.Add(session);

                RunWorker(Resources.LoadingSummary,
                         DoTriggerAcc2Increases,
                         args,
                         false,
                         OnTriggerAcc2IncreasesCompleted);
            }
        }

        /// <summary>
        /// Requests any pending increases for the session on the specified date to be applied.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The work argument passed in.</param>
        private void DoTriggerAcc2Increases(object sender, DoWorkEventArgs e)
        {
            EliteModule.SetThreadCulture(Parent.Settings);

            // Unbox the arguments.
            ArrayList args = (ArrayList)e.Argument;
            DateTime date = (DateTime)args[0];
            short session = (short)args[1];

            var spMsg = new GetSessionPlayedsMessage(date, date);
            spMsg.Send();
            if(spMsg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(spMsg.ReturnCode));

            var sps = spMsg.Sessions;
            Games.Bingo.Model.SessionPlayed theSP = null;
            foreach(var sp in sps)
                if(!sp.Overridden && sp.GamingSession == session)
                {
                    theSP = sp;
                    break;
                }

            if(theSP == null)
                throw new SessionSummaryException("Session identifier not found");

            var accMsg = new TriggerAcc2AutoIncreasesRequest(date, theSP.SessionPlayedID, false, true);
            accMsg.Send();

            if(accMsg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(accMsg.ReturnCode));

            e.Result = accMsg.IncreaseCount;
        }
        
        /// <summary>
        /// Handles when the background worker has completed for the request to apply pending accrual increases.
        /// </summary>
        /// <param name="sender">The source of this event.</param>
        /// <param name="e">A RunWorkerCompletedEventArgs object that contains the event data.</param>
        private void OnTriggerAcc2IncreasesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            int increaseCount = 0;
            if(!CheckForError(e.Error))
                increaseCount = (int)e.Result;

            var h = TriggerAcc2IncreasesCompleted;
            if(h != null)
                h(this, new TriggerAcc2IncreasesEventArgs(increaseCount));
        }

        /// <summary>
        /// Requests a session summary be saved
        /// </summary>
        /// <param name="summary">The session summary to save</param>
        public void RequestSetSummary(Summary summary)
        {
            if (!IsBusy)
            {
                IsBusy = true;

                RunWorker(Resources.SavingSummary, DoRequestSetSummary, summary, false, OnRequestSetSummaryCompleted);
            }
        }

        /// <summary>
        /// Saves the session summary
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The work argument passed in.</param>
        private void DoRequestSetSummary(object sender, DoWorkEventArgs e)
        {
            EliteModule.SetThreadCulture(Parent.Settings);

            SetSessionSummary msg = new SetSessionSummary((Summary)e.Argument);
            msg.Send();

            if (msg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msg.ReturnCode));
        }

        /// <summary>
        /// Handles whent the request set summary background worker has completed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A RunWorkerCompletedEventArgs objec that contains the event data.</param>
        private void OnRequestSetSummaryCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            CheckForError(e.Error);

            AsyncCompletedEventHandler handler = RequestSetSummaryCompleted;
            if (handler != null)
                handler(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, null));
        }

        /// <summary>
        /// Retreives a list of staff
        /// </summary>
        private IEnumerable<Staff> SendRequestStaff()
        {
            GetStaffDataMessage msg = new GetStaffDataMessage(0);
            msg.Send();

            if (msg.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msg.ReturnCode));

            if (msg.Staff == null)
                return new List<Staff>();
            else
                return msg.Staff;
        }

        /// <summary>
        /// Checks for an error after a message is called
        /// </summary>
        /// <param name="ex">The exception to check or null.</param>
        /// <returns>true if there is an error false otherwise</returns>
        protected override bool CheckForError(Exception ex)
        {
            if(ex == null)
                return false;
            else
            {
                if(ex is ServerCommException)
                    Parent.ServerCommFailure();
                else
                {
                    if(Parent.CurrentView != null)
                        MessageWindow.Show(Parent.CurrentView, ex.Message, Resources.SessionSummaryName, MessageWindowType.Close);
                    else
                        MessageWindow.Show(ex.Message, Resources.SessionSummaryName, MessageWindowType.Close);
                }

                return true;
            }
        }

        /// <summary>
        /// Navigates to the SelectSessionCosts UI
        /// </summary>
        public void SelectSessionCosts(Summary summary)
        {
            NavigateToArgs args = new NavigateToArgs();
            args.Argument = summary;
            Parent.NavigateTo(SessionSummaryView.SelectSessionCosts, args);
        }

        /// <summary>
        /// Calculate the actual cash received.
        /// </summary>
        /// <param name="m_editSummary"></param>
        public void CalculateActualCash(Summary summary)
        {
            NavigateToArgs args = new NavigateToArgs();
            args.Argument = summary;
            Parent.NavigateTo(SessionSummaryView.ActualCashCalculator, args);
        }

        /// <summary>
        /// Displays the sent in session banks in a new UI
        /// </summary>
        /// <param name="banks"></param>
        public void DisplaySessionBanks(List<BasicBankInfo> banks)
        {
            NavigateToArgs args = new NavigateToArgs();
            args.Argument = banks;
            Parent.NavigateTo(SessionSummaryView.SessionBankBreakdown, args);
        }

        /// <summary>
        /// Causes a session summary report to be generated and displayed
        /// </summary>
        /// <param name="date">The date of the session for the report.</param>
        /// <param name="session">The session number of the session for the report.</param>
        public void GenerateReport(DateTime date, short session)
        {
            NavigateToArgs args = new NavigateToArgs();
            ArrayList list = new ArrayList();
            list.Add(date);
            list.Add(session);
            args.Argument = list;
            Parent.NavigateTo(SessionSummaryView.Report, args);
        }

        /// US5110
        /// <summary>
        /// Requests the list of banks for the specified date and session
        /// </summary>
        /// <param name="date">date of the session</param>
        /// <param name="session">session number to retrieve the summary for</param>
        public void RequestSessionBanks(DateTime date, short session)
        {
            //if (!IsBusy)
            {
                //IsBusy = true;

                // Box the arguments.
                ArrayList args = new ArrayList();
                args.Add(date);
                args.Add(session);

                RunWorker(Resources.LoadingBanks,
                         DoRequestSessionBanks,
                         args,
                         false,
                         OnRequestSessionBanksCompleted);
            }
        }

        /// <summary>
        /// Retrieves a summary
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The work argument passed in</param>
        private void DoRequestSessionBanks(object sender, DoWorkEventArgs e)
        {
            EliteModule.SetThreadCulture(Parent.Settings);

            // Unbox the arguments.
            ArrayList args = (ArrayList)e.Argument;
            DateTime date = (DateTime)args[0];
            short session = (short)args[1];

            GetSessionBanks msgCalcd = new GetSessionBanks(date, session);
            msgCalcd.Send();

            if (msgCalcd.ReturnCode != ServerReturnCode.Success)
                throw new SessionSummaryException(ServerErrorTranslator.GetReturnCodeMessage(msgCalcd.ReturnCode));

            ArrayList results = new ArrayList();
            results.Add(msgCalcd.Banks);
            e.Result = results;
        }

        /// <summary>
        /// Handles when the request summary background worker has completed.
        /// </summary>
        /// <param name="sender">The source of this event.</param>
        /// <param name="e">A RunWorkerCompletedEventArgs object that contains the event data.</param>
        private void OnRequestSessionBanksCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //IsBusy = false;
            List<BasicBankInfo> banks = null;
            
            if (!CheckForError(e.Error))
            {
                ArrayList results = (ArrayList)e.Result;
                banks = (List<BasicBankInfo>)results[0];
            }

            EventHandler<RequestSessionBanksEventArgs> handler = RequestSessionBanksCompleted;
            if (handler != null)
                handler(this, new RequestSessionBanksEventArgs(banks));
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the controllers parent controller
        /// </summary>
        public ISessionSummaryController Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the current gaming date time
        /// </summary>
        public DateTime GamingDate
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of staff.
        /// </summary>
        public IEnumerable<Staff> StaffMembers
        {
            get
            {
                return Parent.StaffMembers;
            }
        }

        /// <summary>
        /// Gets the currently logged in staff member
        /// </summary>
        public Staff StaffMember
        {
            get
            {
                return Parent.StaffMember;
            }
        }

        #endregion
    }
}
