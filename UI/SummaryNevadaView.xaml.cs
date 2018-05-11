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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace GameTech.Elite.Client.Modules.SessionSummary.UI
{
    /// <summary>
    /// Interaction logic for SummaryNevadaView.xaml
    /// </summary>
    public partial class SummaryNevadaView : UserControl
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SummaryNevadaView class.
        /// </summary>
        public SummaryNevadaView()
        {
            InitializeComponent();

            // Don't let the manual increase calendar pick dates in the future.
            m_gameDate.DisplayDateEnd = DateTime.Today;
            m_gameDate.BlackoutDates.Add(new CalendarDateRange(DateTime.Today.AddDays(1), DateTime.MaxValue));
        }
        #endregion
    }
}
