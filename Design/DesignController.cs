#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using CrystalDecisions.CrystalReports.Engine;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.UI;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Represents a specific user interface in Session Summary.
    /// </summary>
    public enum SessionSummaryView
    {
        Main,
        About,
        SessionCosts,
        SelectSessionCosts,
        Report,
        ActualCashCalculator,
        SessionBankBreakdown,
    }

    /// <summary>
    /// The interface representing the application controller.
    /// </summary>
    internal interface ISessionSummaryController : INotifyPropertyChanged
    {
        #region Member Methods
        /// <summary>
        /// Displays the specified view.
        /// </summary>
        /// <param name="view">The view to display.</param>
        /// <exception cref="System.ArgumentException">view is
        /// invalid.</exception>
        void NavigateTo(SessionSummaryView view, NavigateToArgs e);

        /// <summary>
        /// Closes the current view.
        /// </summary>
        void CloseCurrentView();

        /// <summary>
        /// Displays a window that tells the user the application is closing
        /// because of a server comm. failure.
        /// </summary>
        void ServerCommFailure();

        /// <summary>
        /// Starts the process of exiting the application.
        /// </summary>
        void StartExit();

        /// <summary>
        /// Exits the application (if the program isn't busy).
        /// </summary>
        /// <returns>true if the application is exiting; otherwise
        /// false.</returns>
        bool Exit();

        /// <summary>
        /// Sets the loading message (if the program is still loading)
        /// </summary>
        /// <param name="msg">The message to set the loading form to</param>
        void LoadingMessage(string msg);

        #endregion

        #region Member Properties

        /// <summary>
        /// Gets whether the controller is performing a long running operation.
        /// </summary>
        bool IsBusy
        {
            get;
        }

        /// <summary>
        /// Gets the operator id of this module.
        /// </summary>
        int OperatorId
        {
            get;
        }

        /// <summary>
        /// Gets a list of all staff members
        /// </summary>
        IEnumerable<Staff> StaffMembers
        {
            get;
        }

        /// <summary>
        /// Gets the Id of the staff member currently logged in
        /// </summary>
        Staff StaffMember
        {
            get;
        }

        /// <summary>
        /// Gets the application's main window.
        /// </summary>
        Window MainWindow
        {
            get;
        }

        /// <summary>
        /// Gets the current, active view.
        /// </summary>
        Window CurrentView
        {
            get;
        }

        /// <summary>
        /// Gets Session Summary's settings.
        /// </summary>
        SessionSummarySettings Settings
        {
            get;
        }

        /// <summary>
        /// Gets the summary child controller.
        /// </summary>
        ISummaryController SummaryController
        {
            get;
        }

        /// <summary>
        /// Gets the currencies in the system.
        /// </summary>
        IEnumerable<Currency> Currencies
        {
            get;
        }

        /// <summary>
        /// The current active cash method for this user.
        /// </summary>
        CashMethod CashMethod { get; }

        #endregion
    }

    /// <summary>
    /// Class for passing arguments for navigation calls
    /// </summary>
    internal class NavigateToArgs
    {
        #region Constructors
        /// <summary>
        /// Initializes a NavigateToArgs object
        /// </summary>
        public NavigateToArgs()
        {
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// The argument used in the navigation.
        /// </summary>
        public object Argument
        {
            get;
            set;
        }
        #endregion
    }

    /// <summary>
    /// Represents the SessionSummary application running at design time.
    /// </summary>
    internal sealed class DesignSessionSummaryController : ISessionSummaryController
    {
        #region Events
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the DesignSessionSummaryController
        /// class.
        /// </summary>
        /// <param name="window"></param>
        public DesignSessionSummaryController(MainWindow window)
        {
            MainWindow = window;
            CurrentView = MainWindow;

            Currencies = new List<Currency>();
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Notifies any listeners that a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that has
        /// changed.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Displays the specified view.
        /// </summary>
        /// <param name="view">The view to display.</param>
        /// <exception cref="System.ArgumentException">view is
        /// invalid.</exception>
        public void NavigateTo(SessionSummaryView view, NavigateToArgs e)
        {
        }

        /// <summary>
        /// Closes the current view.
        /// </summary>
        public void CloseCurrentView()
        {
        }

        /// <summary>
        /// Displays a window that tells the user the application is closing
        /// because of a server comm. failure.
        /// </summary>
        public void ServerCommFailure()
        {
        }

        /// <summary>
        /// Starts the process of exiting the application.
        /// </summary>
        public void StartExit()
        {
            if(MainWindow != null)
                MainWindow.Close();
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        /// <returns>true if the application is exiting; otherwise
        /// false.</returns>
        public bool Exit()
        {
            return true;
        }

        /// <summary>
        /// Sets the loading message (if the program is still loading)
        /// </summary>
        /// <param name="msg">The message to set the loading form to</param>
        public void LoadingMessage(string msg)
        {
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets whether the controller is performing a long running operation.
        /// </summary>
        public bool IsBusy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operator Id
        /// </summary>
        public int OperatorId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a list of all staff members
        /// </summary>
        public IEnumerable<Staff> StaffMembers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Id of the staff member currently logged in
        /// </summary>
        public Staff StaffMember
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the session summary report
        /// </summary>
        public ReportDocument SessionSummaryReport
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the application's main window.
        /// </summary>
        public Window MainWindow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current, active view.
        /// </summary>
        public Window CurrentView
        {
            get;
            set;
        }

        /// <summary>
        /// Gets Session Summary's settings.
        /// </summary>
        public SessionSummarySettings Settings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the summary child controller.
        /// </summary>
        public ISummaryController SummaryController
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the currencies in the system.
        /// </summary>
        public IEnumerable<Currency> Currencies
        {
            get;
            set;
        }

        /// <summary>
        /// The current active cash method for this user.
        /// </summary>
        public CashMethod CashMethod { get; set; }

        #endregion
    }
}