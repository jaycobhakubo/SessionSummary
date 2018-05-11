#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2016 Fortunet, Inc.
#endregion

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
    /// Interaction logic for ActualCashCalculatorWindow.xaml
    /// </summary>
    internal partial class ActualCashCalculatorWindow
    {
        #region Constructors

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        public ActualCashCalculatorWindow()
            : base(true)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        public ActualCashCalculatorWindow(bool useAcceleration)
            : base(useAcceleration)
        {
            InitializeComponent();
        }

        #endregion
    }
}
