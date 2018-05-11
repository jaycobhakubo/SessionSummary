#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Business;
using GameTech.Elite.Client.Modules.SessionSummary.Properties;
using GameTech.Elite.UI;

namespace GameTech.Elite.Client.Modules.SessionSummary.ViewModel
{
    /// <summary>
    /// A view model class that handles creating and editing session costs.
    /// </summary>
    internal class SessionCostsViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Constants
        private const string CostNameProperty = "Name";
        #endregion

        #region Member Variables
        private ListCollectionView m_costs;
        private bool m_showInactive;
        private SessionCost m_selectedCost;
        private SessionCost m_pendingCost;
        private string m_editCostValue;
        private bool m_hasSuccess;
        private bool m_closing;
        #endregion

        #region Constructors
        /// <summary>
        /// Intializes and intance of SessionCostsViewModel.
        /// </summary>
        /// <param name="controller">The controller of the view model.</param>
        public SessionCostsViewModel(ISessionCostsController controller)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            Controller = controller;
            PopulateCosts(null);

            CostNewCommand = new RelayCommand((parameter) => NewCost(), (parameter) => !IsBusy);
            CostSaveCommand = new RelayCommand((parameter) => SaveCost(), (parameter) => !IsBusy && !HasError && IsDataModified);
            CostCancelCommand = new RelayCommand((parameter) => CancelCost(), (parameter) => !IsBusy && IsDataModified);
            CostDeactivateCommand = new RelayCommand((parameter) => DeactivateCost(), (parameter) => !IsBusy && SelectedCost != null);
            CloseCommand = new RelayCommand((parameter) => Close((CancelEventArgs)parameter));

