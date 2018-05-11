#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System.Windows;
using Microsoft.Windows.Themes;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// Represents the Session Summary application running at design time.
    /// </summary>
    public partial class App : Application
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the App class.
        /// </summary>
        public App()
        {
            // Reference something in the Themes namespace in order to get the
            // system to load it (in order to use it in the resource
            // dictionaries).
            #pragma warning disable 0219
            ButtonChrome chrome = new ButtonChrome();
            #pragma warning restore 0219
        }
        #endregion
    }
}
