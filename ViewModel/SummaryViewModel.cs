#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Business;
using GameTech.Elite.Client.Modules.SessionSummary.Properties;
using GameTech.Elite.UI;
using GameTech.Elite.Base.System;

namespace GameTech.Elite.Client.Modules.SessionSummary.ViewModel
{
    /// <summary>
    /// View model class for editing a session summary
    /// </summary>
    internal class SummaryViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Constants
        private const string FormatTwoDigit = "00";
        private const string FormatOneDigit = "0";
        private const string CheckFormatTwelveHour = "h";
        private const string CheckFormatTwoDigitHour = "hh";
        #endregion

        #region Member Variables
        private bool m_dataModified;
        private bool m_dataCalcd;
        private DateTime m_gamingDate;
        private string m_selectedSession;

        private bool m_canEditDate;
        private bool m_canEditSession;
        private bool m_canEditSummaryData;

        private bool m_allowModifications;

        private bool m_hasSuccess;

        private Staff m_staffDefault;

        private Summary m_editSummary;
        private Summary m_calcSummary;
        private Summary m_saveSummary;
        private bool m_doGenerateAfterSave;
        private List<BasicBankInfo> m_sessionBankInfo;

        private ListCollectionView m_sessionList;

        private string m_attendManual;
        private int m_attendManualTimeHrIndex;
        private int m_attendManualTimeMinIndex;
        private int m_attendManualTimeAMPMIndex;

        private Staff m_callerSelected;

        private string m_salePaper;
        private string m_saleElectronic;
        private string m_saleBingoOther; //FIX: DE8961 Session summary does calculate bingo other sales
        private string m_salePullTab;
        private string m_saleConcession;
        private string m_saleMerchandise;
        private string m_saleValidation;
        private string m_saleDeviceFee;
        private string m_saleDiscount;

        private string m_kioskSales;
        private string m_kioskVoids;//US5352 - kiosk voids

        private string m_prizeCash;
        private string m_prizeCheck;
        private string m_prizeMerchandise;
        private string m_prizeAccrualInc;
        private int m_prizeAccrualIncCountExpected;
        private int m_prizeAccrualIncCountApplied;
        private string m_prizePullTab;
        private string m_prizeOther;

        private string m_bankBegin;
        private string m_bankActual;
        private string m_bankFill;
        private string m_bankDebit;
        private string m_bankCheck;
        private string m_bankMoneyOrder;
        private string m_bankCoupon;
        private string m_bankGiftCard;
        private string m_bankChip;
        private string m_bankEnd;

        private string m_expectAccrualPay;
        private string m_expectAccrualCashPayouts;
        private string m_expectFeeWithheld;
        private string m_expectTax;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new summary view model
        /// </summary>
        /// <param name="controller">The controller for this view model</param>
        public SummaryViewModel(ISummaryController controller)
        {
            if(controller == null)
                throw new ArgumentNullException("controller");

            Controller = controller;

            AddCallerCommand = new RelayCommand((parameter) => OnAddCaller(), (parameter) => !IsBusy && SelectedCaller != null && CanEditSummaryData);
            DeleteCallerCommand = new RelayCommand((parameter) => OnDeleteCaller(), (parameter) => !IsBusy && SelectedChoosenCaller != null && CanEditSummaryData);
            EditSessionCostCommand = new RelayCommand((parameter) => OnSelectSessionCosts(), (parameter) => !IsBusy && CanEditSummaryData);
            ActualCashCalculatorCommand = new RelayCommand((parameter) => OnActualCashCalculator(), (parameter) => !IsBusy && CanEditSummaryData && (Controller.Parent.CashMethod != CashMethod.ByStaffMoneyCenter));
            DisplayStaffOverShortCommand = new RelayCommand((parameter) => OnDisplayBankOverShort(), (parameter) => !IsBusy);
            CalculateCommand = new RelayCommand((parameter) => OnCalculateSummary(), (parameter) => !IsBusy && CanEditSummaryData);
            TriggerAcc2IncreasesCommand = new RelayCommand((parameter) => OnTriggerAcc2Increases(), (parameter) => !IsBusy && CanEditSummaryData && PrizeAccrualIncreasesPending);
            GenerateCommand = new RelayCommand((parameter) => OnGenerateSummary(), (parameter) => !IsBusy && !HasError && CanEditSummaryData && Controller.StaffMember.Id != Staff.GameTechEmployee);
            SaveCommand = new RelayCommand((parameter) => OnSaveSummary(), (parameter) => !IsBusy && !HasError && CanEditSummaryData && Controller.StaffMember.Id != Staff.GameTechEmployee);
            ResetCommand = new RelayCommand((parameter) => OnResetSummary(), (parameter) => !IsBusy && CanEditSummaryData);

            // Listen for changes to the controller.
            PropertyChangedEventManager.AddListener(controller, this, string.Empty);
            controller.RequestSessionsCompleted += OnRequestSessionsCompleted;
            controller.RequestSummaryCompleted += OnRequestSummaryCompleted;
            controller.RequestCalculatedSummaryCompleted += OnRequestCalcdSummaryCompleted;
            controller.TriggerAcc2IncreasesCompleted += OnTriggerAcc2IncreasesCompleted;
            controller.RequestSetSummaryCompleted += OnRequestSetSummaryCompleted;
            controller.RequestSessionBanksCompleted += OnRequestSessionBanksCompleted;

            // Choose default staff
            m_staffDefault = new Staff();
            m_staffDefault.Id = 0;
            m_staffDefault.FirstName = "(N/A)";
            List<Staff> staffMembers = new List<Staff>();
            staffMembers.Add(m_staffDefault);
            staffMembers.AddRange(Controller.StaffMembers);

            // Manager
            Managers = new List<Staff>(staffMembers);

            // Caller
            Callers = new ObservableCollection<Staff>(Controller.StaffMembers);

            IEnumerator<Staff> staffMember = Callers.GetEnumerator();
            if(staffMember != null && staffMember.MoveNext())
                SelectedCaller = staffMember.Current;

            EditSummary = new Summary();
            EditSummary.LoadAvailableCurrencies(Controller.Parent.Currencies);

            GamingDate = Controller.GamingDate;

            AllowModifications = Controller.StaffMember.CheckModuleFeaturePermission((int)EliteModuleId.SessionSummary, (int)SessionSummaryFeatures.AllowModifications);
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Handles when a property has changed on the controller.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A PropertyChangedEventArgs object that contains the
        /// event data.</param>
        protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(sender == this)
                HandleSelfPropertyChanged(e);
            if(sender == Controller)
                HandleControllerPropertyChanged(e);
            else if(sender == EditSummary)
                HandleDataModelPropertyChanged(e);
        }

