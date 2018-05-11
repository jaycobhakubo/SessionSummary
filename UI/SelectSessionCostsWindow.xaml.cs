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

namespace GameTech.Elite.Client.Modules.SessionSummary.UI
{
    /// <summary>
    /// Interaction logic for SessionCostsWindow
    /// </summary>
    internal partial class SelectSessionCostsWindow
    {
        #region Constructors
        public SelectSessionCostsWindow()
            : base(true)
        {
            InitializeComponent();
        }

        public SelectSessionCostsWindow(bool useAcceleration)
            : base(useAcceleration)
        {
            InitializeComponent();
        }

        #endregion

        private void CostsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
        }

        private void ChoosenCostsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
        }
    }
}
