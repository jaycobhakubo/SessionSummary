#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2016 Fortunet, Inc.
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

namespace GameTech.Elite.Client.Modules.SessionSummary.ViewModel
{
    /// <summary>
    /// View Model for the "Actual Cash" calculator
    /// </summary>
    internal class ActualCashCalculatorViewModel : ViewModelBase
    {
        #region Fields
        private ISessionSummaryController m_controller;
        #endregion

        #region Properties

        public Summary Summary { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of this view model.
        /// </summary>
        /// <param name="paramController"></param>
        /// <param name="paramSummary"></param>
        internal ActualCashCalculatorViewModel(ISessionSummaryController paramController
            , Summary paramSummary)
        {
            this.m_controller = paramController;
            this.Summary = paramSummary;

            List<Currency> currentCurrencies = new List<Currency>(Summary.AvailableCurrencies);
            currentCurrencies.Sort((x, y) => { return y.IsDefault.CompareTo(x.IsDefault); });

            DisplayCurrencies = new ObservableCollection<ActualCashCurrencyViewModel>();
            foreach (var currency in currentCurrencies)
            {
                var actualCashCurrency = new ActualCashCurrencyViewModel(this, currency);
                actualCashCurrency.PropertyChanged += new PropertyChangedEventHandler(actualCashCurrency_PropertyChanged);
                DisplayCurrencies.Add(actualCashCurrency);

                if (currency.IsDefault) DefaultCurrency = actualCashCurrency;
            }

            ClearCommand = new RelayCommand((parameter) => OnClear());
            SaveCommand = new RelayCommand((param) => m_controller.CloseCurrentView());
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to a a caller to the choosen caller list
        /// </summary>
        public ICommand ClearCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to a a caller to the choosen caller list
        /// </summary>
        public ICommand SaveCommand
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Our list of display currencies
        /// </summary>
        public ObservableCollection<ActualCashCurrencyViewModel> DisplayCurrencies { get; private set; }

        /// <summary>
        /// The currently selected currency on our UI
        /// </summary>
        public ActualCashCurrencyViewModel SelectedCurrency
        {
            get
            {
                if (_selectedCurrency == null && DisplayCurrencies.Count > 0)
                {
                    _selectedCurrency = DisplayCurrencies[0];
                }

                return _selectedCurrency;
            }
            set
            {
                if (_selectedCurrency != value)
                {
                    _selectedCurrency = value;
                    RaisePropertyChanged("SelectedCurrency");
                }
            }
        }
        private ActualCashCurrencyViewModel _selectedCurrency;

        /// <summary>
        /// Default currency view model
        /// </summary>
        public ActualCashCurrencyViewModel DefaultCurrency
        {
            get { return _defaultCurrency; }
            set
            {
                if (_defaultCurrency != value)
                {
                    _defaultCurrency = value;
                    RaisePropertyChanged("DefaultCurrency");
                }
            }
        }
        private ActualCashCurrencyViewModel _defaultCurrency;

        /// <summary>
        /// Are we in "Multi Currency" mode?
        /// </summary>
        public bool IsMultiCurrency
        {
            get { return DisplayCurrencies.Count > 1; }
        }

        /// <summary>
        /// Our total value
        /// </summary>
        public decimal TotalValue
        {
            get
            {
                decimal total = 0.0M;

                foreach (var currency in DisplayCurrencies)
                {
                    total += currency.TotalValueInCurrency;
                }

                return total;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the pushing of the "clear" button.
        /// </summary>
        private void OnClear()
        {
            foreach (var item in this.DisplayCurrencies)
            {
                foreach (var item2 in item.DisplayDenominations)
                {
                    item2.Quantity = "0";
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event called when the currency property has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void actualCashCurrency_PropertyChanged(object sender
            , PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TotalValueInCurrency":
                    {
                        RaisePropertyChanged("TotalValue");
                    }
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// View model for actual cash currency objects.
    /// </summary>
    internal class ActualCashCurrencyViewModel : ViewModelBase
    {
        #region Fields
        private Currency m_currency;
        #endregion

        #region Properties

        public ActualCashCalculatorViewModel Parent { get; private set; }

        /// <summary>
        /// Our list of display currencies
        /// </summary>
        public ObservableCollection<ActualCashDenominationViewModel> DisplayDenominations { get; private set; }

        /// <summary>
        /// Gets the English name of the currency.
        /// </summary>
        public string Name
        {
            get
            {
                return m_currency.Name;
            }
        }

        /// <summary>
        /// Gets the three-character ISO 4217 currency code.
        /// </summary>
        public string ISOCode
        {
            get
            {
                return m_currency.ISOCode;
            }
        }

        /// <summary>
        /// Gets or sets this currency's exchange rate versus the system's
        /// default currency.
        /// </summary>
        public decimal ExchangeRate
        {
            get
            {
                return m_currency.ExchangeRate;
            }
        }

        /// <summary>
        /// Returns the total value in the currency iteself without exchange
        /// </summary>
        public decimal TotalValue
        {
            get
            {
                decimal total = 0.0M;

                foreach (var denom in DisplayDenominations)
                {
                    total += denom.CurrencyTotal;
                }

                return total;
            }
        }

        /// <summary>
        /// total value for this currency
        /// </summary>
        public decimal TotalValueInCurrency
        {
            get
            {
                decimal total = 0.0M;

                foreach (var denom in DisplayDenominations)
                {
                    total += denom.ExchangedTotal;
                }

                return total;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        /// <param name="actualCashCalculatorViewModel"></param>
        /// <param name="currency"></param>
        public ActualCashCurrencyViewModel(ActualCashCalculatorViewModel parent
            , Currency currency)
        {
            Parent = parent;
            m_currency = currency;

            DisplayDenominations = new ObservableCollection<ActualCashDenominationViewModel>();
            foreach (var denom in currency.Denominations.OrderBy(d => d.Order).ThenBy(d => d.Value).ThenBy(d => d.Name))
            {
                var newDenom = new ActualCashDenominationViewModel(this, denom);
                newDenom.PropertyChanged += new PropertyChangedEventHandler(newDenom_PropertyChanged);
                DisplayDenominations.Add(newDenom);
            }
        }

        #endregion

        #region Methods

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event Handler called for denomination changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void newDenom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ExchangedTotal":
                    {
                        RaisePropertyChanged("TotalValueInCurrency");
                        RaisePropertyChanged("TotalValue");
                    }
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// View model for a cash denomination
    /// </summary>
    internal class ActualCashDenominationViewModel : ViewModelBase
    {
        #region Fields
        private Denomination m_denom;
        #endregion

        #region Properties

        public ActualCashCurrencyViewModel Parent { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return m_denom.Name;
            }
        }

        #endregion

        /// <summary>
        /// Gets or Sets the quantity of the item
        /// </summary>
        public string Quantity
        {
            get
            {
                return _quantity.ToString();
            }
            set
            {
                if (_quantity.ToString() != value)
                {
                    int intVal;
                    if (Int32.TryParse(value, out intVal) &&
                        intVal >= 0)
                    {
                        _quantity = intVal;
                    }
                    else
                    {
                        _quantity = 0;
                    }

                    UpdateSummary(Parent.Parent.Summary);

                    RaisePropertyChanged("Quantity");
                    RaisePropertyChanged("CurrencyValue");
                    RaisePropertyChanged("CurrencyTotal");
                    RaisePropertyChanged("ExchangedTotal");
                }
            }
        }
        private int _quantity;

        /// <summary>
        /// Gets or sets the value of the denomination in relation to the 
        /// currency's base denomination.
        /// </summary>
        public decimal Value
        {
            get
            {
                return m_denom.Value;
            }
        }

        /// <summary>
        /// Gets the currency local total (Value * quantity)
        /// </summary>
        public decimal CurrencyTotal
        {
            get { return Value * _quantity; }
        }

        /// <summary>
        /// Gets the exchanged total value
        /// </summary>
        public decimal ExchangedTotal
        {
            get { return CurrencyTotal * Parent.ExchangeRate; }
        }

        #region Constructors

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        /// <param name="actualCashCalculatorViewModel"></param>
        /// <param name="currency"></param>
        public ActualCashDenominationViewModel(ActualCashCurrencyViewModel parent
            , Denomination denom)
        {
            Parent = parent;
            m_denom = denom;

            LoadFromSummary(Parent.Parent.Summary);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads our object from the data in the summary
        /// </summary>
        /// <param name="summary"></param>
        private void LoadFromSummary(Summary summary)
        {
            foreach (var item in summary.ActualCashDenoms)
            {
                if (item.CurrencyDetailID == m_denom.Id)
                {
                    _quantity = item.Quantity;
                }
            }
        }

        /// <summary>
        /// Saves our object to the summary
        /// </summary>
        /// <param name="summary"></param>
        private void UpdateSummary(Summary summary)
        {
            if (_quantity <= 0)
            {
                foreach (var item in summary.ActualCashDenoms)
                {
                    if (item.CurrencyDetailID == m_denom.Id &&
                        item.ISOCode == Parent.ISOCode)
                    {
                        summary.ActualCashDenoms.Remove(item);
                        break;
                    }
                }
            }
            else
            {
                bool itemUpdated = false;

                foreach (var item in summary.ActualCashDenoms)
                {
                    if (item.CurrencyDetailID == m_denom.Id &&
                        item.ISOCode == Parent.ISOCode)
                    {
                        item.Quantity = _quantity;
                        item.CurrencyValue = _quantity * m_denom.Value;
                        item.ExchangeRate = Parent.ExchangeRate;
                        item.DefaultValue = item.CurrencyValue * item.ExchangeRate;
                        itemUpdated = true;
                    }
                }

                if (!itemUpdated)
                {
                    var newDenom = new ActualCashCurrencyDenom();
                    newDenom.CurrencyDetailID = m_denom.Id;
                    newDenom.Quantity = _quantity;
                    newDenom.CurrencyValue = _quantity * m_denom.Value;
                    newDenom.ExchangeRate = Parent.ExchangeRate;
                    newDenom.DefaultValue = newDenom.CurrencyValue * newDenom.ExchangeRate;
                    newDenom.ISOCode = Parent.ISOCode;
                    summary.ActualCashDenoms.Add(newDenom);
                }
            }

            summary.ReloadActualCashFromDenoms();
        }

        #endregion
    }
}