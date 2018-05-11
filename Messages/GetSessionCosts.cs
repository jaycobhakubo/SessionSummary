#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System.Collections.Generic;
using System.IO;
using GameTech.Elite.Client.Modules.SessionSummary.Business;

namespace GameTech.Elite.Client.Modules.SessionSummary.Messages
{
    /// <summary>
    /// Message class for getting a list of session costs
    /// </summary>
    internal class GetSessionCosts : ServerMessage
    {
        #region Constructors
        /// <summary>
        /// Initializes a GetSessionCostsMessage object.
        /// </summary>
        public GetSessionCosts()
        {
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Packs a request to sent to the server.
        /// </summary>
        /// <param name="requestWriter">The request.</param>
        protected override void PackRequest(BinaryWriter requestWriter)
        {
        }

        /// <summary>
        /// Unpacks a GetSessionCostsMessage server response.
        /// </summary>
        /// <param name="responseReader">The server response.</param>
        protected override void UnpackResponse(BinaryReader responseReader)
        {
            base.UnpackResponse(responseReader);

            List<SessionCost> costs = new List<SessionCost>();

            uint itemCount = responseReader.ReadUInt32();
            for (int itemIndex = 0; itemIndex < itemCount; ++itemIndex)
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

            Costs = costs;
        }

        #endregion

        #region Member Properties
        /// <summary>
        /// The Id number of the message
        /// </summary>
        public override int Id
        {
            get
            {
                return 6058;
            }
        }

        /// <summary>
        /// The name of the message
        /// </summary>
        public override string Name
        {
            get
            {
                return "Get Session Costs";
            }
        }

        /// <summary>
        /// An enumeration of the session costs returned.
        /// </summary>
        public IEnumerable<SessionCost> Costs
        {
            get;
            private set;
        }
        #endregion
    }
}