            // Listen for changes to the controller.
            PropertyChangedEventManager.AddListener(Controller, this, string.Empty);
            Controller.RequestCostsCompleted += OnRequestCostsCompleted;
            Controller.RequestSaveCostCompleted += OnRequestSaveCostCompleted;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Handles when a property has changed on the parent or children.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A PropertyChangedEventArgs object that contains the
        /// event data.</param>
        protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBusy")
            {
                IsBusy = Controller.IsBusy;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Populates the view model with costs
        /// </summary>
        /// <param name="select">the cost to try to select.</param>
        private void PopulateCosts(SessionCost select)
        {
            List<SessionCost> list = new List<SessionCost>(Controller.SessionCosts);
            Costs = new ListCollectionView(list);
            Costs.SortDescriptions.Add(new SortDescription(CostNameProperty, ListSortDirection.Ascending));
            // Reset the filter
            ShowInactive = ShowInactive;

            SessionCost newSelect = null;
            if (select != null)
            {
                // Find the cost item again and select id
                foreach (SessionCost cost in Costs)
                {
                    if (cost.MasterId == select.MasterId)
                    {
                        newSelect = cost;
                        break;
                    }
                }
            }

            if (newSelect == null  && Costs.Count > 0)
                newSelect = (SessionCost)Costs.GetItemAt(0);

            SelectedCost = newSelect;
        }

        /// <summary>
        /// Updates information when a selection has been changed
        /// </summary>
        private void RaiseEditingCostChanges()
        {
            SessionCost cost = EditingCost;

            RaisePropertyChanged("EditingCost");
            RaisePropertyChanged("CostName");
            RaisePropertyChanged("CostCode");

            if (cost == null)
                CostValue = null;
            else
                CostValue = cost.Value.ToString("F2", CultureInfo.CurrentCulture);

            RaisePropertyChanged("CostRegister");
            RaisePropertyChanged("DeactivateText");
            IsDataModified = false;

            HasSuccess = false;
            ProgressMessage = Error;
        }

        /// <summary>
        /// Creates a new cost
        /// </summary>
        private void NewCost()
        {
            bool proceed = true;
            if (IsDataModified)
            {
                MessageBoxResult result = MessageBoxResult.Cancel;
                if (HasError)
                    result = Controller.ShowObjectModifiedMessage(Resources.ChangedCantSave, false);
                else
                    result = Controller.ShowObjectModifiedMessage(Resources.CostChanged, true);

                if (result == MessageBoxResult.Yes)
                    SaveCost();
                else if (result == MessageBoxResult.Cancel)
                    proceed = false;
            }

            if (proceed)
            {
                SelectedCost = null;
                EditingCost = new SessionCost();
                RaiseEditingCostChanges();
            }
        }

        /// <summary>
        /// Saves the cost
        /// </summary>
        private void SaveCost()
        {
            if (m_pendingCost == null)
                m_pendingCost = EditingCost;
            Controller.RequestSaveCost(EditingCost);
        }

        /// <summary>
        /// Cancels changes made to a cost
        /// </summary>
        private void CancelCost()
        {
            PopulateCosts(EditingCost);
        }

        /// <summary>
        /// Activates/Deactivate the cost
        /// </summary>
        private void DeactivateCost()
        {
            if (EditingCost != null)
            {
                EditingCost.IsInactive = !EditingCost.IsInactive;
                SaveCost();
            }
        }

        /// <summary>
        /// Closes the windows
        /// </summary>
        /// <param name="e">The cancel arguments.</param>
        private void Close(CancelEventArgs e)
        {
            if (IsDataModified)
            {
                MessageBoxResult result = MessageBoxResult.Cancel;
                if (HasError)
                    result = Controller.ShowObjectModifiedMessage(Resources.ChangedCantSave, false);
                else
                    result = Controller.ShowObjectModifiedMessage(Resources.CostChanged, true);

                if (result == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    m_closing = true;
                    SaveCost();
                }
                else if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles when a request cost event has completed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnRequestCostsCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // Refresh the costs completed
                PopulateCosts(m_pendingCost);
                m_pendingCost = null;
            }
        }

        /// <summary>
        /// Handles when a reques save cost event has completed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The even data.</param>
        private void OnRequestSaveCostCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IsDataModified = false;

                // Refresh the costs
                PopulateCosts(m_pendingCost);
                m_pendingCost = null;

                if (!m_closing)
                {
                    HasSuccess = true;
                    ProgressMessage = Resources.SaveSuccess;
                }
                else
                    Controller.CloseCurrentView();
            }
            else
            {
                HasSuccess = false;
                ProgressMessage = Error;
            }
        }

        /// <summary>
        /// Releases all resources used by SessionCostsViewModel.
        /// </summary>
        /// <param name="disposing">Whether this function is being called from 
        /// user code.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Set data modified false
                    IsDataModified = false;
                    SelectedCost = null;
                    Costs = null;

                    m_pendingCost = null;

                    PropertyChangedEventManager.RemoveListener(Controller, this, string.Empty);
                    Controller.RequestCostsCompleted -= OnRequestCostsCompleted;
                    Controller.RequestSaveCostCompleted -= OnRequestSaveCostCompleted;
                }

                base.Dispose(disposing);
            }
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the controller of the view model
        /// </summary>
        public ISessionCostsController Controller
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of costs
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
        /// Gets or sets the currently selected cost
        /// </summary>
        public SessionCost SelectedCost
        {
            get
            {
                return m_selectedCost;
            }
            set
            {
                bool proceed = true;
                if (IsDataModified)
                {
                    MessageBoxResult result = MessageBoxResult.Cancel;
                    if (HasError)
                        result = Controller.ShowObjectModifiedMessage(Resources.ChangedCantSave, false);
                    else
                        result = Controller.ShowObjectModifiedMessage(Resources.CostChanged, true);

                    if (result == MessageBoxResult.Yes)
                    {
                        m_pendingCost = value;
                        SaveCost();
                    }
                    else if (result == MessageBoxResult.Cancel)
                        proceed = false;
                }

                if (proceed)
                {
                    m_selectedCost = value;
                    EditingCost = (value == null) ? new SessionCost() : new SessionCost(value);
                    RaiseEditingCostChanges();
                    RaisePropertyChanged("SelectedCost");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the data has been modified
        /// </summary>
        private bool IsDataModified
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the editing cost
        /// </summary>
        private SessionCost EditingCost
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the is active filter
        /// </summary>
        public bool ShowInactive
        {
            get
            {
                return m_showInactive;
            }
            set
            {
                m_showInactive = value;
                RaisePropertyChanged("ShowInactive");

                if (m_showInactive)
                    Costs.Filter = null;
                else
                    Costs.Filter = (parameter) => !((SessionCost)parameter).IsInactive;
            }
        }

        /// <summary>
        /// Gets or sets the name of the cost
        /// </summary>
        public string CostName
        {
            get
            {
                if (EditingCost != null)
                    return EditingCost.Name;

                return null;
            }
            set
            {
                if (EditingCost != null && EditingCost.Name != value)
                {
                    EditingCost.Name = value;
                    IsDataModified = true;
                    RaisePropertyChanged("CostName");

                    // Check for errors
                    RaisePropertyChanged("HasError");
                    ProgressMessage = Error;
                }
            }
        }

        /// <summary>
        /// Gets or sets the accounting code of the cost
        /// </summary>
        public string CostCode
        {
            get
            {
                if (EditingCost != null)
                    return EditingCost.Code;

                return null;
            }
            set
            {
                if (EditingCost != null && EditingCost.Code != value)
                {
                    EditingCost.Code = value;
                    IsDataModified = true;
                    RaisePropertyChanged("CostCode");
                }
            }
        }

        /// <summary>
        /// Gets or set the value of the selected cost.
        /// </summary>
        public string CostValue
        {
            get
            {
                return m_editCostValue;
            }
            set
            {
                if (m_editCostValue != value)
                {
                    m_editCostValue = value;
                    decimal result = 0M;

                    if (!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    if (EditingCost != null)
                        EditingCost.Value = result;

                    IsDataModified = true;
                    RaisePropertyChanged("CostValue");

                    // Check for errors
                    RaisePropertyChanged("HasError");
                    ProgressMessage = Error;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the cost is register or not
        /// </summary>
        public bool CostRegister
        {
            get
            {
                if (EditingCost != null)
                    return EditingCost.IsRegister;

                return false;
            }
            set
            {
                if (EditingCost != null && EditingCost.IsRegister != value)
                {
                    EditingCost.IsRegister = value;
                    IsDataModified = true;
                    RaisePropertyChanged("CostRegister");
                }
            }
        }

        /// <summary>
        /// Gets the text to display for the deactivate command.
        /// </summary>
        public string DeactivateText
        {
            get
            {
                if (EditingCost != null && EditingCost.IsInactive)
                    return Resources.Activate;
                else
                    return Resources.Deactivate;
            }
        }

        #region ICommand Member Properties
        /// <summary>
        /// Gets the command to create a new cost
        /// </summary>
        public ICommand CostNewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to save the cost
        /// </summary>
        public ICommand CostSaveCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to cancel the cost changes
        /// </summary>
        public ICommand CostCancelCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to activate/deactivate the cost
        /// </summary>
        public ICommand CostDeactivateCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to close the window
        /// </summary>
        public ICommand CloseCommand
        {
            get;
            private set;
        }
        #endregion

        /// <summary>
        /// Gets whether the view can close.
        /// </summary>
        public bool CanClose
        {
            get
            {
                return !IsBusy;
            }
        }

        /// <summary>
        /// Gets or sets whether the last operation was successful.
        /// </summary>
        public bool HasSuccess
        {
            get
            {
                if (!HasError && !IsDataModified)
                    return m_hasSuccess;

                return false;
            }
            private set
            {
                if (m_hasSuccess != value)
                {
                    m_hasSuccess = value;
                    RaisePropertyChanged("HasSuccess");
                }
            }
        }

        /// <summary>
        /// Gets whether there is an error in the data.
        /// </summary>
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(Error);
            }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error
        {
            get
            {
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
                {
                    string propertyError = this[prop.Name];
                    if (!string.IsNullOrEmpty(propertyError))
                        return propertyError;
                }

                return null;       
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property whose error
        /// message to get.</param>
        /// <returns>The error message for the property with the given
        /// name.</returns>
        public string this[string propertyName]
        {
            get
            {
                string error = null;

                if (propertyName == "CostName")
                {
                    if (string.IsNullOrWhiteSpace(CostName))
                        error = Resources.InvalidCostName;
                }
                else if (propertyName == "CostValue")
                {
                    if (EditingCost != null && EditingCost.Value == decimal.MinValue)
                        error = Resources.InvalidCostValue;
                }

                return error;
            }
        }

        #endregion
    }
}
