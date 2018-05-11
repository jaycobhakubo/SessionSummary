#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Business;
using GameTech.Elite.UI;

namespace GameTech.Elite.Client.Modules.SessionSummary.ViewModel
{
    /// <summary>
    /// View Model class for selecting session costs
    /// </summary>
    internal class SelectSessionCostsViewModel : ViewModelBase
    {
        #region Constants
        private const string CostNameProperty = "Name";
        #endregion

        #region Member Variables
        private ListCollectionView m_costs;
        private ListCollectionView m_choosenCosts;
        private string m_costsRegister;
        private string m_costsNonRegister;
        private string m_costsTotal;
        #endregion

        #region Contructors
        /// <summary>
        /// Intializes an instance of a SessionCostsViewModel
        /// </summary>
        /// <param name="controller">The view models controller.</param>
        public SelectSessionCostsViewModel(ISessionCostsController controller, Summary summary)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");
            else if (summary == null)
                throw new ArgumentNullException("summary");

            Controller = controller;
            Summary = summary;

            CostIncludeCommand = new RelayCommand((parameter) => OnCostInclude(), (parameter) => !IsBusy && HasSelectedCost());
            CostExcludeCommand = new RelayCommand((parameter) => OnCostExclude(), (parameter) => !IsBusy && HasSelectedChoosenCost());
            CostSaveCommand = new RelayCommand((parameter) => OnCostSave(), (parameter) => !IsBusy && IsDataModified);
            CostClearCommand = new RelayCommand((parameter) => OnCostClear(), (parameter) => !IsBusy && ChoosenCosts != null && ChoosenCosts.Count > 0);
            CostCancelCommand = new RelayCommand((parameter) => OnCostCancel(), (parameter) => !IsBusy);
            CostCloseCommand = new RelayCommand((parameter) => OnCostClose((CancelEventArgs)parameter));

            PopulateCosts();

        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Populates the costs lists with various costs
        /// </summary>
        private void PopulateCosts()
        {
            List<SessionCostSelectionViewModel> list = new List<SessionCostSelectionViewModel>();
            foreach (SessionCost cost in Controller.SessionCosts)
            {
                bool doAdd = true;
                foreach (SessionCost compareCost in Summary.SessionCosts)
                {
                    if (cost.Id == compareCost.Id)
                    {
                        // the cost will already by choosen
                        doAdd = false;
                        break;
                    }
                }

                if (doAdd)
                    list.Add(new SessionCostSelectionViewModel(cost));
            }
            Costs = new ListCollectionView(list);
            Costs.SortDescriptions.Add(new SortDescription(CostNameProperty, ListSortDirection.Ascending));
            Costs.Filter = (parameter) => !((SessionCostSelectionViewModel)parameter).Cost.IsInactive;

            list = new List<SessionCostSelectionViewModel>();
            foreach (SessionCost cost in Summary.SessionCosts)
            {
                list.Add(new SessionCostSelectionViewModel(cost));
            }
            ChoosenCosts = new ListCollectionView(list);
            ChoosenCosts.SortDescriptions.Add(new SortDescription(CostNameProperty, ListSortDirection.Ascending));

            CostsModified();
        }

        /// <summary>
        /// Updates costs total when costs have been modified
        /// </summary>
        private void CostsModified()
        {
            decimal register = 0M;
            decimal nonregister = 0M;
            decimal total = 0M;

            foreach (SessionCostSelectionViewModel costVM in ChoosenCosts)
            {
                if (costVM.Cost.IsRegister)
                    register += costVM.Cost.Value;
                else
                    nonregister += costVM.Cost.Value;
            }

            total = register + nonregister;

            CostsRegister = register.ToString("F2", CultureInfo.CurrentCulture);
            CostsNonRegister = nonregister.ToString("F2", CultureInfo.CurrentCulture);
            CostsTotal = total.ToString("F2", CultureInfo.CurrentCulture);
        }
        
        /// <summary>
        /// Adds the selected costs to the current costs
        /// </summary>
        private void OnCostInclude()
        {
            IEnumerable<SessionCostSelectionViewModel> costs = GetSelectedCosts();
            foreach (SessionCostSelectionViewModel costVM in costs)
            {
                ChoosenCosts.AddNewItem(costVM);
                Costs.Remove(costVM);
                costVM.IsSelected = false;
            }
            ChoosenCosts.CommitNew();

            CostsModified();
            IsDataModified = true;
        }

        /// <summary>
        /// Removes the selected costs from the choosen costs
        /// </summary>
        private void OnCostExclude()
        {
            IEnumerable<SessionCostSelectionViewModel> costs = GetSelectedChoosenCosts();
            foreach (SessionCostSelectionViewModel costVM in costs)
            {
                Costs.AddNewItem(costVM);
                ChoosenCosts.Remove(costVM);
                costVM.IsSelected = false;
            }
            Costs.CommitNew();

            CostsModified();
            IsDataModified = true;
        }

