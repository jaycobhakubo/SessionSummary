#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// The interface representing the session costs controller controller.
    /// </summary>
    internal interface ISessionCostsController : INotifyPropertyChanged
    {
        #region Member Events
        /// <summary>
        /// Occurs when the controller retreives a list of availables session costs.
        /// </summary>
        event AsyncCompletedEventHandler RequestCostsCompleted;

        /// <summary>
        /// Occurs when the controller completes a request to save a session cost.
        /// </summary>
        event AsyncCompletedEventHandler RequestSaveCostCompleted;

        #endregion

        #region Member Methods
        /// <summary>
        /// Requests a list of session costs.
        /// </summary>
        void RequestCosts();

        /// <summary>
        /// Request that a session cost be saved.
        /// </summary>
        /// <param name="cost">The cost to be saved.</param>
        void RequestSaveCost(SessionCost cost);

        /// <summary>
        /// Request that a session cost be deleted.
        /// </summary>
        /// <param name="cost">The cost to be deleted.</param>
        //void RequestDeleteCost(SessionCost cost);

        /// <summary>
        /// Closes the current view.
        /// </summary>
        void CloseCurrentView();

        /// <summary>
        /// Shows a message asking if the user wants to save, not save, or
        /// cancel.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="canSave">Whether the user is allowed to save or
        /// not.</param>
        /// <returns>The result of the message window.</returns>
        MessageBoxResult ShowObjectModifiedMessage(string message, bool canSave);

        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the operator id
        /// </summary>
        int OperatorId
        {
            get;
        }

        /// <summary>
        /// Gets whether the controller is busy.
        /// </summary>
        bool IsBusy
        {
            get;
        }

        /// <summary>
        /// Gets a list of session costs.
        /// </summary>
        IEnumerable<SessionCost> SessionCosts
        {
            get;
        }

        #endregion
    }
}
