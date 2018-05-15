using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTech.Elite.Client.Modules.SessionSummary.Messages
{
    internal class GetSessionSummaryReportType : ServerMessage
    {
        #region  Member Variables
        #endregion

        #region  Construction
        #endregion

        #region  Member Methods
        #endregion

       
        #region Member Properties
        /// <summary>
        /// Gets the id of the message
        /// </summary>
        public override int Id
        {
            get
            {
                return 18253;
            }
        }

        /// <summary>
        /// Gets the name of the message
        /// </summary>
        public override string Name
        {
            get
            {
                return "Get Session Summary Report Type";
            }
        }

        /// <summary>
        /// Gets the session summary
        /// </summary>
        //public Summary Summary
        //{
        //    get;
        //    private set;
        //}

        #endregion
    }
}
