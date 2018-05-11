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
using GameTech.Elite.UI;

namespace GameTech.Elite.Client.Modules.SessionSummary.UI
{
    /// <summary>
    /// Interaction logic for SessionBanksWindow.xaml
    /// </summary>
    public partial class SessionBanksWindow : EliteWindowBase
    {
        public SessionBanksWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        public SessionBanksWindow(bool useAcceleration)
            : base(useAcceleration)
        {
            InitializeComponent();
        }
    }
}
