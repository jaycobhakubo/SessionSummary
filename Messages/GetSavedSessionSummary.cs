#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Business;

namespace GameTech.Elite.Client.Modules.SessionSummary.Messages
{
    /// <summary>
    /// Message class for getting a saved session summary
    /// </summary>
    internal class GetSavedSessionSummary : ServerMessage
    {
        #region Member Variables
        private DateTime m_date;
        private short m_session;
        private IEnumerable<Staff> m_staff;
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a session summary message
        /// </summary>
        /// <param name="date">The date of the session.</param>
        /// <param name="session">The number of the session on the specified date.</param>
        public GetSavedSessionSummary(DateTime date, short session, IEnumerable<Staff> staff)
        {
            if (staff == null)
                throw new ArgumentNullException("staff");

            m_date = date;
            m_session = session;
            m_staff = staff;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Packs a request to sent to the server.
        /// </summary>
        /// <param name="requestWriter">The request.</param>
        protected override void PackRequest(BinaryWriter requestWriter)
        {
            WriteDateTime(requestWriter, m_date);
            requestWriter.Write(m_session);
        }

        /// <summary>
        /// Unpacks the request reponse received from the server.
        /// </summary>
        /// <param name="responseReader">The response.</param>
        protected override void UnpackResponse(BinaryReader responseReader)
        {
            if (responseReader == null)
                throw new ArgumentNullException("responseReader");

            if (ReturnCode == ServerReturnCode.Success)
            {
                // Create a new summary with the passed in date and session
                Summary summary = new Summary(m_date, m_session);
                // Attendance
                summary.AttendanceSystem = responseReader.ReadInt32();
                summary.AttendanceSystemTime = ReadDateTime(responseReader) ?? DateTime.Now;
                summary.AttendanceManual = responseReader.ReadInt32();
                summary.AttendanceManualTime = ReadDateTime(responseReader) ?? DateTime.Now;

                // Staff (Manager and Callers)
                int managerId = responseReader.ReadInt32();

                summary.Manager = m_staff.FirstOrDefault(staffMember => staffMember.Id == managerId);
                IList<Staff> callers = new List<Staff>();

                short callerCount = responseReader.ReadInt16();
                for (int callerIndex = 0; callerIndex < callerCount; ++callerIndex)
                {
                    int callerId = responseReader.ReadInt32();
                    Staff caller = m_staff.FirstOrDefault(staffMember => staffMember.Id == callerId);

                    if (caller != null)
                        callers.Add(caller);
                }
                summary.Callers = callers;

                // Comments
                summary.Comments = ReadString(responseReader);

                // Sales
                summary.SalesPaper = ReadDecimal(responseReader) ?? 0M;
                summary.SalesElectronic = ReadDecimal(responseReader) ?? 0M;
                summary.SalesBingoOther = ReadDecimal(responseReader) ?? 0M; //FIX: DE8961 Session summary does calculate bingo other sales
                summary.SalesPullTab = ReadDecimal(responseReader) ?? 0M;
                summary.SalesConcession = ReadDecimal(responseReader) ?? 0M;
                summary.SalesMerchandise = ReadDecimal(responseReader) ?? 0M;
                summary.SalesDeviceFee = ReadDecimal(responseReader) ?? 0M;
                summary.SalesDiscount = ReadDecimal(responseReader) ?? 0M;
                summary.SalesValidation = ReadDecimal(responseReader) ?? 0M;

                // Prizes
                summary.PrizesCash = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesCheck = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesMerchandise = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesAccrualInc = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesPullTab = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesOther = ReadDecimal(responseReader) ?? 0M;

                // Session Costs
                IList<SessionCost> costs = new List<SessionCost>();
                short costCount = responseReader.ReadInt16();
                for (int costIndex = 0; costIndex < costCount; ++costIndex)
                {
                    SessionCost cost = new SessionCost();
                    cost.Id = responseReader.ReadInt32();
                    cost.MasterId = responseReader.ReadInt32();
                    cost.Name = ReadString(responseReader);
                    cost.Code = ReadString(responseReader);
                    cost.Value = ReadDecimal(responseReader) ?? 0;
                    cost.IsRegister = responseReader.ReadBoolean();
                    cost.IsInactive = responseReader.ReadBoolean();
                    costs.Add(cost);
                }
                summary.SessionCosts = costs;

                // Money values
                summary.BankBegin = ReadDecimal(responseReader) ?? 0M;
                summary.BankFill = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesAccrualPay = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesFees = ReadDecimal(responseReader) ?? 0M;
                summary.OverCoupons = ReadDecimal(responseReader) ?? 0M;
                summary.SalesTax = ReadDecimal(responseReader) ?? 0M;
                summary.OverCash = ReadDecimal(responseReader) ?? 0M;
                summary.OverDebitCredit = ReadDecimal(responseReader) ?? 0M;
                summary.OverChecks = ReadDecimal(responseReader) ?? 0M;
                summary.BankEnd = ReadDecimal(responseReader) ?? 0M;
                summary.OverMoneyOrders = ReadDecimal(responseReader) ?? 0M;
                summary.OverGiftCards = ReadDecimal(responseReader) ?? 0M;
                summary.OverChips = ReadDecimal(responseReader) ?? 0M;
                summary.AccrualCashPayouts = ReadDecimal(responseReader) ?? 0M;
                summary.KioskSale = ReadDecimal(responseReader) ?? 0M;
                summary.KioskVoids = ReadDecimal(responseReader) ?? 0M; //US5352 - kiosk voids

                var actualCashDenoms = new List<ActualCashCurrencyDenom>();
                short actualCashCount = responseReader.ReadInt16();
                for (short i = 0; i < actualCashCount; ++i)
                {
                    var item = new ActualCashCurrencyDenom();
                    item.ID = responseReader.ReadInt32();
                    item.SessionSummaryID = responseReader.ReadInt32();
                    item.ISOCode = ReadString(responseReader);
                    item.CurrencyDetailID = responseReader.ReadInt32();
                    item.Quantity = responseReader.ReadInt32();
                    item.CurrencyValue = ReadDecimal(responseReader) ?? 0M;
                    item.ExchangeRate = ReadDecimal(responseReader) ?? 1M;
                    item.DefaultValue = ReadDecimal(responseReader) ?? 0M;
                    actualCashDenoms.Add(item);
                }
                summary.LoadActualCashDenomQuantities(actualCashDenoms);

                // Do the assignment
                Summary = summary;
            }
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the id of the message
        /// </summary>
        public override int Id
        {
            get
            {
                return 6062;
            }
        }

        /// <summary>
        /// Gets the name of the message
        /// </summary>
        public override string Name
        {
            get
            {
                return "Get Session Summary";
            }
        }

        /// <summary>
        /// Gets the session summary
        /// </summary>
        public Summary Summary
        {
            get;
            private set;
        }

        #endregion
    }
}
