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
    /// Message class to set a session cost
    /// </summary>
    internal class SetSessionCost : ServerMessage
    {
        #region Member Variables
        private SessionCost m_cost;
        #endregion

        #region Construtors
        /// <summary>
        /// Intializez a SetSessionCostMessage with the associated cost
        /// </summary>
        /// <param name="cost">The cost.</param>
        public SetSessionCost(SessionCost cost)
        {
            if (cost == null)
                throw new ArgumentNullException("cost");

            m_cost = cost;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Packs the request that is sent out
        /// </summary>
        /// <param name="requestWriter"></param>
        protected override void PackRequest(BinaryWriter requestWriter)
        {
            requestWriter.Write(m_cost.Id);
            requestWriter.Write(m_cost.MasterId);
            WriteString(requestWriter, m_cost.Name);
            WriteString(requestWriter, m_cost.Code);
            WriteDecimal(requestWriter, m_cost.Value, "F2");
            requestWriter.Write(m_cost.IsRegister);
            requestWriter.Write(m_cost.IsInactive);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the Id of the message
        /// </summary>
        public override int Id
        {
            get
            {
                return 6059;
            }
        }

        /// <summary>
        /// Gets the name of the message
        /// </summary>
        public override string Name
        {
            get
            {
                return "Set Session Cost";
            }
        }
        #endregion
    }
}
