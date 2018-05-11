#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System.ComponentModel;
using System.Windows.Controls;
using System;

namespace GameTech.Elite.Client.Modules.SessionSummary.UI
{
    /// <summary>
    /// Interaction logic for SummaryView.xaml
    /// </summary>
    internal partial class SummaryView : UserControl
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SummaryView class.
        /// </summary>
        public SummaryView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Width = double.NaN;
                Height = double.NaN;
            }

            // Don't let the manual increase calendar pick dates in the future.
            m_gameDate.DisplayDateEnd = DateTime.Today;
            m_gameDate.BlackoutDates.Add(new CalendarDateRange(DateTime.Today.AddDays(1), DateTime.MaxValue));
        }
        #endregion
    }
}
