using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CrystalDecisions.ReportSource;
using CrystalDecisions.CrystalReports.Engine;

namespace GameTech.Elite.Client.Modules.SessionSummary.UI
{
    /// <summary>
    /// Interaction logic for SessionSummaryReportWindow.xaml
    /// </summary>
    internal partial class SessionSummaryReportWindow
    {
        #region Constructors
        public SessionSummaryReportWindow()
            : base(true)
        {
            InitializeComponent();
        }

        public SessionSummaryReportWindow(bool useAcceleration)
            : base(useAcceleration)
        {
            InitializeComponent();
        }
        #endregion

        /// <summary>
        /// Set the reports to display and displays it.
        /// </summary>
        /// <param name="report">The report document to display.</param>
        public void DisplayReport(ReportDocument report)
        {
            m_viewer.ViewerCore.Owner = this; // crystal reports needs an owner window or it will crash when it goes to display a pop-up
            m_viewer.ViewerCore.ReportSource = report;
        }
    }
}
