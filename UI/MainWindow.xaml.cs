#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.ComponentModel;
using GameTech.Elite.Client.Modules.SessionSummary.Business;
using System.Globalization;
using GameTech.Elite.Reports;
using CrystalDecisions.ReportSource;
using CrystalDecisions.CrystalReports.Engine;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace GameTech.Elite.Client.Modules.SessionSummary.UI
{
    /// <summary>
    /// The main window of the application.
    /// </summary>
    internal partial class MainWindow
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// <remarks>This constructor is only intended to be used at design
        /// time.</remarks>
        public MainWindow()
            : base(true)
        {
            InitializeComponent();

            ParentController = new DesignSessionSummaryController(this);
        }

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// <param name="parent">The ISessionSummaryController instance that
        /// owns this window.</param>
        /// <param name="useAcceleration">true to use hardware accelerated
        /// rendering; otherwise false.</param>
        /// <exception cref="System.ArgumentNullException">parent is a null
        /// reference.</exception>
        public MainWindow(ISessionSummaryController parent, bool useAcceleration)
            : base(useAcceleration)
        {
            if(parent == null)
                throw new ArgumentNullException("parent");

            InitializeComponent();

            ParentController = parent;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Raises the Closing event. 
        /// </summary>
        /// <param name="e">An CancelEventArgs that contains the event
        /// data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Notify the controller we are closing.
            e.Cancel = !ParentController.Exit();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets this window's parent.
        /// </summary>
        private ISessionSummaryController ParentController
        {
            get;
            set;
        }
        #endregion
    }
}
