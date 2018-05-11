#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.IO;
using GameTech.Elite.Client.Modules.SessionSummary.Business;

namespace GameTech.Elite.Client.Modules.SessionSummary.Messages
{
    /// <summary>
    /// Message class for getting a calculated session summary.
    /// </summary>
    internal class GetCalculatedSessionSummary : ServerMessage
    {
        #region Member Variables
        private DateTime m_date;
        private short m_session;
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a session summary message
        /// </summary>
        /// <param name="date">The date of the session.</param>
        /// <param name="session">The number of the session on the specified date.</param>
        public GetCalculatedSessionSummary(DateTime date, short session)
        {
            m_date = date;
            m_session = session;
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
            requestWriter.Write((int)m_session);
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
                Summary summary = new Summary(m_date, m_session);

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
                summary.PrizesOther = ReadDecimal(responseReader) ?? 0M;

                summary.PrizesFees = ReadDecimal(responseReader) ?? 0M;
                summary.SalesTax = ReadDecimal(responseReader) ?? 0M;

                summary.BankBegin = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesAccrualInc = ReadDecimal(responseReader) ?? 0M;
                summary.PrizesAccrualIncCountExpected = responseReader.ReadInt32();
                summary.PrizesAccrualIncCountApplied = responseReader.ReadInt32();
                summary.PrizesAccrualPay = ReadDecimal(responseReader) ?? 0M;
                summary.BankEnd = ReadDecimal(responseReader) ?? 0M;

                // Over/short
                summary.OverCash = ReadDecimal(responseReader) ?? 0M;
                summary.OverDebitCredit = ReadDecimal(responseReader) ?? 0M;
                summary.OverChecks = ReadDecimal(responseReader) ?? 0M;
                summary.OverMoneyOrders = ReadDecimal(responseReader) ?? 0M;
                summary.OverCoupons = ReadDecimal(responseReader) ?? 0M;
                summary.OverGiftCards = ReadDecimal(responseReader) ?? 0M;
                summary.OverChips = ReadDecimal(responseReader) ?? 0M;

                summary.AccrualCashPayouts = ReadDecimal(responseReader) ?? 0M;
                summary.AttendanceManual = responseReader.ReadInt32(); // US4867
                summary.KioskSale = ReadDecimal(responseReader) ?? 0M;
                summary.KioskVoids = ReadDecimal(responseReader) ?? 0M; //US5352 - kiosk voids

                // Assign the summary
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
                return 6063;
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