        /// <summary>
        /// Removes all entries from the choosen costs
        /// </summary>
        private void OnCostClear()
        {
            List<SessionCostSelectionViewModel> list = new List<SessionCostSelectionViewModel>((IEnumerable<SessionCostSelectionViewModel>)ChoosenCosts.SourceCollection);
            foreach (SessionCostSelectionViewModel costVM in list)
            {
                ChoosenCosts.Remove(costVM);
                Costs.AddNewItem(costVM);
                costVM.IsSelected = false;
            }
            Costs.CommitNew();

            CostsModified();
            IsDataModified = true;
        }

        /// <summary>
        /// Save the cost and closes the window
        /// </summary>
        private void OnCostSave()
        {
            if (IsDataModified)
            {
                List<SessionCost> costList = new List<SessionCost>();
                foreach (SessionCostSelectionViewModel costVM in ChoosenCosts)
                {
                    SessionCost cost = (SessionCost)costVM.Cost.Clone();
                    costList.Add(cost);
                }

                Summary.SessionCosts = costList;
                IsDataModified = false;
            }

            Controller.CloseCurrentView();
        }

        /// <summary>
        /// Closes the view
        /// </summary>
        private void OnCostCancel()
        {
            Controller.CloseCurrentView();
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <param name="e">The cancel arguments.</param>
        private void OnCostClose(CancelEventArgs e)
        {
            // Allow the window to close.
        }

        /// <summary>
        /// Returns whether or not a cost in the list is selected
        /// </summary>
        /// <returns>true if selected.</returns>
        private bool HasSelectedCost()
        {
            foreach (SessionCostSelectionViewModel costVM in Costs)
            {
                if (costVM.IsSelected)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether or not a cost in the list is selected
        /// </summary>
        /// <returns>true if selected.</returns>
        private bool HasSelectedChoosenCost()
        {
            foreach (SessionCostSelectionViewModel costVM in ChoosenCosts)
            {
                if (costVM.IsSelected)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of the selected costs
        /// </summary>
        /// <returns>a list of the selected costs</returns>
        private IEnumerable<SessionCostSelectionViewModel> GetSelectedCosts()
        {
            List<SessionCostSelectionViewModel> selectedCosts = new List<SessionCostSelectionViewModel>();
            foreach (SessionCostSelectionViewModel costVM in Costs)
            {
                if (costVM.IsSelected)
                    selectedCosts.Add(costVM);
            }

            return selectedCosts;
        }

        /// <summary>
        /// Gets a list of the selected costs
        /// </summary>
        /// <returns>a list of the selected costs</returns>
        private IEnumerable<SessionCostSelectionViewModel> GetSelectedChoosenCosts()
        {
            List<SessionCostSelectionViewModel> selectedCosts = new List<SessionCostSelectionViewModel>();
            foreach (SessionCostSelectionViewModel costVM in ChoosenCosts)
            {
                if (costVM.IsSelected)
                    selectedCosts.Add(costVM);
            }

            return selectedCosts;
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets this view models controller
        /// </summary>
        public ISessionCostsController Controller
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets this view models summary controller
        /// </summary>
        public Summary Summary
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the data has been modified
        /// </summary>
        public bool IsDataModified
        {
            get;
            private set;
        }

        /// <summary>
        /// A list of all costs
        /// </summary>
        public ListCollectionView Costs
        {
            get
            {
                return m_costs;
            }
            private set
            {
                m_costs = value;
                RaisePropertyChanged("Costs");
            }
        }

        /// <summary>
        /// A list of the choosen costs
        /// </summary>
        //public ObservableCollection<SessionCost> ChoosenCosts
        public ListCollectionView ChoosenCosts
        {
            get
            {
                return m_choosenCosts;
            }
            private set
            {
                m_choosenCosts = value;
                RaisePropertyChanged("ChoosenCosts");
            }
        }

        /// <summary>
        /// Gets the register session costs
        /// </summary>
        public string CostsRegister
        {
            get
            {
                return m_costsRegister;
            }
            private set
            {
                if (m_costsRegister != value)
                {
                    m_costsRegister = value;
                    RaisePropertyChanged("CostsRegister");
                }
            }
        }

        /// <summary>
        /// Gets the non register session costs
        /// </summary>
        public string CostsNonRegister
        {
            get
            {
                return m_costsNonRegister;
            }
            private set
            {
                if (m_costsNonRegister != value)
                {
                    m_costsNonRegister = value;
                    RaisePropertyChanged("CostsNonRegister");
                }
            }
        }

        /// <summary>
        /// Gets the total session costs
        /// </summary>
        public string CostsTotal
        {
            get
            {
                return m_costsTotal;
            }
            private set
            {
                if (m_costsTotal != value)
                {
                    m_costsTotal = value;
                    RaisePropertyChanged("CostsTotal");
                }
            }
        }

        #region ICommand Member Properties
        /// <summary>
        /// Gets the command to include a session cost.
        /// </summary>
        public ICommand CostIncludeCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to exclude a session cost.
        /// </summary>
        public ICommand CostExcludeCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to clear session costs.
        /// </summary>
        public ICommand CostClearCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to save the costs.
        /// </summary>
        public ICommand CostSaveCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to cancel costs.
        /// </summary>
        public ICommand CostCancelCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to close the window.
        /// </summary>
        public ICommand CostCloseCommand
        {
            get;
            private set;
        }

        #endregion

        #endregion
    }
}
