#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2016 Fortunet, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Class representing a currency denom for cash collected.
    /// </summary>
    internal class ActualCashCurrencyDenom : ICloneable
    {
        #region Constructors
        /// <summary>
        /// Initializes a session cost object
        /// </summary>
        public ActualCashCurrencyDenom()
        {
        }

        /// <summary>
        /// Copy constructs a actual cash currency denom from another
        /// </summary>
        /// <param name="src">The source of the session cost to copy.</param>
        public ActualCashCurrencyDenom(ActualCashCurrencyDenom src)
        {
            ID = src.ID;
            SessionSummaryID = src.SessionSummaryID;
            ISOCode = src.ISOCode;
            CurrencyDetailID = src.CurrencyDetailID;
            Quantity = src.Quantity;
            CurrencyValue = src.CurrencyValue;
            ExchangeRate = src.ExchangeRate;
            DefaultValue = src.DefaultValue;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Clones the Actual Cash Denoms object
        /// </summary>
        /// <returns>The new cloned object.</returns>
        public object Clone()
        {
            ActualCashCurrencyDenom clone = new ActualCashCurrencyDenom(this);
            return clone;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the ID of the denom
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or Sets the ID of the session summary
        /// </summary>
        public int SessionSummaryID { get; set; }

        /// <summary>
        /// Gets or Sets 
        /// </summary>
        public string ISOCode { get; set; }

        /// <summary>
        /// The Currency Detail ID of this currency
        /// </summary>
        public int CurrencyDetailID { get; set; }

        /// <summary>
        /// The quantity of this item
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The value of this item in the current currency
        /// </summary>
        public decimal CurrencyValue { get; set; }

        /// <summary>
        /// The exchange rate applied to this item
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// The default value of this item.
        /// </summary>
        public decimal DefaultValue { get; set; }

        #endregion
    }
}
