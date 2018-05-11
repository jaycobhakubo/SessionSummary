#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Class representing a session costs
    /// </summary>
    internal class SessionCost : ICloneable
    {
        #region Constructors
        /// <summary>
        /// Initializes a session cost object
        /// </summary>
        public SessionCost()
        {
        }

        /// <summary>
        /// Copy constructs a session cost from another
        /// </summary>
        /// <param name="src">The source of the session cost to copy.</param>
        public SessionCost(SessionCost src)
        {
            Id = src.Id;
            MasterId = src.MasterId;
            Name = src.Name;
            Code = src.Code;
            Value = src.Value;
            IsRegister = src.IsRegister;
            IsInactive = src.IsInactive;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Clones the session cost object
        /// </summary>
        /// <returns>The new cloned object.</returns>
        public object Clone()
        {
            SessionCost clone = new SessionCost(this);
            return clone;
        }

        /// <summary>
        /// Returns a string representing the object to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the Id of the cost
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the MasterId of the cost
        /// </summary>
        public int MasterId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the cost
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the accounting code for the cost
        /// </summary>
        public string Code
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the cost value of the cost
        /// </summary>
        public decimal Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the cost is a cash cost or not
        /// </summary>
        public bool IsRegister
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the cost is inactive
        /// </summary>
        public bool IsInactive
        {
            get;
            set;
        }

        #endregion
    }
}
