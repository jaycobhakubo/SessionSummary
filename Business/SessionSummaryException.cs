#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using GameTech.Elite.Base;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// The exception that is thrown when a Session Summary error
    /// occurs.
    /// </summary>
    internal class SessionSummaryException : EliteModuleException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SessionSummaryException class.
        /// </summary>
        public SessionSummaryException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SessionSummaryException class 
        /// with a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public SessionSummaryException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SessionSummaryException class 
        /// with a specified error message and a reference to the inner 
        /// exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of 
        /// the current exception. If the innerException parameter is not a 
        /// null reference, the current exception is raised in a catch block 
        /// that handles the inner exception.</param>
        public SessionSummaryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion
    }
}