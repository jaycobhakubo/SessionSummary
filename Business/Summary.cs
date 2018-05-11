#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2012 GameTech International, Inc.
//
// DE12622 Progressive prizes paid out by check are being included in the expected
//  cash calculation causing the expected cash to be understated.
#endregion

using System;
using System.Collections.Generic;
using GameTech.Elite.Base;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Represents a session summary
    /// </summary>
    internal class Summary : Notifier, ICloneable
    {
        #region Member Variables
        DateTime m_sessionDate;
        short m_sessionNumber;
        int m_attendanceSystem;
        DateTime m_attedanceSystemTime;
        int m_attendanceManual;
        DateTime m_attendanceManualTime;
        Staff m_manager;
        List<Staff> m_callers;
        string m_comments;
        List<Currency> m_availableCurrencies;
        List<ActualCashCurrencyDenom> m_actualCashDenomsSet;

        // Bank beginning and ending
        decimal m_bankBegin;
        decimal m_bankFill;
        decimal m_bankEnd;
        List<SessionCost> m_sessionCosts;
        // Sales
        decimal m_salesPaper;
        decimal m_salesElectronic;
        decimal m_salesBingoOther; //FIX: DE8961 Session summary does calculate bingo other sales
        decimal m_salesPullTab;
        decimal m_salesConcession;
        decimal m_salesMerchandise;
        decimal m_salesValidation;
        decimal m_salesDeviceFee;
        decimal m_salesDiscount;
        decimal m_salesTax;

        decimal m_kioskSales;
        decimal m_kioskVoids;

        // Prizes
        decimal m_prizesCash;
        decimal m_prizesCheck;
        decimal m_prizesMerchandise;
        decimal m_prizesAccrualInc;
        int m_prizesAccrualIncCountExpected;
        int m_prizesAccrualIncCountApplied;
        decimal m_prizesPullTab;
        decimal m_prizesOther;
        decimal m_prizesAccrualPay;
        decimal m_accrualCashPayouts;
        decimal m_prizesFees;

        // Over/short values
        decimal m_overCash;
        decimal m_overDebitCredit;
        decimal m_overChecks;
        decimal m_overMoneyOrders;
        decimal m_overCoupons;
        decimal m_overGiftCards;
        decimal m_overChips;
       
        #endregion

        #region Constructors
        /// <summary>
        /// Intitializes a Summary object
        /// </summary>
        public Summary()
        {
            AttendanceSystemTime = DateTime.Now;
            AttendanceManualTime = DateTime.Now;

            m_actualCashDenomsSet = new List<ActualCashCurrencyDenom>();
            m_availableCurrencies = new List<Currency>();
            Callers = new List<Staff>();
            SessionCosts = new List<SessionCost>();            
        }

        /// <summary>
        /// Intializes a Summary object with the specified gaming date and session number
        /// </summary>
        /// <param name="date">The gaming date of the session.</param>
        /// <param name="session">The session number of the session.</param>
        public Summary(DateTime date, short session)
        {
            SessionDate = date;
            SessionNumber = session;

            AttendanceSystemTime = DateTime.Now;
            AttendanceManualTime = DateTime.Now;

            m_actualCashDenomsSet = new List<ActualCashCurrencyDenom>();
            m_availableCurrencies = new List<Currency>();
            Callers = new List<Staff>();
            SessionCosts = new List<SessionCost>();            
        }

        /// <summary>
        /// Initalizes a Summary object from another
        /// </summary>
        /// <param name="src">The Summary object to copy.</param>
        public Summary(Summary src)
        {
            if (src == null)
                throw new ArgumentNullException("src");

            SessionDate = src.SessionDate;
            SessionNumber = src.SessionNumber;

            AttendanceSystem = src.AttendanceSystem;
            AttendanceSystemTime = src.AttendanceSystemTime;
            AttendanceManual = src.AttendanceManual;
            AttendanceManualTime = src.AttendanceManualTime;

            Manager = src.Manager;
            Callers = new List<Staff>(src.Callers);
            Comments = src.Comments;

            BankBegin = src.BankBegin;
            BankFill = src.BankFill;
            BankEnd = src.BankEnd;

            List<SessionCost> costs = new List<SessionCost>();
            foreach (SessionCost cost in src.SessionCosts)
            {
                SessionCost newCost = (SessionCost)cost.Clone();
                costs.Add(newCost);
            }
            SessionCosts = costs;

            // Copy sales information
            SalesPaper = src.SalesPaper;
            SalesElectronic = src.SalesElectronic;
            SalesBingoOther = src.SalesBingoOther; //FIX: DE8961 Session summary does calculate bingo other sales
            SalesPullTab = src.SalesPullTab;
            SalesConcession = src.SalesConcession;
            SalesMerchandise = src.SalesMerchandise;
            SalesDeviceFee = src.SalesDeviceFee;
            SalesDiscount = src.SalesDiscount;
            SalesValidation = src.SalesValidation;
            SalesTax = src.SalesTax;

            KioskSale = src.KioskSale;
            KioskVoids = src.KioskVoids;

            // Copy prize information
            PrizesCash = src.PrizesCash;
            PrizesCheck = src.PrizesCheck;
            PrizesMerchandise = src.PrizesMerchandise;
            PrizesPullTab = src.PrizesPullTab;
            PrizesOther = src.PrizesOther;
            PrizesAccrualInc = src.PrizesAccrualInc;
            PrizesAccrualIncCountExpected = src.PrizesAccrualIncCountExpected;
            PrizesAccrualIncCountApplied = src.PrizesAccrualIncCountApplied;
            PrizesAccrualPay = src.PrizesAccrualPay;
            PrizesFees = src.PrizesFees;

            OverCash = src.OverCash;
            OverDebitCredit = src.OverDebitCredit;
            OverChecks = src.OverChecks;
            OverMoneyOrders = src.OverMoneyOrders;
            OverCoupons = src.OverCoupons;
            OverGiftCards = src.OverGiftCards;
            OverChips = src.OverChips;
            AccrualCashPayouts = src.AccrualCashPayouts;

            List<Currency> curr = new List<Currency>();
            foreach (var srcCur in src.m_availableCurrencies)
            {
                Currency newCur = (Currency)srcCur.Clone();
                curr.Add(newCur);
            }
            AvailableCurrencies = curr;

            List<ActualCashCurrencyDenom> denoms = new List<ActualCashCurrencyDenom>();
            foreach (var srcDenom in src.m_actualCashDenomsSet)
	        {
                ActualCashCurrencyDenom newDenom = (ActualCashCurrencyDenom)srcDenom.Clone();
                denoms.Add(newDenom);
	        }
            ActualCashDenoms = denoms;
        }
        #endregion

        #region Member Methods

        /// <summary>
        /// Loads our available currencies from the passed set.
        /// </summary>
        /// <param name="currencies"></param>
        public void LoadAvailableCurrencies(IEnumerable<Currency> currencies)
        {
            List<Currency> curr = new List<Currency>();
            foreach (var srcCur in currencies)
            {
                Currency newCur = (Currency)srcCur.Clone();
                curr.Add(newCur);
            }
            AvailableCurrencies = curr;
        }

        /// <summary>
        /// Loads the "Actual Cash Denoms" with the quantities of items known,
        /// but does not remove empty denoms already defined.
        /// </summary>
        /// <param name="actualCashDenoms"></param>
        public void LoadActualCashDenomQuantities(List<ActualCashCurrencyDenom> actualCashDenoms)
        {
            List<ActualCashCurrencyDenom> denoms = new List<ActualCashCurrencyDenom>();
            foreach (var srcDenom in actualCashDenoms)
            {
                ActualCashCurrencyDenom newDenom = (ActualCashCurrencyDenom)srcDenom.Clone();
                denoms.Add(newDenom);
            }
            ActualCashDenoms = denoms;
        }

        /// <summary>
        /// Reloads the "Actual Cash" from Denoms
        /// </summary>
        internal void ReloadActualCashFromDenoms()
        {
            decimal overCash = 0.0M;

            foreach (var srcDenom in ActualCashDenoms)
            {
                overCash += srcDenom.DefaultValue;
            }

            OverCash = overCash;
        }

        /// <summary>
        /// Creates a copy of the Session Summary object and returns the copy
        /// </summary>
        /// <returns>The copy of the object.</returns>
        public object Clone()
        {
            Summary clone = new Summary(this);
            return clone;
        }

        /// <summary>
        /// Calculates the bingo sales total
        /// </summary>
        /// <returns></returns>
        public decimal SalesBingoTotal()
        {
            decimal total = 0M;

            try
            {
                if (SalesPaper != decimal.MinValue)
                    total += SalesPaper;

                if (SalesElectronic != decimal.MinValue)
                    total += SalesElectronic;

                //FIX: DE8961 Session summary does calculate bingo other sales
                if (SalesBingoOther != decimal.MinValue)
                    total += SalesBingoOther;
                //END: DE8961
            }
            catch (Exception)
            {
                // Calculation Overflow problem
                total = decimal.MinValue;
            }

            return total;
        }

        public decimal SalesAdjustedTotal()
        {
            decimal total = 0M;

            try
            {
                decimal salesTotal = SalesTotal();
                if (salesTotal != decimal.MinValue)
                    total += salesTotal;

                if (SalesTax != decimal.MinValue)
                    total -= SalesTax;

                if (SalesDeviceFee != decimal.MinValue)
                    total -= SalesDeviceFee;
            }
            catch (Exception)
            {
                // Calculation overflow problem
                total = decimal.MinValue;
            }

            return total;
        }


        /// <summary>
        /// Calculates the sales total.
        /// </summary>
        /// <returns>The sales total.</returns>
        public decimal SalesTotal()
        {
            decimal total = 0M;

            try
            {
                decimal salesBingoTotal = SalesBingoTotal();
                if (salesBingoTotal != decimal.MinValue)
                    total += salesBingoTotal;

                if (SalesPullTab != decimal.MinValue)
                    total += SalesPullTab;

                if (SalesConcession != decimal.MinValue)
                    total += SalesConcession;

                if (SalesValidation != decimal.MinValue)
                    total += SalesValidation;

                if (SalesMerchandise != decimal.MinValue)
                    total += SalesMerchandise;

                if (SalesDeviceFee != decimal.MinValue)
                    total += SalesDeviceFee;

                if (SalesDiscount != decimal.MinValue)
                    total -= SalesDiscount;

                if (SalesTax != decimal.MinValue)
                    total += SalesTax;

            }
            catch (Exception)
            {
                // Calculation overflow problem
                total = decimal.MinValue;
            }

            return total;
        }

        /// <summary>
        /// Calculates the prize total.
        /// </summary>
        /// <returns>The prize total.</returns>
        public decimal PrizesTotal()
        {
            decimal total = 0M;

            try
            {
                if (PrizesCash != decimal.MinValue)
                    total += PrizesCash;

                if (PrizesCheck != decimal.MinValue)
                    total += PrizesCheck;

                if (PrizesMerchandise != decimal.MinValue)
                    total += PrizesMerchandise;

                if (PrizesPullTab != decimal.MinValue)
                    total += PrizesPullTab;

                if (PrizesOther != decimal.MinValue)
                    total += PrizesOther;

            }
            catch (Exception)
            {
                // Calculation overflow problem
                total = decimal.MinValue;
            }

            return total;
        }

        /// <summary>
        /// Calculates the prize total for accrual based.
        /// </summary>
        /// <returns>The total accrual based prizes.</returns>
        public decimal PrizesTotalAccrualBased()
        {
            decimal total = 0M;

            try
            {
                decimal prizesTotal = PrizesTotal();
                if (prizesTotal != decimal.MinValue)
                    total += prizesTotal;

                if (PrizesAccrualInc != decimal.MinValue)
                    total += PrizesAccrualInc;
            }
            catch (Exception)
            {
                // Calculation overflow problem
                total = decimal.MinValue;
            }

            return total;
        }

        /// <summary>
        /// Calculates the prize total for cash based.
        /// </summary>
        /// <returns>The total cash based prizes.</returns>
        public decimal PrizesTotalCashBased()
        {
            decimal total = 0M;

            try
            {
                decimal prizesTotal = PrizesTotal();
                if (prizesTotal != decimal.MinValue)
                    total += prizesTotal;

                if (PrizesAccrualPay != decimal.MinValue)
                    total += PrizesAccrualPay;

            }
            catch (Exception)
            {
                // Calculation overflow problem
                total = decimal.MinValue;
            }

            return total;
        }

        /// <summary>
        /// Calculates the expected cash total
        /// </summary>
        /// <returns>The expected cash total.</returns>
        public decimal ExpectedTotal(bool isNevada)
        {
            decimal total = 0M;

            try
            {
                if (BankBegin != decimal.MinValue)
                    total += BankBegin;

                if (BankFill != decimal.MinValue)
                    total += BankFill;

                if (!isNevada)
                {
                    decimal salesTotal = SalesTotal();
                    if (salesTotal != decimal.MinValue)
                        total += salesTotal;

                    if (KioskSale != decimal.MinValue)
                        total -= KioskSale;

                    if (KioskVoids != decimal.MinValue)
                        total -= KioskVoids;
                }

                if (PrizesCash != decimal.MinValue)
                    total -= PrizesCash;

                // DE12622
                if (AccrualCashPayouts != decimal.MinValue)
                    total -= AccrualCashPayouts;

                if (PrizesFees != decimal.MinValue)
                    total += PrizesFees;

                if (PrizesPullTab != decimal.MinValue)
                    total -= PrizesPullTab;

                if (SessionCostsRegister != decimal.MinValue)
                    total -= SessionCostsRegister;

                if (SessionCostsNonRegister != decimal.MinValue)
                    total -= SessionCostsNonRegister;

            }
            catch (Exception)
            {
                // Calculation overflow error
                total = decimal.MinValue;
            }

            return total;
        }

        public decimal WinTotalAccrualBased()
        {
            decimal total = 0M;

            try
            {
                decimal totalSalesAdjusted = SalesAdjustedTotal();
                if (totalSalesAdjusted != decimal.MinValue)
                    total += totalSalesAdjusted;

                decimal totalPrizesAccrualBased = PrizesTotalAccrualBased();
                if (totalPrizesAccrualBased != decimal.MinValue)
                    total -= totalPrizesAccrualBased;
            }
            catch (Exception)
            {
                // Calculation overflow error
                total = decimal.MinValue;
            }

            return total;
        }

        public decimal WinTotalCashBased()
        {
            decimal total = 0M;

            try
            {
                decimal totalSalesAdjusted = SalesAdjustedTotal();
                if (totalSalesAdjusted != decimal.MinValue)
                    total += totalSalesAdjusted;

                decimal totalPrizesCashBased = PrizesTotalCashBased();
                if (totalPrizesCashBased != decimal.MinValue)
                    total -= totalPrizesCashBased;
            }
            catch (Exception)
            {
                // Calculation overflow error
                total = decimal.MinValue;
            }

            return total;
        }

        /// <summary>
        /// Calculates the Over/Short total.
        /// </summary>
        /// <returns>The over/short total.</returns>
        public decimal OverTotal(bool isNevada)
        {
            decimal total = 0M;

            try
            {
                if (OverCash != decimal.MinValue)
                    total += OverCash;

                if (OverDebitCredit != decimal.MinValue)
                    total += OverDebitCredit;

                if (OverChecks != decimal.MinValue)
                    total += OverChecks;

                if (OverMoneyOrders != decimal.MinValue)
                    total += OverMoneyOrders;

                if (OverCoupons != decimal.MinValue)
                    total += OverCoupons;

                if (OverGiftCards != decimal.MinValue)
                    total += OverGiftCards;
                
                if (OverChips != decimal.MinValue)
                    total += OverChips;

                decimal expectedTotal = ExpectedTotal(isNevada);
                if (expectedTotal != decimal.MinValue)
                    total -= expectedTotal;
            }
            catch (Exception)
            {
                // Calculation overflow errors
                total = decimal.MinValue;
            }

            return total;
        }

        /// <summary>
        /// Calculates the deposit amount.
        /// </summary>
        /// <returns>The deposit amount.</returns>
        public decimal DepositTotal(bool isNevada)
        {
            decimal total = 0M;

            try
            {
                decimal expectedTotal = ExpectedTotal(isNevada);
                if (expectedTotal != decimal.MinValue)
                    total += expectedTotal;

                decimal overTotal = OverTotal(isNevada);
                if (overTotal != decimal.MinValue)
                    total += overTotal;

                if (BankEnd != decimal.MinValue)
                    total -= BankEnd;
            }
            catch (Exception)
            {
                // Calulation overflow problems
                total = decimal.MinValue;
            }

            return total;
        }

        /// <summary>
        /// Calculates the total spent per player.
        /// </summary>
        /// <returns>The total spent per player</returns>
        public decimal SpendTotal()
        {
            decimal total = 0M;
            decimal salesTotal = SalesAdjustedTotal();

            int attendance = AttendanceManual;
            if (attendance != 0 && salesTotal != decimal.MinValue)
                total = salesTotal / attendance;

            return total;
        }

        /// <summary>
        /// Calculates the merchandise spent per player.
        /// </summary>
        /// <returns>The total merchandise spent per player.</returns>
        public decimal SpendMerchandiseTotal()
        {
            decimal total = 0;

            int attendance = AttendanceManual;
            if (attendance != 0 && SalesMerchandise != decimal.MinValue)
                total = SalesMerchandise / attendance;

            return total;
        }

        /// <summary>
        /// Calculates the concessions spent per player.
        /// </summary>
        /// <returns>The concessions spent per player.</returns>
        public decimal SpendConcessionTotal()
        {
            decimal total = 0;

            int attendance = AttendanceManual;
            if (attendance != 0 && SalesConcession != decimal.MinValue)
                total = SalesConcession / attendance;

            return total;
        }

        /// <summary>
        /// Calculates the bingo spent per player.
        /// </summary>
        /// <returns>The bingo spent per player.</returns>
        public decimal SpendBingoTotal()
        {
            decimal total = 0;
            decimal salesBingoTotal = SalesBingoTotal();

            int attendance = AttendanceManual;
            if (attendance != 0 && salesBingoTotal != decimal.MinValue)
                total = salesBingoTotal / attendance;

            return total;
        }

        /// <summary>
        /// Calculates the pull tab spent per player.
        /// </summary>
        /// <returns>The pull tab spent per player.</returns>
        public decimal SpendPullTabTotal()
        {
            decimal total = 0;

            int attendance = AttendanceManual;
            if (attendance != 0 && SalesPullTab != decimal.MinValue)
                total = (SalesPullTab) / attendance;

            return total;
        }

        /// <summary>
        /// Calculates the percentage of payout accrual based.
        /// </summary>
        /// <returns>The percentage of bingo payout.</returns>
        public decimal SpendPercentPayAccrualBased()
        {
            decimal total = 0;

            decimal sales = SalesAdjustedTotal();
            decimal prizes = PrizesTotalAccrualBased();
            if (sales != 0 && sales != decimal.MinValue && prizes != decimal.MinValue)
                total = (prizes / sales);

            return total;
        }

        /// <summary>
        /// Calculates the percentage of bingo payout.
        /// </summary>
        /// <returns>The percentage of bingo payout.</returns>
        public decimal SpendPercentPayCashBased()
        {
            decimal total = 0;

            decimal sales = SalesAdjustedTotal();
            decimal prizes = PrizesTotalCashBased();
            if (sales != 0 && sales != decimal.MinValue && prizes != decimal.MinValue)
                total = (prizes / sales);

            return total;
        }

        /// <summary>
        /// Calculates the percentage of hold accrual based.
        /// </summary>
        /// <returns>The percentage of bingo hold.</returns>
        public decimal SpendPercentHoldAccrualBased()
        {
            decimal total = 0;

            decimal sales = SalesAdjustedTotal();
            decimal win = WinTotalAccrualBased();
            if (sales != 0 && sales != decimal.MinValue && win != decimal.MinValue)
                total = (win / sales);

            return total;
        }

        /// <summary>
        /// Calculates the percentage of hold cash based.
        /// </summary>
        /// <returns>The percentage of bingo hold.</returns>
        public decimal SpendPercentHoldCashBased()
        {
            decimal total = 0;

            try
            {
                decimal sales = SalesAdjustedTotal();
                decimal win = WinTotalCashBased();
                if (sales != 0 && sales != decimal.MinValue && win != decimal.MinValue)
                    total = (win / sales);
            }
            catch (Exception)
            {
            }

            return total;
        }
        
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the date of the session summary
        /// </summary>
        public DateTime SessionDate
        {
            get
            {
                return m_sessionDate;
            }
            set
            {
                if (m_sessionDate != value)
                {
                    m_sessionDate = value;
                    RaisePropertyChanged("SessionDate");
                }
            }
        }

        /// <summary>
        /// Gets or sets the session number of the session summary
        /// </summary>
        public short SessionNumber
        {
            get
            {
                return m_sessionNumber;
            }
            set
            {
                if (m_sessionNumber != value)
                {
                    m_sessionNumber = value;
                    RaisePropertyChanged("SessionNumber");
                }
            }
        }

        /// <summary>
        /// Gets or sets the manager for the summary.
        /// </summary>
        public Staff Manager
        {
            get
            {
                return m_manager;
            }
            set
            {
                if (m_manager != value)
                {
                    m_manager = value;
                    RaisePropertyChanged("Manager");
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of callers for the summary.
        /// </summary>
        public IList<Staff> Callers
        {
            get
            {
                return m_callers;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Callers value");
                else
                {
                    m_callers = new List<Staff>(value);
                    RaisePropertyChanged("Callers");
                }
            }
        }

        /// <summary>
        /// The actual cash denoms set
        /// </summary>
        public IList<ActualCashCurrencyDenom> ActualCashDenoms
        {
            get
            {
                return m_actualCashDenomsSet;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Actual Cash Denoms");
                else
                {
                    m_actualCashDenomsSet = new List<ActualCashCurrencyDenom>(value);
                    RaisePropertyChanged("ActualCashDenoms");
                    RaisePropertyChanged("AvailableCurrencies");
                }
            }
        }

        /// <summary>
        /// The currently available currencies in the system
        /// </summary>
        public IList<Currency> AvailableCurrencies 
        {
            get
            {
                return m_availableCurrencies;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Available Currencies");
                else
                {
                    m_availableCurrencies = new List<Currency>(value);
                    RaisePropertyChanged("AvailableCurrencies");
                    RaisePropertyChanged("ActualCashDenoms");
                }
            }
        }

        /// <summary>
        /// Gets or sets the comments
        /// </summary>
        public string Comments
        {
            get
            {
                return m_comments;
            }
            set
            {
                if (m_comments != value)
                {
                    m_comments = value;
                    RaisePropertyChanged("Comments");
                }
            }
        }

        /// <summary>
        /// Gets or sets the beginning bank.
        /// </summary>
        public decimal BankBegin
        {
            get
            {
                return m_bankBegin;
            }
            set
            {
                if (m_bankBegin != value)
                {
                    m_bankBegin = value;
                    RaisePropertyChanged("BankBegin");
                }
            }
        }

        /// <summary>
        /// Gets or sets the bank fill amount
        /// </summary>
        public decimal BankFill 
        {
            get
            {
                return m_bankFill;
            }
            set
            {
                if (m_bankFill != value)
                {
                    m_bankFill = value;
                    RaisePropertyChanged("BankFill");
                }
            }
        }

        /// <summary>
        /// Gets or sets the ending bank.
        /// </summary>
        public decimal BankEnd
        {
            get
            {
                return m_bankEnd;
            }
            set
            {
                if (m_bankEnd != value)
                {
                    m_bankEnd = value;
                    RaisePropertyChanged("BankEnd");
                }
            }
        }

        /// <summary>
        /// Gets or sets the session costs.
        /// </summary>
        public decimal SessionCostsNonRegister
        {
            get
            {
                decimal total = 0M;
                foreach (SessionCost cost in SessionCosts)
                {
                    if (!cost.IsRegister)
                        total += cost.Value;
                }

                return total;
            }
        }

        /// <summary>
        /// Gets or sets the list of session costs.
        /// </summary>
        public IEnumerable<SessionCost> SessionCosts
        {
            get
            {
                return m_sessionCosts;
            }
            set
            {
                m_sessionCosts = new List<SessionCost>(value);
                RaisePropertyChanged("SessionCosts");
                RaisePropertyChanged("SessionCostsRegister");
                RaisePropertyChanged("SessionCostsNonRegister");
            }
        }

        #region Sales Properties
        /// <summary>
        /// Gets or paper sales.
        /// </summary>
        public decimal SalesPaper
        {
            get
            {
                return m_salesPaper;
            }
            set
            {
                if (m_salesPaper != value)
                {
                    m_salesPaper = value;
                    RaisePropertyChanged("SalesPaper");
                }
            }
        }

        /// <summary>
        /// Gets or sets electronic sales.
        /// </summary>
        public decimal SalesElectronic
        {
            get
            {
                return m_salesElectronic;
            }
            set
            {
                if (m_salesElectronic != value)
                {
                    m_salesElectronic = value;
                    RaisePropertyChanged("SalesElectronic");
                }
            }
        }

        //FIX: DE8961 Session summary does calculate bingo other sales
        /// <summary>
        /// Gets or sets bingo other sales.
        /// </summary>
        public decimal SalesBingoOther
        {
            get
            {
                return m_salesBingoOther;
            }
            set
            {
                if (m_salesBingoOther != value)
                {
                    m_salesBingoOther = value;
                    RaisePropertyChanged("SalesBingoOther");
                }
            }
        }
        //END: DE8961

        /// <summary>
        /// Gets or sets pull tab sales.
        /// </summary>
        public decimal SalesPullTab
        {
            get
            {
                return m_salesPullTab;
            }
            set
            {
                if (m_salesPullTab != value)
                {
                    m_salesPullTab = value;
                    RaisePropertyChanged("SalesPullTab");
                }
            }
        }

        /// <summary>
        /// Gets or sets concession sales.
        /// </summary>
        public decimal SalesConcession
        {
            get
            {
                return m_salesConcession;
            }
            set
            {
                if (m_salesConcession != value)
                {
                    m_salesConcession = value;
                    RaisePropertyChanged("SalesConcession");
                }
            }
        }

        /// <summary>
        /// Gets or sets merchandise sales.
        /// </summary>
        public decimal SalesMerchandise
        {
            get
            {
                return m_salesMerchandise;
            }
            set
            {
                if (m_salesMerchandise != value)
                {
                    m_salesMerchandise = value;
                    RaisePropertyChanged("SalesMerchandise");
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation sales.
        /// </summary>
        public decimal SalesValidation
        {
            get
            {
                return m_salesValidation;
            }
            set
            {
                if (m_salesValidation != value)
                {
                    m_salesValidation = value;
                    RaisePropertyChanged("SalesValidation");
                }
            }
        }

        /// <summary>
        /// Gets or sets device fees.
        /// </summary>
        public decimal SalesDeviceFee
        {
            get
            {
                return m_salesDeviceFee;
            }
            set
            {
                if (m_salesDeviceFee != value)
                {
                    m_salesDeviceFee = value;
                    RaisePropertyChanged("SalesDeviceFee");
                }
            }
        }

        /// <summary>
        /// Gets or sets discounts.
        /// </summary>
        public decimal SalesDiscount
        {
            get
            {
                return m_salesDiscount;
            }
            set
            {
                if (m_salesDiscount != value)
                {
                    m_salesDiscount = value;
                    RaisePropertyChanged("SalesDiscount");
                }
            }
        }

        /// <summary>
        /// Gets or sets the tax.
        /// </summary>
        public decimal SalesTax
        {
            get
            {
                return m_salesTax;
            }
            set
            {
                if (m_salesTax != value)
                {
                    m_salesTax = value;
                    RaisePropertyChanged("SalesTax");
                }
            }
        }

        /// <summary>
        /// Gets or sets the kiosk sales.
        /// </summary>
        public decimal KioskSale
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
                    RaisePropertyChanged("KioskSale");
                }
            }
        }

        //US5352 - kiosk voids
        /// <summary>
        /// Gets or sets the kiosk sales.
        /// </summary>
        public decimal KioskVoids
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
                    RaisePropertyChanged("KioskVoids");
                }
            }
        }
        #endregion

        #region Prizes Properties
        /// <summary>
        /// Gets or sets cash prizes.
        /// </summary>
        public decimal PrizesCash
        {
            get
            {
                return m_prizesCash;
            }
            set
            {
                if (m_prizesCash != value)
                {
                    m_prizesCash = value;
                    RaisePropertyChanged("PrizesCash");
                }
            }
        }

        /// <summary>
        /// Gets or sets check prizes.
        /// </summary>
        public decimal PrizesCheck
        {
            get
            {
                return m_prizesCheck;
            }
            set
            {
                if (m_prizesCheck != value)
                {
                    m_prizesCheck = value;
                    RaisePropertyChanged("PrizesCheck");
                }
            }
        }

        /// <summary>
        /// Gets or sets merchandise prizes.
        /// </summary>
        public decimal PrizesMerchandise
        {
            get
            {
                return m_prizesMerchandise;
            }
            set
            {
                if (m_prizesMerchandise != value)
                {
                    m_prizesMerchandise = value;
                    RaisePropertyChanged("PrizesMerchandise");
                }
            }
        }

        /// <summary>
        /// Gets or sets accrual increase.
        /// </summary>
        public decimal PrizesAccrualInc
        {
            get
            {
                return m_prizesAccrualInc;
            }
            set
            {
                if (m_prizesAccrualInc != value)
                {
                    m_prizesAccrualInc = value;
                    RaisePropertyChanged("PrizesAccrualInc");
                }
            }
        }

        public int PrizesAccrualIncCountExpected
        {
            get
            {
                return m_prizesAccrualIncCountExpected;
            }
            set
            {
                if(m_prizesAccrualIncCountExpected != value)
                {
                    m_prizesAccrualIncCountExpected = value;
                    RaisePropertyChanged("PrizesAccrualIncCountExpected");
                }
            }
        }

        public int PrizesAccrualIncCountApplied
        {
            get
            {
                return m_prizesAccrualIncCountApplied;
            }
            set
            {
                if(m_prizesAccrualIncCountApplied != value)
                {
                    m_prizesAccrualIncCountApplied = value;
                    RaisePropertyChanged("PrizesAccrualIncCount");
                }
            }
        }

        /// <summary>
        /// Gets or sets pull tab prizes.
        /// </summary>
        public decimal PrizesPullTab
        {
            get
            {
                return m_prizesPullTab;
            }
            set
            {
                if (m_prizesPullTab != value)
                {
                    m_prizesPullTab = value;
                    RaisePropertyChanged("PrizesPullTab");
                }
            }
        }

        public decimal PrizesOther
        {
            get
            {
                return m_prizesOther;
            }
            set
            {
                if (m_prizesOther != value)
                {
                    m_prizesOther = value;
                    RaisePropertyChanged("PrizesOther");
                }
            }
        }

        #endregion

        #region Expected Cash Properties

        /// <summary>
        /// Gets or sets accrual payouts.
        /// </summary>
        public decimal PrizesAccrualPay
        {
            get
            {
                return m_prizesAccrualPay;
            }
            set
            {
                if (m_prizesAccrualPay != value)
                {
                    m_prizesAccrualPay = value;
                    RaisePropertyChanged("PrizesAccrualPay");
                }
            }
        }

        
        /// <summary>
        /// Gets or sets prize fees withheld
        /// </summary>
        public decimal PrizesFees
        {
            get
            {
                return m_prizesFees;
            }
            set
            {
                if (m_prizesFees != value)
                {
                    m_prizesFees = value;
                    RaisePropertyChanged("PrizesFees");
                }
            }
        }

        /// <summary>
        /// Gets or sets the cash session costs.
        /// </summary>
        public decimal SessionCostsRegister
        {
            get
            {
                decimal total = 0M;
                foreach (SessionCost cost in SessionCosts)
                {
                    if (cost.IsRegister)
                        total += cost.Value;
                }

                return total;
            }
        }

        #endregion

        #region Over Cash Properties
        /// <summary>
        /// Gets or sets the actual cash.
        /// </summary>
        public decimal OverCash
        {
            get
            {
                return m_overCash;
            }
            set
            {
                if (m_overCash != value)
                {
                    m_overCash = value;
                    RaisePropertyChanged("OverCash");
                }
            }
        }

        /// <summary>
        /// Gets or sets the debit/credit.
        /// </summary>
        public decimal OverDebitCredit
        {
            get
            {
                return m_overDebitCredit;
            }
            set
            {
                if (m_overDebitCredit != value)
                {
                    m_overDebitCredit = value;
                    RaisePropertyChanged("OverDebitCredit");
                }
            }
        }

        /// <summary>
        /// Gets or sets the checks.
        /// </summary>
        public decimal OverChecks
        {
            get
            {
                return m_overChecks;
            }
            set
            {
                if (m_overChecks != value)
                {
                    m_overChecks = value;
                    RaisePropertyChanged("OverChecks");
                }
            }
        }

        /// <summary>
        /// Gets or sets the money orders.
        /// </summary>
        public decimal OverMoneyOrders
        {
            get
            {
                return m_overMoneyOrders;
            }
            set
            {
                if (m_overMoneyOrders != value)
                {
                    m_overMoneyOrders = value;
                    RaisePropertyChanged("OverMoneyOrders");
                }
            }
        }

        /// <summary>
        /// Gets or sets the coupons.
        /// </summary>
        public decimal OverCoupons
        {
            get
            {
                return m_overCoupons;
            }
            set
            {
                if (m_overCoupons != value)
                {
                    m_overCoupons = value;
                    RaisePropertyChanged("OverCoupons");
                }
            }
        }

        /// <summary>
        /// Gets or sets the gift cards.
        /// </summary>
        public decimal OverGiftCards
        {
            get
            {
                return m_overGiftCards;
            }
            set
            {
                if (m_overGiftCards != value)
                {
                    m_overGiftCards = value;
                    RaisePropertyChanged("OverGiftCards");
                }
            }
        }

        /// <summary>
        /// Gets or sets the chips.
        /// </summary>
        public decimal OverChips
        {
            get
            {
                return m_overChips;
            }
            set
            {
                if (m_overChips != value)
                {
                    m_overChips = value;
                    RaisePropertyChanged("OverChips");
                }
            }
        }
        /// <summary>
        /// The accrual cash payouts
        /// </summary>
        public decimal AccrualCashPayouts
        {
            get
            {
                return m_accrualCashPayouts;
            }

            set
            {
                if (m_accrualCashPayouts != value)
                {
                    m_accrualCashPayouts = value;
                    RaisePropertyChanged("AccrualCashPayouts");
                }
            }
        }
       
        #endregion

        #region Spend Calculation Properties
        /// <summary>
        /// Gets or set the system attendance count
        /// </summary>
        public int AttendanceSystem
        {
            get
            {
                return m_attendanceSystem;
            }
            set
            {
                if (m_attendanceSystem != value)
                {
                    m_attendanceSystem = value;
                    RaisePropertyChanged("AttendanceSystem");
                }
            }
        }

        /// <summary>
        /// Gets or sets the attendance system time
        /// </summary>
        public DateTime AttendanceSystemTime
        {
            get
            {
                return m_attedanceSystemTime;
            }
            set
            {
                if (m_attedanceSystemTime != value)
                {
                    m_attedanceSystemTime = value;
                    RaisePropertyChanged("AttendanceSystemTime");
                }
            }
        }

        /// <summary>
        /// Gets or sets the manual attendance count
        /// </summary>
        public int AttendanceManual
        {
            get
            {
                return m_attendanceManual;
            }
            set
            {
                if (m_attendanceManual != value)
                {
                    m_attendanceManual = value;
                    RaisePropertyChanged("AttendanceManual");
                }
            }
        }

        /// <summary>
        /// Gets or sets the time the manual attendance count was taken.
        /// </summary>
        public DateTime AttendanceManualTime
        {
            get
            {
                return m_attendanceManualTime;
            }
            set
            {
                if (m_attendanceManualTime != value)
                {
                    m_attendanceManualTime = value;
                    RaisePropertyChanged("AttendanceManualTime");
                }
            }
        }
        #endregion

        #endregion
    }
}
