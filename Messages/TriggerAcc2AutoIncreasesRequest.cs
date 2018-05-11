#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2016 Fortunet, Inc.
#endregion


using System.IO;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameTech.Elite.Client.Modules.SessionSummary.Messages
{
    /// <summary>
    /// Represents the auto accruals increase request server message.
    /// </summary>
    internal class TriggerAcc2AutoIncreasesRequest : ServerMessage
    {
        #region Constructors
        /// <summary>Initializes a new instance of the TriggerAcc2AutoIncreasesRequest class.</summary>
        /// <param name="gamingDate">Gaming date to accrue for</param>
        /// <param name="sessionPlayedId">SessionPlayedId of session to accrue for, or 0 for Daily accruals.</param>
        public TriggerAcc2AutoIncreasesRequest(DateTime gamingDate, int sessionPlayedId, bool overridePrior, bool spawnAutomatedTransactions)
        {
            if(sessionPlayedId < 0)
                throw new ArgumentNullException("sessionPlayedId");
            GamingDate = gamingDate;
            SessionPlayedId = sessionPlayedId;
            OverridePriorIncreases = overridePrior;
            SpawnAutomatedTransactions = spawnAutomatedTransactions;
        }

        /// <summary>Initializes a new instance of the TriggerAcc2AutoIncreasesRequest class.</summary>
        /// <param name="client">The client to use to send the server message.</param>
        /// <param name="gamingDate">Gaming date to accrue for</param>
        /// <param name="sessionPlayedId">SessionPlayedId of session to accrue for, or 0 for Daily accruals.</param>
        /// <exception cref="System.ArgumentNullException">client or trans are null references.</exception>
        public TriggerAcc2AutoIncreasesRequest(IEliteClient client, DateTime gamingDate, int sessionPlayedId, bool overridePrior, bool spawnAutomatedTransactions)
            : base(client)
        {
            if(sessionPlayedId < 0)
                throw new ArgumentNullException("sessionPlayedId");
            GamingDate = gamingDate;
            SessionPlayedId = sessionPlayedId;
            OverridePriorIncreases = overridePrior;
            SpawnAutomatedTransactions = spawnAutomatedTransactions;
        }
        #endregion

        #region Member Methods
        /// <summary>Prepares the request to be sent to the server.</summary>
        /// <param name="requestWriter">The binary stream writer that should be used to write any request data necessary.</param>
        protected override void PackRequest(BinaryWriter requestWriter)
        {
            requestWriter.Write(SessionPlayedId);

            string tempDate = GamingDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            requestWriter.Write((ushort)tempDate.Length);
            requestWriter.Write(tempDate.ToCharArray());

            requestWriter.Write(OverridePriorIncreases);
            requestWriter.Write(SpawnAutomatedTransactions);
        }

        /// <summary>Parses the response received from the server.</summary>
        /// <param name="responseReader">The binary stream reader that should be used to read any response data necessary.</param>
        protected override void UnpackResponse(BinaryReader responseReader)
        {
            // Clear existing.
            // Don't want to end up duplicating all the code from MoneyCenter....
            //IncreaseResults.Clear();
            this.IncreaseCount = 0;

            if(ReturnCode == ServerReturnCode.Success)
            {
                this.IncreaseCount = responseReader.ReadInt32();
                // Don't want to end up duplicating all the code from MoneyCenter....
                //for(int i = 0; i < tCount; i++)
                //{
                //    var t = new Acc2Transaction();
                //    t.ReadFromBinReader(responseReader);
                //    IncreaseResults.Add(t);
                //}
            }
        }
        #endregion

        #region Member Properties
        /// <summary>Gets the id.</summary>
        public override int Id { get { return 21114; } }

        /// <summary>Gets the name.</summary>
        public override string Name { get { return "Acc2 Auto Increase/Accrue Request"; } }

        /// <summary>The gaming date for which the accrual/increase is being requested.</summary>
        public DateTime GamingDate { get; protected set; }

        /// <summary>The SessionPlayedId for which the accrual/increase is being requested.</summary>
        public int SessionPlayedId { get; protected set; }

        public bool OverridePriorIncreases { get; protected set; }
        public bool SpawnAutomatedTransactions { get; protected set; }

        public int IncreaseCount { get; protected set; }

        #endregion

    }
}
