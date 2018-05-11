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
    /// Sends a request to the server to set a session summary
    /// </summary>
    internal class SetSessionSummary : ServerMessage
    {
        #region Member Variables
        private Summary m_summary;
        #endregion

        #region Contructors
        /// <summary>
        /// Initializes a SetSessionSummaryMessageg
        /// </summary>
        /// <param name="summary">The summary to send.</param>
        public SetSessionSummary(Summary summary)
        {
            if (summary == null)
                throw new ArgumentNullException("summary");

            m_summary = summary;
        }
        #endregion

        #region Member Methods

        /// <summary>
        /// Packs a request to sent to the server.
        /// </summary>
        /// <param name="requestWriter">The request.</param>
        protected override void PackRequest(BinaryWriter requestWriter)
        {
            if (requestWriter == null)
                throw new ArgumentNullException("requestWriter");

            // Write attendance
            WriteDateTime(requestWriter, m_summary.SessionDate);
            requestWriter.Write(m_summary.SessionNumber);
            requestWriter.Write(m_summary.AttendanceSystem);
            WriteDateTime(requestWriter, m_summary.AttendanceSystemTime);
            requestWriter.Write(m_summary.AttendanceManual);
            WriteDateTime(requestWriter, m_summary.AttendanceManualTime);

            // Write Manager
            if (m_summary.Manager == null)
                requestWriter.Write((int)0);
            else
                requestWriter.Write(m_summary.Manager.Id);

            // Write Callers
            if (m_summary.Callers == null)
                requestWriter.Write((short)0);
            else
            {
                IList<Staff> callers = m_summary.Callers.ToList();
                short callerCount = (short)callers.Count;
                requestWriter.Write(callerCount);
                foreach (Staff staffMember in callers)
                    requestWriter.Write(staffMember.Id);
            }

            // Write Comments
            WriteString(requestWriter, m_summary.Comments);

            // Write Sales Data
            WriteDecimal(requestWriter, m_summary.SalesPaper, "F2");
            WriteDecimal(requestWriter, m_summary.SalesElectronic, "F2");
            WriteDecimal(requestWriter, m_summary.SalesBingoOther, "F2"); //FIX: DE8961 Session summary does calculate bingo other sales
            WriteDecimal(requestWriter, m_summary.SalesPullTab, "F2");
            WriteDecimal(requestWriter, m_summary.SalesConcession, "F2");
            WriteDecimal(requestWriter, m_summary.SalesMerchandise, "F2");
            WriteDecimal(requestWriter, m_summary.SalesDeviceFee, "F2");
            WriteDecimal(requestWriter, m_summary.SalesDiscount, "F2");
            WriteDecimal(requestWriter, m_summary.SalesValidation, "F2");

            // Write Prizes Data
            WriteDecimal(requestWriter, m_summary.PrizesCash, "F2");
            WriteDecimal(requestWriter, m_summary.PrizesCheck, "F2");
            WriteDecimal(requestWriter, m_summary.PrizesMerchandise, "F2");
            WriteDecimal(requestWriter, m_summary.PrizesAccrualInc, "F2");
            WriteDecimal(requestWriter, m_summary.PrizesPullTab, "F2");
            WriteDecimal(requestWriter, m_summary.PrizesOther, "F2");

            // Write Session Costs
            if (m_summary.SessionCosts == null)
                requestWriter.Write((short)0);
            else
            {
                IList<SessionCost> costs = m_summary.SessionCosts.ToList();
                short costCount = (short)costs.Count;
                requestWriter.Write(costCount);
                foreach (SessionCost cost in costs)
                    requestWriter.Write(cost.Id);
            }

            // Write Cash Data
            WriteDecimal(requestWriter, m_summary.BankBegin, "F2");
            WriteDecimal(requestWriter, m_summary.BankFill, "F2");
            WriteDecimal(requestWriter, m_summary.PrizesAccrualPay, "F2");
            WriteDecimal(requestWriter, m_summary.PrizesFees, "F2");
            WriteDecimal(requestWriter, m_summary.OverCoupons, "F2");
            WriteDecimal(requestWriter, m_summary.SalesTax, "F2");
            WriteDecimal(requestWriter, m_summary.OverCash, "F2");
            WriteDecimal(requestWriter, m_summary.OverDebitCredit, "F2");
            WriteDecimal(requestWriter, m_summary.OverChecks, "F2");
            WriteDecimal(requestWriter, m_summary.BankEnd, "F2");
            WriteDecimal(requestWriter, m_summary.OverMoneyOrders, "F2");
            WriteDecimal(requestWriter, m_summary.OverGiftCards, "F2");
            WriteDecimal(requestWriter, m_summary.OverChips, "F2");
            WriteDecimal(requestWriter, m_summary.AccrualCashPayouts, "F2");
            WriteDecimal(requestWriter, m_summary.KioskSale, "F2");
            WriteDecimal(requestWriter, m_summary.KioskVoids, "F2"); //US5352 - kiosk voids

            if (m_summary.ActualCashDenoms == null)
                requestWriter.Write((short)0);
            else
            {
                IList<ActualCashCurrencyDenom> denoms = m_summary.ActualCashDenoms.ToList();
                short denomcount = (short)denoms.Count;
                requestWriter.Write(denomcount);
                foreach (var item in denoms)
                {
                    WriteString(requestWriter, item.ISOCode);
                    requestWriter.Write((int)item.CurrencyDetailID);
                    requestWriter.Write((int)item.Quantity);
                    WriteDecimal(requestWriter, item.CurrencyValue, "F2");
                    WriteDecimal(requestWriter, item.ExchangeRate, "F2");
                    WriteDecimal(requestWriter, item.DefaultValue, "F2");
                }
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
                return 6061;
            }
        }

        /// <summary>
        /// Gets the name of the message
        /// </summary>
        public override string Name
        {
            get
            {
                return "Set Session Summary";
            }
        }

        #endregion
    }
}