        /// <summary>
        /// Handles when a self property has changed
        /// </summary>
        /// <param name="e">the PropertyChangedEventArgs.</param>
        private void HandleSelfPropertyChanged(PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsBusy")
            {
                UpdateEditPermissions();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Handles when a controller property has changed
        /// </summary>
        /// <param name="e">the PropertyChangedEventArgs.</param>
        private void HandleControllerPropertyChanged(PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsBusy")
            {
                IsBusy = Controller.IsBusy;
                UpdateEditPermissions();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Handles when a summary data model has changed
        /// </summary>
        /// <param name="e">the PropertyChangedEventArgs.</param>
        private void HandleDataModelPropertyChanged(PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SessionCosts")
            {
                DataModified();
                RaisePropertyChanged("SessionCostsRegister");
                RaisePropertyChanged("SessionCostsNonRegister");
                RaisePropertyChanged("SessionCostsCombined");
            }
        }

        /// <summary>
        /// Prepares and sends request for a list of sessions for the gaming date
        /// </summary>
        /// <param name="date">The date of the list of session to request</param>
        private void UpdateRequestSessions()
        {
            // Requesting a new gaming date we need to clear the session list and selected session
            Sessions = null;
            SelectedSession = null;

            // update the ability to edit the summary form
            UpdateEditPermissions();

            // Sends the request to the controller which calls the message
            Controller.RequestSessions(m_gamingDate);
        }

        /// <summary>
        /// Handles when the session list has been updated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A RequestSessionsEventArgs object that contains the event data.</param>
        private void OnRequestSessionsCompleted(object sender, RequestSessionsEventArgs e)
        {
            if(e.Sessions != null)
            {
                List<string> tempSessions = new List<string>();
                if(e.Sessions != null)
                {
                    foreach(short session in e.Sessions)
                        tempSessions.Add(session.ToString(CultureInfo.CurrentCulture));
                }

                Sessions = new ListCollectionView(tempSessions);

                if(Sessions.Count > 0)
                {
                    SelectedSession = tempSessions.First();
                }
                else
                    SelectedSession = Resources.NoSessions;

                UpdateEditPermissions();
            }
            else
            {
                Sessions = null;
                SelectedSession = null;

                CheckForError();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Prepares and sends a request for a the session summary information for the gaming date and session number
        /// </summary>
        private void UpdateRequestSummary()
        {
            // update the ability to edit the summary form
            UpdateEditPermissions();

            short session = 0;
            if (short.TryParse(SelectedSession, out session))
            {
                Controller.RequestSummary(m_gamingDate, session); // Sends the request to the controller which calls the message
                Controller.RequestSessionBanks(m_gamingDate, session);
            }
        }

        /// <summary>
        /// Handles when the summary request has completed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A RequestSummaryEventArgs object that contains the event data.</param>
        private void OnRequestSummaryCompleted(object sender, RequestSummaryEventArgs e)
        {
            if(e.CalcdSummary != null)
            {
                m_calcSummary = e.CalcdSummary;
                m_saveSummary = e.SavedSummary;

                if(m_saveSummary != null)
                {
                    EditSummary = (Summary)m_saveSummary.Clone();
                    IsDataCalcd = false;
                }
                else
                {
                    EditSummary = (Summary)m_calcSummary.Clone();
                    IsDataCalcd = true;
                }

                // The save and get has completed now perform the generate
                if(m_doGenerateAfterSave)
                {
                    m_doGenerateAfterSave = false;
                    OnGenerateSummary();
                }

                if(!HasSuccess || HasError)
                {
                    CheckForError();
                }

                UpdateEditPermissions();
            }
            else
            {
                // The save summary
                m_calcSummary = null;
                m_saveSummary = null;

                CheckForError();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Handles when the calculate summary request has completed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A RequestCalcdSummaryEventArgs object that contains the event data.</param>
        private void OnRequestCalcdSummaryCompleted(object sender, RequestCalcdSummaryEventArgs e)
        {
            if(e.Summary != null)
            {
                m_calcSummary = e.Summary;

                // Create a clone of the calculated summary so that we can edit it.
                Summary newSummary = (Summary)m_calcSummary.Clone();
                // These values are not calculated and need to be kept after the calculation button is pressed.
                //newSummary.AttendanceManual = EditSummary.AttendanceManual; // since we load this value now, throw out what the user entered
                newSummary.AttendanceManualTime = EditSummary.AttendanceManualTime;
                newSummary.Manager = EditSummary.Manager;
                newSummary.Callers = EditSummary.Callers;
                newSummary.Comments = EditSummary.Comments;
                newSummary.SessionCosts = EditSummary.SessionCosts;
                newSummary.BankFill = EditSummary.BankFill;
                if (Controller.Parent.CashMethod != CashMethod.ByStaffMoneyCenter) // DE13400
                {
                    newSummary.ActualCashDenoms = EditSummary.ActualCashDenoms; // US4863 keep their entered denoms
                    newSummary.OverCash = EditSummary.OverCash;                 // this field is the display of the ActualCashDenoms
                }
                if (!Controller.Parent.Settings.UsePrevSessEndingBank || e.Summary.SessionNumber == 1 || newSummary.BankBegin == 0) // DE13718
                {
                    newSummary.BankBegin = EditSummary.BankBegin; // won't be loading the begin bank, keep whatever they entered in
                }
                if (newSummary.BankEnd == 0) // DE13400
                {
                    newSummary.BankEnd = EditSummary.BankEnd;
                }
                // Assign the new summary
                EditSummary = newSummary;
                IsDataCalcd = true;

                CheckForError();
                UpdateEditPermissions();
            }
            else
            {
                m_calcSummary = null;

                CheckForError();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void OnTriggerAcc2IncreasesCompleted(object sender, TriggerAcc2IncreasesEventArgs e)
        {
            OnCalculateSummary();
        }

        /// <summary>
        /// Handles when the set summary request has completed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An AsyncCompletedEventArgs.</param>
        private void OnRequestSetSummaryCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                HasSuccess = true;
                ProgressMessage = Resources.SaveSuccess;
                // Refresh the summary with the requested session
                UpdateRequestSummary();
            }
            else
            {
                CheckForError();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Handles when the get session banks has completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRequestSessionBanksCompleted(object sender, RequestSessionBanksEventArgs e)
        {
            m_sessionBankInfo = e.Banks;
            RaisePropertyChanged("CashierDrops");
            RaisePropertyChanged("ExpectTotal"); // DE13867
            RaisePropertyChanged("CashierOverShortTotal"); // DE13867
        }

        /// <summary>
        /// Updates the permissions of data on the view
        /// </summary>
        private void UpdateEditPermissions()
        {
            CanEditDate = (!IsBusy);
            CanEditSession = (!IsBusy && Sessions != null && Sessions.Count > 1);
            short result = 0;
            CanEditSummaryData = (!IsBusy && SelectedSession != null && short.TryParse(SelectedSession, out result));

            if(Controller.StaffMember.CheckModuleFeaturePermission((int)EliteModuleId.SessionSummary, (int)SessionSummaryFeatures.AllowModifications))
            {
                AllowModifications = CanEditSummaryData;
            }
            else
            {
                AllowModifications = false;
            }
        }

        /// <summary>
        /// Releases all resources used by SummaryViewModel.
        /// </summary>
        /// <param name="disposing">Whether this function is being called from 
        /// user code.</param>
        protected override void Dispose(bool disposing)
        {
            if(!IsDisposed)
            {
                if(disposing)
                {
                    Sessions = null;
                    EditSummary = null;

                    PropertyChangedEventManager.RemoveListener(Controller, this, string.Empty);
                    Controller.RequestSessionsCompleted -= OnRequestSessionsCompleted;
                    Controller.RequestSummaryCompleted -= OnRequestSummaryCompleted;
                    Controller.RequestCalculatedSummaryCompleted -= OnRequestCalcdSummaryCompleted;
                    Controller.TriggerAcc2IncreasesCompleted -= OnTriggerAcc2IncreasesCompleted;
                    Controller.RequestSetSummaryCompleted -= OnRequestSetSummaryCompleted;
                }

                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Notifies and set values for property of editing summary
        /// </summary>
        private void RaiseEditSummaryChanged()
        {
            // Attendance and times
            DateTime time = m_editSummary.AttendanceManualTime;
            int hrIndex = 0;
            if(Is12HrMode)
            {
                hrIndex = time.Hour % 12;
                if(hrIndex == 0)
                    hrIndex = 11;
                else
                    hrIndex -= 1;
            }
            else
                hrIndex = time.Hour;

            m_attendManual = m_editSummary.AttendanceManual.ToString(CultureInfo.CurrentCulture);
            m_attendManualTimeHrIndex = hrIndex;
            m_attendManualTimeMinIndex = time.Minute;
            m_attendManualTimeAMPMIndex = time.Hour >= 12 ? 1 : 0;
            RaisePropertyChanged("AttendManual");
            RaisePropertyChanged("AttendManualTimeHrIndex");
            RaisePropertyChanged("AttendManualTimeMinIndex");
            RaisePropertyChanged("AttendManualTimeAMPMIndex");

            // Spend properties
            RaisePropertyChanged("SpendTotal");
            RaisePropertyChanged("SpendBingoTotal");
            RaisePropertyChanged("SpendPTTotal");
            RaisePropertyChanged("SpendMrchTotal");
            RaisePropertyChanged("SpendConcTotal");
            RaisePropertyChanged("SpendPercentPayAccrualBased");
            RaisePropertyChanged("SpendPercentPayCashBased");
            RaisePropertyChanged("SpendPercentHoldAccrualBased");
            RaisePropertyChanged("SpendPercentHoldCashBased");

            // Manger, callers, and comments
            if(m_editSummary.Manager == null)
                m_editSummary.Manager = m_staffDefault;
            RaisePropertyChanged("SelectedManager");
            RaisePropertyChanged("ChoosenCallers");
            RaisePropertyChanged("Comments");

            // Sales
            SalePaper = m_editSummary.SalesPaper.ToString("F2", CultureInfo.CurrentCulture);
            SaleElec = m_editSummary.SalesElectronic.ToString("F2", CultureInfo.CurrentCulture);
            SaleBingoOther = m_editSummary.SalesBingoOther.ToString("F2", CultureInfo.CurrentCulture); //FIX: DE8961 Session summary does calculate bingo other sales
            SalePT = m_editSummary.SalesPullTab.ToString("F2", CultureInfo.CurrentCulture);
            SaleConc = m_editSummary.SalesConcession.ToString("F2", CultureInfo.CurrentCulture);
            SaleMrch = m_editSummary.SalesMerchandise.ToString("F2", CultureInfo.CurrentCulture);
            SaleValid = m_editSummary.SalesValidation.ToString("F2", CultureInfo.CurrentCulture);
            SaleDeviceFee = m_editSummary.SalesDeviceFee.ToString("F2", CultureInfo.CurrentCulture);
            SaleDisc = m_editSummary.SalesDiscount.ToString("F2", CultureInfo.CurrentCulture);

            KioskSales = m_editSummary.KioskSale.ToString("F2", CultureInfo.CurrentCulture);
            KioskVoids = m_editSummary.KioskVoids.ToString("F2", CultureInfo.CurrentCulture);//US5352 - kiosk voids

            RaisePropertyChanged("SaleTotal");
            RaisePropertyChanged("SaleAdjustTotal");

            // Prizes
            PrizeCash = m_editSummary.PrizesCash.ToString("F2", CultureInfo.CurrentCulture);
            PrizeCheck = m_editSummary.PrizesCheck.ToString("F2", CultureInfo.CurrentCulture);
            PrizeMerchandise = m_editSummary.PrizesMerchandise.ToString("F2", CultureInfo.CurrentCulture);
            PrizeAccrualInc = m_editSummary.PrizesAccrualInc.ToString("F2", CultureInfo.CurrentCulture);
            PrizeAccrualIncCountExpected = m_editSummary.PrizesAccrualIncCountExpected;
            PrizeAccrualIncCountApplied = m_editSummary.PrizesAccrualIncCountApplied;
            PrizePullTab = m_editSummary.PrizesPullTab.ToString("F2", CultureInfo.CurrentCulture);
            PrizeOther = m_editSummary.PrizesOther.ToString("F2", CultureInfo.CurrentCulture);
            RaisePropertyChanged("PrizeTotal");
            RaisePropertyChanged("PrizeTotalAccrualBased");
            RaisePropertyChanged("PrizeTotalCashBased");

            BankBegin = m_editSummary.BankBegin.ToString("F2", CultureInfo.CurrentCulture);
            BankFill = m_editSummary.BankFill.ToString("F2", CultureInfo.CurrentCulture);
            BankActual = m_editSummary.OverCash.ToString("F2", CultureInfo.CurrentCulture);
            BankDebit = m_editSummary.OverDebitCredit.ToString("F2", CultureInfo.CurrentCulture);
            BankCheck = m_editSummary.OverChecks.ToString("F2", CultureInfo.CurrentCulture);
            BankMoneyOrder = m_editSummary.OverMoneyOrders.ToString("F2", CultureInfo.CurrentCulture);
            BankCoupon = m_editSummary.OverCoupons.ToString("F2", CultureInfo.CurrentCulture);
            BankGiftCard = m_editSummary.OverGiftCards.ToString("F2", CultureInfo.CurrentCulture);
            BankChip = m_editSummary.OverChips.ToString("F2", CultureInfo.CurrentCulture);
            RaisePropertyChanged("BankOverTotal");
            BankEnd = m_editSummary.BankEnd.ToString("F2", CultureInfo.CurrentCulture);
            RaisePropertyChanged("BankTotal");
            RaisePropertyChanged("CashierOverShortTotal");
            RaisePropertyChanged("SessionOverShortTotal");
            RaisePropertyChanged("CashierBanks");

            ExpectAccrualPay = m_editSummary.PrizesAccrualPay.ToString("F2", CultureInfo.CurrentCulture);
            ExpectAccrualCashPay = m_editSummary.AccrualCashPayouts.ToString("F2", CultureInfo.CurrentCulture);

            ExpectFeeWithheld = m_editSummary.PrizesFees.ToString("F2", CultureInfo.CurrentCulture);
            RaisePropertyChanged("SessionCostsRegister");
            RaisePropertyChanged("SessionCostsCombined");
            ExpectTax = m_editSummary.SalesTax.ToString("F2", CultureInfo.CurrentCulture);
            
            RaisePropertyChanged("ExpectTotal");

            RaisePropertyChanged("SessionCostsNonRegister");
            RaisePropertyChanged("SessionCostsCombined");
            IsDataModified = false;
        }

        /// <summary>
        /// updates the date when a portion of the date is changed
        /// </summary>
        private void DateModified()
        {
            DateTime temp = GamingDate ?? DateTime.MinValue;
            int hr = Is12HrMode ? AttendManualTimeHrIndex + 1 : AttendManualTimeHrIndex;
            int min = AttendManualTimeMinIndex;
            int sec = 0;

            // Is AM or PM selected
            if(Is12HrMode)
            {
                if(AttendManualTimeAMPMIndex != 0 && hr < 12)
                    hr += 12;
                else if(AttendManualTimeAMPMIndex == 0 && hr == 12)
                    hr -= 12;
            }

            m_editSummary.AttendanceManualTime = new DateTime(temp.Year, temp.Month, temp.Day, hr, min, sec);
            IsDataModified = true;
        }

        /// <summary>
        /// Updates total fields when data has changed
        /// </summary>
        private void DataModified()
        {
            IsDataModified = true;
            // Spend Totals
            RaisePropertyChanged("SpendTotal");
            RaisePropertyChanged("SpendBingoTotal");
            RaisePropertyChanged("SpendPTTotal");
            RaisePropertyChanged("SpendMrchTotal");
            RaisePropertyChanged("SpendConcTotal");
            RaisePropertyChanged("SpendPercentPayAccrualBased");
            RaisePropertyChanged("SpendPercentPayCashBased");
            RaisePropertyChanged("SpendPercentHoldAccrualBased");
            RaisePropertyChanged("SpendPercentHoldCashBased");

            // Money Totals
            RaisePropertyChanged("SaleTotal");
            RaisePropertyChanged("SaleAdjustTotal");
            RaisePropertyChanged("PrizeTotal");
            RaisePropertyChanged("PrizeTotalAccrualBased");
            RaisePropertyChanged("PrizeTotalCashBased");
            RaisePropertyChanged("ExpectTotal");
            RaisePropertyChanged("BankOverTotal");
            RaisePropertyChanged("BankDepositTotal");
            RaisePropertyChanged("CashierBanks");
            RaisePropertyChanged("CashierOverShortTotal");
            RaisePropertyChanged("SessionOverShortTotal");
        }

        /// <summary>
        /// Adds a caller to the list of choosen callers
        /// </summary>
        private void OnAddCaller()
        {
            Staff staffPerson = SelectedCaller;

            if(staffPerson != null)
            {
                if(m_editSummary.Callers == null)
                    m_editSummary.Callers = new List<Staff>();

                if(!m_editSummary.Callers.Contains(staffPerson))
                    m_editSummary.Callers.Add(staffPerson);
                RaisePropertyChanged("ChoosenCallers");
                IsDataModified = true;
            }
        }

        /// <summary>
        /// Removes a caller from the list of choosen callers
        /// </summary>
        private void OnDeleteCaller()
        {
            Staff staffPerson = SelectedChoosenCaller;
            if(staffPerson != null)
            {
                if(m_editSummary.Callers == null)
                    m_editSummary.Callers = new List<Staff>();

                m_editSummary.Callers.Remove(staffPerson);
                RaisePropertyChanged("ChoosenCallers");
                IsDataModified = true;
            }
        }

        /// <summary>
        /// Selects the session costs
        /// </summary>
        private void OnSelectSessionCosts()
        {
            Controller.SelectSessionCosts(m_editSummary);
        }

        /// <summary>
        /// Pops up the "Actual Cash Calculator"
        /// </summary>
        private void OnActualCashCalculator()
        {
            Controller.CalculateActualCash(m_editSummary);
            BankActual = m_editSummary.OverCash.ToString("F2");
            if (Controller.Parent.Settings.SetEndingBankToActualCash) // US4979
                BankEnd = BankActual;
            RaisePropertyChanged("AllowManualBankActual");
        }

        /// <summary>
        /// Displays the status of the banks for the selected session
        /// </summary>
        private void OnDisplayBankOverShort()
        {
            Controller.DisplaySessionBanks(m_sessionBankInfo);
        }

        /// <summary>
        /// Display calculated a session summary report.
        /// </summary>
        private void OnCalculateSummary()
        {
            // update the ability to edit the summary form
            UpdateEditPermissions();

            short session = 0;
            if(short.TryParse(SelectedSession, out session))
                Controller.RequestCalcdSummary(m_gamingDate, session); // Sends the request to the controller which calls the message
            IsDataCalcd = true;
            Controller.RequestSessionBanks(m_gamingDate, session);
        }

        public void OnTriggerAcc2Increases()
        {
            // update the ability to edit the summary form
            UpdateEditPermissions();

            short session = 0;
            if(short.TryParse(SelectedSession, out session))
                Controller.TriggerAcc2Increases(m_gamingDate, session); // Sends the request to the controller which calls the message
            IsDataCalcd = true;
        }

        /// <summary>
        /// Generates a session summary report.
        /// </summary>
        private void OnGenerateSummary()
        {
            if(GenerateRequiresSave())
            {
                m_doGenerateAfterSave = true;
                OnSaveSummary();
            }
            else
            {
                short session = 0;
                if(!short.TryParse(SelectedSession, out session))
                    session = 0;

                Controller.GenerateReport(m_gamingDate, session);
            }
        }

        /// <summary>
        /// Checks to see if a generate requires a save
        /// </summary>
        /// <returns></returns>
        private bool GenerateRequiresSave()
        {
            return (IsDataModified || IsDataCalcd);
        }

        /// <summary>
        /// Save the session summary.
        /// </summary>
        private void OnSaveSummary()
        {
            m_editSummary.SessionDate = GamingDate ?? DateTime.MinValue;

            short result = 0;
            if(!short.TryParse(m_selectedSession, out result))
                result = short.MinValue;

            m_editSummary.SessionNumber = result;
            Controller.RequestSetSummary(m_editSummary);
        }

        /// <summary>
        /// Reverts date to saved data if available otherwise calculated data.
        /// </summary>
        private void OnResetSummary()
        {
            short session = 0;
            short.TryParse(SelectedSession, out session);
            if(m_saveSummary != null)
            {
                EditSummary = (Summary)m_saveSummary;
                IsDataCalcd = false;
            }
            else if(m_calcSummary != null)
            {
                EditSummary = (Summary)m_calcSummary.Clone();
                IsDataCalcd = true;
            }
            else
            {
                if(GamingDate == null || session == 0)
                    EditSummary = new Summary();
                else
                    EditSummary = new Summary(m_gamingDate, session);

                EditSummary.LoadAvailableCurrencies(Controller.Parent.Currencies);

                IsDataCalcd = false;
            }
            Controller.RequestSessionBanks(m_gamingDate, session);
        }

        #endregion

        #region Member Properties
        /// <summary>
        /// The controller of this view model.
        /// </summary>
        public ISummaryController Controller
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the text for the calculated button
        /// </summary>
        public string CalculateText
        {
            get
            {
                if(m_saveSummary == null || !IsDataCalcd) // if it's never been saved or not calculated, then display "calculate"
                    return Resources.Calculate;
                else
                    return Resources.Recalculate;
            }
        }

        /// <summary>
        /// Gets the text for the accrual increase button
        /// </summary>
        public string TriggerAcc2IncreasesText { get { return Resources.Acc2TriggerIncreasesAndReseed; } }

        /// <summary>
        /// Getst the text for the generate button
        /// </summary>
        public string GenerateText
        {
            get
            {
                if(GenerateRequiresSave())
                    return Resources.SaveGenerate;
                else
                    return Resources.Generate;
            }
        }

        /// <summary>
        /// Gets the current editing summary
        /// </summary>
        public Summary EditSummary
        {
            get
            {
                return m_editSummary;
            }
            private set
            {
                if(m_editSummary != null)
                    PropertyChangedEventManager.RemoveListener(m_editSummary, this, string.Empty);
                m_editSummary = value;
                if(m_editSummary != null)
                    PropertyChangedEventManager.AddListener(m_editSummary, this, string.Empty);
                RaisePropertyChanged("EditSummary");
                RaiseEditSummaryChanged(); // IsDataModified = false;
            }
        }

        /// <summary>
        /// The gets or sets whether data has been modified
        /// </summary>
        public bool IsDataModified
        {
            get
            {
                return m_dataModified;
            }
            set
            {
                m_dataModified = value;
                RaisePropertyChanged("IsDateModified");
                RaisePropertyChanged("GenerateText");
                RaisePropertyChanged("CalculateText");

                if(m_dataModified)
                {
                    HasSuccess = false;
                    ProgressMessage = Error;
                }
            }
        }

        /// <summary>
        /// Get whether the data being used is the calculated data or not
        /// </summary>
        public bool IsDataCalcd
        {
            get
            {
                return m_dataCalcd;
            }
            private set
            {
                if(m_dataCalcd != value)
                {
                    m_dataCalcd = value;
                    RaisePropertyChanged("IsDataCalcd");
                    RaisePropertyChanged("GenerateText");
                    RaisePropertyChanged("CalculateText");
                }
            }
        }

        /// <summary>
        /// The selected gaming date.
        /// </summary>
        public DateTime? GamingDate
        {
            get
            {
                return m_gamingDate;
            }
            set
            {
                DateTime date = DateTime.MinValue;
                if(value.HasValue)
                    date = (DateTime)value;

                if(m_gamingDate != date)
                {
                    m_gamingDate = date;
                    RaisePropertyChanged("GamingDate");
                    UpdateRequestSessions();
                    HasSuccess = false;
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// A list of session for the selected gaming date.
        /// </summary>
        public ListCollectionView Sessions
        {
            get
            {
                return m_sessionList;
            }
            private set
            {
                m_sessionList = value;
                RaisePropertyChanged("Sessions");
                HasSuccess = false;
                CheckForError();
            }
        }

        /// <summary>
        /// The currently selected session.
        /// </summary>
        public string SelectedSession
        {
            get
            {
                return m_selectedSession;
            }
            set
            {
                if(m_selectedSession != value)
                {
                    m_selectedSession = value;
                    RaisePropertyChanged("SelectedSession");
                    RaisePropertyChanged("HasError");

                    UpdateRequestSummary();
                }
            }
        }

        /// <summary>
        /// Gets whether the gaming date can be changed
        /// </summary>
        public bool CanEditDate
        {
            get
            {
                return m_canEditDate;
            }
            private set
            {
                if(m_canEditDate != value)
                {
                    m_canEditDate = value;
                    RaisePropertyChanged("CanEditDate");
                }
            }
        }

        /// <summary>
        /// Gets or sets the to edit the current session
        /// </summary>
        public bool CanEditSession
        {
            get
            {
                return m_canEditSession;
            }
            private set
            {
                if(m_canEditSession != value)
                {
                    m_canEditSession = value;
                    RaisePropertyChanged("CanEditSession");
                }
            }
        }

        /// <summary>
        /// Get or sets the ability to edit the session summary data
        /// </summary>
        public bool CanEditSummaryData
        {
            get
            {
                return m_canEditSummaryData;
            }
            set
            {
                if(m_canEditSummaryData != value)
                {
                    m_canEditSummaryData = value;
                    RaisePropertyChanged("CanEditSummaryData");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the last operation was successful.
        /// </summary>
        public bool HasSuccess
        {
            get
            {
                return m_hasSuccess;
            }
            private set
            {
                if(m_hasSuccess != value)
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
        /// Checks for an error in the summary data.
        /// </summary>
        private void CheckForError()
        {
            RaisePropertyChanged("HasError");
            ProgressMessage = Error;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error
        {
            get
            {
                foreach(PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
                {
                    string propertyError = this[prop.Name];
                    if(!string.IsNullOrEmpty(propertyError))
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

                if(propertyName == "GamingDate")
                {
                    if(GamingDate == null || m_gamingDate == DateTime.MinValue)
                        error = Resources.InvalidGamingDate;
                }
                else if(propertyName == "Sessions")
                {
                    if(Sessions == null)
                        error = Resources.LoadingSessionsFailed;
                }
                else if(propertyName == "SelectedSession")
                {
                    short result = 0;
                    if (!IsBusy && (SelectedSession == null || !short.TryParse(SelectedSession, out result)))
                        error = Resources.InvalidSelectedSession;
                }
                else if(propertyName == "EditSummary")
                {
                    if(m_calcSummary == null)
                        error = Resources.LoadingSummaryFailed;
                }
                else if(propertyName == "AttendManual")
                {
                    if(EditSummary.AttendanceManual < 0)
                        error = Resources.InvalidManualAttendance;
                }
                else if(propertyName == "SalePaper")
                {
                    if(EditSummary.SalesPaper == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "SaleElec")
                {
                    if(EditSummary.SalesElectronic == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                //FIX: DE8961 Session summary does calculate bingo other sales
                else if(propertyName == "SaleBingoOther")
                {
                    if(EditSummary.SalesBingoOther == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                //END: DE8961
                else if(propertyName == "SalePT")
                {
                    if(EditSummary.SalesPullTab == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "SaleConc")
                {
                    if(EditSummary.SalesConcession == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "SaleMrch")
                {
                    if(EditSummary.SalesMerchandise == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "SaleDeviceFee")
                {
                    if(EditSummary.SalesDeviceFee == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "SaleDisc")
                {
                    if(EditSummary.SalesDiscount == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "PrizeCash")
                {
                    if(EditSummary.PrizesCash == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "PrizeCheck")
                {
                    if(EditSummary.PrizesCheck == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "PrizeMerchandise")
                {
                    if(EditSummary.PrizesMerchandise == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "PrizeAccrualInc")
                {
                    if(EditSummary.PrizesAccrualInc == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "PrizePullTab")
                {
                    if(EditSummary.PrizesPullTab == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "PrizeOther")
                {
                    if(EditSummary.PrizesOther == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "ExpectAccrualPay")
                {
                    if(EditSummary.PrizesAccrualPay == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "ExpectAccrualCashPay")
                {
                    if(EditSummary.AccrualCashPayouts == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "ExpectFeeWithheld")
                {
                    if(EditSummary.PrizesFees == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "ExpectTax")
                {
                    if(EditSummary.SalesTax == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankBegin")
                {
                    if(EditSummary.BankBegin == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankFill")
                {
                    if(EditSummary.BankFill == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankActual")
                {
                    if(EditSummary.OverCash == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankDebit")
                {
                    if(EditSummary.OverDebitCredit == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankCheck")
                {
                    if(EditSummary.OverChecks == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankMoneyOrder")
                {
                    if(EditSummary.OverChecks == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankCoupon")
                {
                    if(EditSummary.OverChecks == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankGiftCard")
                {
                    if(EditSummary.OverChecks == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankChip")
                {
                    if(EditSummary.OverChecks == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankEnd")
                {
                    if(EditSummary.BankEnd == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if(propertyName == "BankDepositTotal")
                {
                    if(EditSummary.DepositTotal(false) == decimal.MinValue)
                        error = Resources.InvalidValue;
                }
                else if (propertyName == "KioskSales")
                {
                    if (EditSummary.KioskSale == decimal.MinValue)
                        error = Resources.InvalidValue;
                }

                return error;
            }
        }

        #region Attendance Member Properties
        /// <summary>
        /// Gets the system calculated attendance
        /// </summary>
        public int AttendSystem
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time of the system calculated attendance
        /// </summary>
        public DateTime AttendSytemTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or set the manual attendance count.
        /// </summary>
        public string AttendManual
        {
            get
            {
                return m_attendManual;
            }
            set
            {
                if(m_attendManual != value)
                {
                    m_attendManual = value;

                    int result = 0;
                    if(!int.TryParse(m_attendManual, out result))
                        result = int.MinValue;

                    EditSummary.AttendanceManual = result;
                    DataModified();
                    RaisePropertyChanged("AttendManual");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the hour of the manual attendance time.
        /// </summary>
        public int AttendManualTimeHrIndex
        {
            get
            {
                return m_attendManualTimeHrIndex;
            }
            set
            {
                if(m_attendManualTimeHrIndex != value)
                {
                    m_attendManualTimeHrIndex = value;
                    DateModified();
                    RaisePropertyChanged("AttendanceManualTimeHrIndex");
                }
            }
        }

        /// <summary>
        /// Gets or sets the minute of the manual attendance time.
        /// </summary>
        public int AttendManualTimeMinIndex
        {
            get
            {
                return m_attendManualTimeMinIndex;
            }
            set
            {
                if(m_attendManualTimeMinIndex != value)
                {
                    m_attendManualTimeMinIndex = value;
                    DateModified();
                    RaisePropertyChanged("AttendanceManualTimeMinIndex");
                }
            }
        }

        /// <summary>
        /// Gets of sets the AM PM value of the manual attendance time.
        /// </summary>
        public int AttendManualTimeAMPMIndex
        {
            get
            {
                return m_attendManualTimeAMPMIndex;
            }
            set
            {
                if(m_attendManualTimeAMPMIndex != value)
                {
                    m_attendManualTimeAMPMIndex = value;
                    DateModified();
                    RaisePropertyChanged("AttendanceManualTimeAMPMIndex");
                }
            }
        }

        /// <summary>
        /// Gets if the time format is in 12 or 24 hour mode
        /// </summary>
        public bool Is12HrMode
        {
            get
            {
                string shortTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                return shortTimePattern.Contains(CheckFormatTwelveHour); // 12 hour format
            }
        }

        /// <summary>
        /// Gets if the hour format forces 2 digits or not
        /// </summary>
        public bool Is2DigitHr
        {
            get
            {
                string shortTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                return shortTimePattern.IndexOf(CheckFormatTwoDigitHour, StringComparison.OrdinalIgnoreCase) >= 0; // Check for 2 digit hour
            }
        }

        /// <summary>
        /// Gets a list of hours for selecting a time
        /// </summary>
        public List<string> TimeHrs
        {
            get
            {
                List<string> hourList = new List<string>();
                string format = Is2DigitHr ? FormatTwoDigit : FormatOneDigit;
                if(Is12HrMode)
                {
                    for(int index = 1; index <= 12; ++index)
                    {
                        hourList.Add(index.ToString(format, CultureInfo.CurrentCulture));
                    }
                }
                else
                {
                    for(int index = 0; index < 24; ++index)
                    {
                        hourList.Add(index.ToString(format, CultureInfo.CurrentCulture));
                    }
                }

                return hourList;
            }

        }

        /// <summary>
        /// Gets a list of minutes from selecting a time
        /// </summary>
        public List<string> TimeMins
        {
            get
            {
                // List from 00-59
                List<string> minList = new List<string>();
                for(int index = 0; index < 60; ++index)
                {
                    minList.Add(index.ToString(FormatTwoDigit, CultureInfo.CurrentCulture));
                }

                return minList;
            }
        }

        /// <summary>
        /// Gets the time seperator
        /// </summary>
        public string TimeSeparator
        {
            get
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
            }
        }

        /// <summary>
        /// Gets the am and pm designators in a list
        /// </summary>
        public List<string> TimeAMPMDesignators
        {
            get
            {
                List<string> designatorList = new List<string>();
                designatorList.Add(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator);
                designatorList.Add(CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator);
                return designatorList;
            }
        }

        #endregion

        #region Spend Member Properties

        /// <summary>
        /// Gets the average spend per player.
        /// </summary>
        public string SpendTotal
        {
            get
            {
                return m_editSummary.SpendTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the bingo spend per player.
        /// </summary>
        public string SpendBingoTotal
        {
            get
            {
                return m_editSummary.SpendBingoTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the pull tab spend per player.
        /// </summary>
        public string SpendPTTotal
        {
            get
            {
                return m_editSummary.SpendPullTabTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the merchandise spend per player.
        /// </summary>
        public string SpendMrchTotal
        {
            get
            {
                return m_editSummary.SpendMerchandiseTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the concessions spend per player.
        /// </summary>
        public string SpendConcTotal
        {
            get
            {
                return m_editSummary.SpendConcessionTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the bingo pay percentage accrual based.
        /// </summary>
        public string SpendPercentPayAccrualBased
        {
            get
            {
                return m_editSummary.SpendPercentPayAccrualBased().ToString("P2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the bingo pay percentage cash based.
        /// </summary>
        public string SpendPercentPayCashBased
        {
            get
            {
                return m_editSummary.SpendPercentPayCashBased().ToString("P2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the bingo hold percentage accrual based.
        /// </summary>
        public string SpendPercentHoldAccrualBased
        {
            get
            {
                return m_editSummary.SpendPercentHoldAccrualBased().ToString("P2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the bingo hold percentage cash based.
        /// </summary>
        public string SpendPercentHoldCashBased
        {
            get
            {
                return m_editSummary.SpendPercentHoldCashBased().ToString("P2", CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Staff and Comment Properties

        /// <summary>
        /// Gets the list of managers.
        /// </summary>
        public IEnumerable<Staff> Managers
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the currently selected manager
        /// </summary>
        public Staff SelectedManager
        {
            get
            {
                Debug.Assert(m_editSummary != null);
                return m_editSummary.Manager;
            }
            set
            {
                if(m_editSummary.Manager != value)
                {
                    m_editSummary.Manager = value;
                    IsDataModified = true;
                    RaisePropertyChanged("SelectedManager");
                }
            }
        }

        /// <summary>
        /// Gets the list of callers.
        /// </summary>
        public ObservableCollection<Staff> Callers
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the currently selected caller
        /// </summary>
        public Staff SelectedCaller
        {
            get
            {
                return m_callerSelected;
            }
            set
            {
                if(m_callerSelected != value)
                {
                    m_callerSelected = value;
                    RaisePropertyChanged("SelectedCaller");
                }
            }
        }

        /// <summary>
        /// Gets or sets a list of choosen callers
        /// </summary>
        public ObservableCollection<Staff> ChoosenCallers
        {
            get
            {
                Debug.Assert(m_editSummary != null);
                return new ObservableCollection<Staff>(m_editSummary.Callers);
            }
        }

        /// <summary>
        /// Gets or sets the selected caller from the choosen callers
        /// </summary>
        public Staff SelectedChoosenCaller
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets comments.
        /// </summary>
        public string Comments
        {
            get
            {
                Debug.Assert(m_editSummary != null);
                return m_editSummary.Comments;
            }
            set
            {
                if(m_editSummary.Comments != value)
                {
                    m_editSummary.Comments = value;
                    IsDataModified = true;
                    RaisePropertyChanged("Comments");
                }
            }
        }

        #endregion

        #region Bank Member Properties
        /// <summary>
        /// Gets or sets the beginning bank
        /// </summary>
        public string BankBegin
        {
            get
            {
                return m_bankBegin;
            }
            set
            {
                if(m_bankBegin != value)
                {
                    m_bankBegin = value;
                    decimal result = 0M;

                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.BankBegin = result;
                    DataModified();
                    RaisePropertyChanged("BankBegin");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bank fill
        /// </summary>
        public string BankFill
        {
            get
            {
                return m_bankFill;
            }
            set
            {
                if(m_bankFill != value)
                {
                    m_bankFill = value;
                    decimal result = 0M;

                    if(!Decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = Decimal.MinValue;

                    m_editSummary.BankFill = result;
                    DataModified();
                    RaisePropertyChanged("BankFill");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the over/short actual cash value
        /// </summary>
        public string BankActual
        {
            get
            {
                return m_bankActual;
            }
            set
            {
                if(m_bankActual != value)
                {
                    m_bankActual = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.OverCash = result;
                    DataModified();
                    RaisePropertyChanged("BankActual");
                    RaisePropertyChanged("AllowManualBankActual");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Can we modify the actual bank?
        /// </summary>
        public bool AllowManualBankActual
        {
            get
            {
                return (m_editSummary.ActualCashDenoms.Count == 0);
            }
        }

        /// <summary>
        /// Gets or sets the over/short debit/credit value
        /// </summary>
        public string BankDebit
        {
            get
            {
                return m_bankDebit;
            }
            set
            {
                if(m_bankDebit != value)
                {
                    m_bankDebit = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.OverDebitCredit = result;
                    DataModified();
                    RaisePropertyChanged("BankDebit");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the over/short check value
        /// </summary>
        public string BankCheck
        {
            get
            {
                return m_bankCheck;
            }
            set
            {
                if(m_bankCheck != value)
                {
                    m_bankCheck = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.OverChecks = result;
                    DataModified();
                    RaisePropertyChanged("BankCheck");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the over/short money order value
        /// </summary>
        public string BankMoneyOrder
        {
            get
            {
                return m_bankMoneyOrder;
            }
            set
            {
                if(m_bankMoneyOrder != value)
                {
                    m_bankMoneyOrder = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.OverMoneyOrders = result;
                    DataModified();
                    RaisePropertyChanged("BankMoneyOrder");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the over/short coupon value
        /// </summary>
        public string BankCoupon
        {
            get
            {
                return m_bankCoupon;
            }
            set
            {
                if(m_bankCoupon != value)
                {
                    m_bankCoupon = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.OverCoupons = result;
                    DataModified();
                    RaisePropertyChanged("BankCoupon");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the over/short gift card value
        /// </summary>
        public string BankGiftCard
        {
            get
            {
                return m_bankGiftCard;
            }
            set
            {
                if(m_bankGiftCard != value)
                {
                    m_bankGiftCard = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.OverGiftCards = result;
                    DataModified();
                    RaisePropertyChanged("BankGiftCard");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the over/short money order value
        /// </summary>
        public string BankChip
        {
            get
            {
                return m_bankChip;
            }
            set
            {
                if(m_bankChip != value)
                {
                    m_bankChip = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.OverChips = result;
                    DataModified();
                    RaisePropertyChanged("BankChip");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets the over/short value
        /// </summary>
        public string BankOverTotal
        {
            get
            {
                if (Controller.Parent.Settings.SessionSummaryViewMode == SessionSummaryViewModes.Nevada) // US5345
                {
                    var overShort = m_editSummary.OverTotal(true) - Convert.ToDecimal(CashierDrops);
                    return overShort.ToString("F2", CultureInfo.CurrentCulture);
                }

                return m_editSummary.OverTotal(false).ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets or sets the ending bank
        /// </summary>
        public string BankEnd
        {
            get
            {
                return m_bankEnd;
            }
            set
            {
                if(m_bankEnd != value)
                {
                    m_bankEnd = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.BankEnd = result;
                    DataModified();
                    RaisePropertyChanged("BankEnd");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets the the cash deposit value
        /// </summary>
        public string BankDepositTotal
        {
            get
            {
                CheckForError();
                var isNevada = Controller.Parent.Settings.SessionSummaryViewMode == SessionSummaryViewModes.Nevada;
                return m_editSummary.DepositTotal(isNevada).ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        public string CashierBanks
        {
            get
            {
                decimal bankAmount = 0m;
                if (m_sessionBankInfo != null)
                {
                    bankAmount = m_sessionBankInfo.Sum(b => b.BankAmount);
                }

                return bankAmount.ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the total amount that the cashiers have dropped
        /// </summary>
        public string CashierDrops
        {
            get
            {
                decimal total = 0;

                try
                {
                    if (m_sessionBankInfo != null)
                    {
                        foreach (var bank in m_sessionBankInfo)
                        {
                            total += bank.TotalDrop;
                        }
                    }
                }
                catch (Exception)
                {

                }
                return total.ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        public string CashierOverShortTotal
        {
            get
            {
                decimal bankAmount = 0m;
                if (m_sessionBankInfo != null)
                {
                    bankAmount = m_sessionBankInfo.Sum(b => b.BankAmount);
                }
                var overShort = Convert.ToDecimal(CashierBanks) + m_editSummary.SalesTotal() - Convert.ToDecimal(CashierDrops);
                if (Controller.Parent.Settings.SessionSummaryViewMode == SessionSummaryViewModes.Nevada)
                {
                    overShort -= (m_editSummary.KioskVoids + m_editSummary.KioskSale);
                }

                overShort = overShort*-1; //negate
                return overShort.ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        public string SessionOverShortTotal
        {
            get
            {
                var overShort = Convert.ToDecimal(CashierOverShortTotal) + Convert.ToDecimal(BankOverTotal);
                return overShort.ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Sales Member Properties

        /// <summary>
        /// Gets or sets the paper sales
        /// </summary>
        public string SalePaper
        {
            get
            {
                return m_salePaper;
            }
            set
            {
                if(m_salePaper != value)
                {
                    m_salePaper = value;
                    decimal result = 0M;

                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesPaper = result;
                    DataModified();
                    RaisePropertyChanged("SalePaper");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the electronic sales
        /// </summary>
        public string SaleElec
        {
            get
            {
                return m_saleElectronic;
            }
            set
            {
                if(m_saleElectronic != value)
                {
                    m_saleElectronic = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesElectronic = result;
                    DataModified();
                    RaisePropertyChanged("SaleElec");
                    CheckForError();
                }
            }
        }

        //FIX: DE8961 Session summary does calculate bingo other sales
        /// <summary>
        /// Gets or sets the bingo other sales
        /// </summary>
        public string SaleBingoOther
        {
            get
            {
                return m_saleBingoOther;
            }
            set
            {
                if(m_saleBingoOther != value)
                {
                    m_saleBingoOther = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesBingoOther = result;
                    DataModified();
                    RaisePropertyChanged("SaleBingoOther");
                    CheckForError();
                }
            }
        }
        //END: DE8961

        /// <summary>
        /// Gets or sets the pull tab sales
        /// </summary>
        public string SalePT
        {
            get
            {
                return m_salePullTab;
            }
            set
            {
                if(m_salePullTab != value)
                {
                    m_salePullTab = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesPullTab = result;
                    DataModified();
                    RaisePropertyChanged("SalePT");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or set the concession sales
        /// </summary>
        public string SaleConc
        {
            get
            {
                return m_saleConcession;
            }
            set
            {
                if(m_saleConcession != value)
                {
                    m_saleConcession = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesConcession = result;
                    DataModified();
                    RaisePropertyChanged("SaleConc");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the merchandise sales
        /// </summary>
        public string SaleMrch
        {
            get
            {
                return m_saleMerchandise;
            }
            set
            {
                if(m_saleMerchandise != value)
                {
                    m_saleMerchandise = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesMerchandise = result;
                    DataModified();
                    RaisePropertyChanged("SaleMrch");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation sales
        /// </summary>
        public string SaleValid
        {
            get
            {
                return m_saleValidation;
            }
            set
            {
                if(m_saleValidation != value)
                {
                    m_saleValidation = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesValidation = result;
                    DataModified();
                    RaisePropertyChanged("SaleValid");
                    CheckForError();
                }
            }
        }

        public string SaleDeviceFee
        {
            get
            {
                return m_saleDeviceFee;
            }
            set
            {
                if(m_saleDeviceFee != value)
                {
                    m_saleDeviceFee = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesDeviceFee = result;
                    DataModified();
                    RaisePropertyChanged("SaleDeviceFee");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the sales discounts
        /// </summary>
        public string SaleDisc
        {
            get
            {
                return m_saleDiscount;
            }
            set
            {
                if(m_saleDiscount != value)
                {
                    m_saleDiscount = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesDiscount = result;
                    DataModified();
                    RaisePropertyChanged("SaleDisc");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the kiosk sales
        /// </summary>
        public string KioskSales
        {
            get
            {
                return m_kioskSales;
            }

            set
            {
                if (m_kioskSales != value)
                {
                    m_kioskSales = value;
                    decimal result = 0M;
                    if (!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.KioskSale = result;
                    DataModified();
                    RaisePropertyChanged("KioskSales");
                    CheckForError();
                }
            }
        }

        //US5352 - kiosk voids
        /// <summary>
        /// Gets or sets the kiosk voids.
        /// </summary>
        public string KioskVoids
        {
            get
            {
                return m_kioskVoids;
            }
            set
            {
                if (m_kioskVoids != value)
                {
                    m_kioskVoids = value;
                    decimal result = 0M;
                    if (!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.KioskVoids = result;
                    DataModified();
                    RaisePropertyChanged("KioskVoids");
                    CheckForError();
                }
            }
        }
        /// <summary>
        /// Gets the total sales value
        /// </summary>
        public string SaleTotal
        {
            get
            {
                return m_editSummary.SalesTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the total adjusted sales value
        /// </summary>
        public string SaleAdjustTotal
        {
            get
            {
                return m_editSummary.SalesAdjustedTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Prize Member Properties
        /// <summary>
        /// Gets or sets the cash prizes
        /// </summary>
        public string PrizeCash
        {
            get
            {
                return m_prizeCash;
            }
            set
            {
                if(m_prizeCash != value)
                {
                    m_prizeCash = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesCash = result;
                    DataModified();
                    RaisePropertyChanged("PrizeCash");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the check prizes
        /// </summary>
        public string PrizeCheck
        {
            get
            {
                return m_prizeCheck;
            }
            set
            {
                if(m_prizeCheck != value)
                {
                    m_prizeCheck = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesCheck = result;
                    DataModified();
                    RaisePropertyChanged("PrizeCheck");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the merchandise prizes
        /// </summary>
        public string PrizeMerchandise
        {
            get
            {
                return m_prizeMerchandise;
            }
            set
            {
                if(m_prizeMerchandise != value)
                {
                    m_prizeMerchandise = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesMerchandise = result;
                    DataModified();
                    RaisePropertyChanged("PrizeMerchandise");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the accrual increase
        /// </summary>
        public string PrizeAccrualInc
        {
            get
            {
                return m_prizeAccrualInc;
            }
            set
            {
                if(m_prizeAccrualInc != value)
                {
                    m_prizeAccrualInc = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesAccrualInc = result;
                    DataModified();
                    RaisePropertyChanged("PrizeAccrualInc");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of accrual increase transactions expected
        /// </summary>
        public int PrizeAccrualIncCountExpected
        {
            get
            {
                return m_prizeAccrualIncCountExpected;
            }
            set
            {
                if(m_prizeAccrualIncCountExpected == 0 || m_prizeAccrualIncCountExpected != value)
                {
                    m_prizeAccrualIncCountExpected = value;
                    DataModified();
                    RaisePropertyChanged("PrizeAccrualIncCountExpected");
                    RaisePropertyChanged("PrizeAccrualIncreasesPending");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of accrual increase transactions applied (exist and unvoided)
        /// </summary>
        public int PrizeAccrualIncCountApplied
        {
            get
            {
                return m_prizeAccrualIncCountApplied;
            }
            set
            {
                if(m_prizeAccrualIncCountApplied == 0 || m_prizeAccrualIncCountApplied != value)
                {
                    m_prizeAccrualIncCountApplied = value;
                    DataModified();
                    RaisePropertyChanged("PrizeAccrualIncCountApplied");
                    RaisePropertyChanged("PrizeAccrualIncreasesPending");
                    CheckForError();
                }
            }
        }


        /// <summary>
        /// Gets whether the expected number of accrual increases is different than the number of accrual increases applied 
        /// </summary>
        public bool PrizeAccrualIncreasesPending
        {
            get { return PrizeAccrualIncCountExpected != PrizeAccrualIncCountApplied; }
        }

        /// <summary>
        /// Gets or sets the pull tabl prizes
        /// </summary>
        public string PrizePullTab
        {
            get
            {
                return m_prizePullTab;
            }
            set
            {
                if(m_prizePullTab != value)
                {
                    m_prizePullTab = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesPullTab = result;
                    DataModified();
                    RaisePropertyChanged("PrizePullTab");
                    CheckForError();
                }
            }
        }

        public string PrizeOther
        {
            get
            {
                return m_prizeOther;
            }
            set
            {
                if(m_prizeOther != value)
                {
                    m_prizeOther = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesOther = result;
                    DataModified();
                    RaisePropertyChanged("PrizeOther");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets the prize total
        /// </summary>
        public string PrizeTotal
        {
            get
            {
                return m_editSummary.PrizesTotal().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the prize total accrual based
        /// </summary>
        public string PrizeTotalAccrualBased
        {
            get
            {
                return m_editSummary.PrizesTotalAccrualBased().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the prize total cash based
        /// </summary>
        public string PrizeTotalCashBased
        {
            get
            {
                return m_editSummary.PrizesTotalCashBased().ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Expected Member Properties

        /// <summary>
        /// Gets or sets the expected accrual payout
        /// </summary>
        public string ExpectAccrualPay
        {
            get
            {
                return m_expectAccrualPay;
            }
            set
            {
                if(m_expectAccrualPay != value)
                {
                    m_expectAccrualPay = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesAccrualPay = result;
                    DataModified();
                    RaisePropertyChanged("ExpectAccrualPay");
                    CheckForError();
                }
            }
        }

        public string ExpectAccrualCashPay
        {
            get
            {
                return m_expectAccrualCashPayouts;
            }

            set
            {
                if(m_expectAccrualCashPayouts != value)
                {
                    m_expectAccrualCashPayouts = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;
                    m_editSummary.AccrualCashPayouts = result;

                    DataModified();
                    RaisePropertyChanged("ExpectAccrualCashPay");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets or sets the prize fees withheld
        /// </summary>
        public string ExpectFeeWithheld
        {
            get
            {
                return m_expectFeeWithheld;
            }
            set
            {
                if(m_expectFeeWithheld != value)
                {
                    m_expectFeeWithheld = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.PrizesFees = result;
                    DataModified();
                    RaisePropertyChanged("ExpectFeeWithheld");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets the expected session cost
        /// </summary>
        public string SessionCostsRegister
        {
            get
            {
                return m_editSummary.SessionCostsRegister.ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        public string SessionCostsCombined
        {
            get
            {
                decimal temp = 0M;
                temp += m_editSummary.SessionCostsRegister;
                temp += m_editSummary.SessionCostsNonRegister;
                return temp.ToString("F2", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets or sets the expected tax
        /// </summary>
        public string ExpectTax
        {
            get
            {
                return m_expectTax;
            }
            set
            {
                if(m_expectTax != value)
                {
                    m_expectTax = value;
                    decimal result = 0M;
                    if(!decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
                        result = decimal.MinValue;

                    m_editSummary.SalesTax = result;
                    DataModified();
                    RaisePropertyChanged("ExpectTax");
                    CheckForError();
                }
            }
        }

        /// <summary>
        /// Gets the expected cash total
        /// </summary>
        public string ExpectTotal
        {
            get
            {
                if (Controller.Parent.Settings.SessionSummaryViewMode == SessionSummaryViewModes.Nevada) // US5345
                {
                    var expectedTotal = m_editSummary.ExpectedTotal(true) + Convert.ToDecimal(CashierDrops);
                    return expectedTotal.ToString("F2", CultureInfo.CurrentCulture);
                }
                else
                {
                    return m_editSummary.ExpectedTotal(false).ToString("F2", CultureInfo.CurrentCulture);
                }                
            }
        }

        #endregion

        #region Session Member Properties

        /// <summary>
        /// Gets the non register session summary costs
        /// </summary>
        public string SessionCostsNonRegister
        {
            get
            {
                return m_editSummary.SessionCostsNonRegister.ToString("F2", CultureInfo.CurrentCulture);
            }
        }
        #endregion

        #region ICommand Member Properties
        /// <summary>
        /// Gets the command to a a caller to the choosen caller list
        /// </summary>
        public ICommand AddCallerCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to remove a caller from the choosen caller list
        /// </summary>
        public ICommand DeleteCallerCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to edit session costs
        /// </summary>
        public ICommand EditSessionCostCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to pop up the "Actual Cash Calculator"
        /// </summary>
        public ICommand ActualCashCalculatorCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to calculate the session data
        /// </summary>
        public ICommand CalculateCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to apply accrual increases
        /// </summary>
        public ICommand TriggerAcc2IncreasesCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to pop up the "Staff Over/Short" window
        /// </summary>
        public ICommand DisplayStaffOverShortCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to generate a session summary report
        /// </summary>
        public ICommand GenerateCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to save a session summary
        /// </summary>
        public ICommand SaveCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to clear edited data
        /// </summary>
        public ICommand ResetCommand
        {
            get;
            private set;
        }

        public bool AllowModifications
        {
            get
            {
                return m_allowModifications;
            }

            private set
            {
                m_allowModifications = value;
                RaisePropertyChanged("AllowModifications");
            }
        }
        #endregion

        #endregion
    }
}
