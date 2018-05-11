#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2017 Fortunet, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameTech.Elite.Base;
using System.Collections.ObjectModel;
using GameTech.Elite.Client.Modules.SessionSummary.Business;
using System.ComponentModel;
using System.Windows.Input;
using GameTech.Elite.UI;
using GameTech.Elite.Base.System;

namespace GameTech.Elite.Client.Modules.SessionSummary.ViewModel
{
    public class SessionBanksViewModel : ViewModelBase
    {
        private ISessionSummaryController m_controller;

        public ObservableCollection<BasicBankInfo> Banks
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public ICommand ExitCommand
        {
            get;
            private set;
        }

        internal SessionBanksViewModel(ISessionSummaryController paramController, List<BasicBankInfo> banks)
        {
            m_controller = paramController;
            ExitCommand = new RelayCommand((param) => m_controller.CloseCurrentView());
            if (banks != null)
            {
                Banks = new ObservableCollection<BasicBankInfo>(banks);
            }
            else
            {
                Banks = new ObservableCollection<BasicBankInfo>();
            }
        }
    }
}
