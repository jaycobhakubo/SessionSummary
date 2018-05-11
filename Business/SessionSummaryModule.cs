#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Properties;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// The implementation of the IGTIModule COM interface for Session Summary.
    /// </summary>
    [
        ComVisible(true),
        Guid("4686253a-aab5-4054-8c9a-b2b5a5229567"),
        ClassInterface(ClassInterfaceType.None),
        ComSourceInterfaces(typeof(_IGTIModuleEvents)),
        ProgId("GameTech.Elite.Client.Modules.SessionSummary")
    ]
    public sealed class SessionSummaryCOMModule : EliteCOMModule, IGTIModule
    {
        #region Member Methods
        /// <summary>
        /// Returns the name of this module.
        /// </summary>
        /// <returns>The module's name.</returns>
        public override string QueryModuleName()
        {
            return Resources.ModuleName;
        }

        /// <summary>
        /// Creates an instance of SessionSummaryModule to run in the
        /// specified application domain.
        /// </summary>
        /// <param name="domain">The application domain the module will run
        /// in.</param>
        /// <returns>An instance of SessionSummaryModule.</returns>
        protected override EliteModule CreateModule(AppDomain domain)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            SessionSummaryModule module = (SessionSummaryModule)domain.CreateInstanceFromAndUnwrap(ModuleAssemblyFile, ModuleTypeName);
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;

            return module;
        }

        /// <summary>
        /// Handles when the resolution of an assembly fails. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A ResolveEventArgs that contains the event
        /// data.</param>
        /// <returns>The Assembly that resolves the type, assembly, or
        /// resource; or null if the assembly cannot be resolved.</returns>
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if(args.Name == Assembly.GetExecutingAssembly().FullName)
                return Assembly.GetExecutingAssembly();
            else
                return null;
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the file of the assembly that contains module class.
        /// </summary>
        protected override string ModuleAssemblyFile
        {
            get
            {
                return Assembly.GetExecutingAssembly().Location;
            }
        }

        /// <summary>
        /// Gets the name of the module class.
        /// </summary>
        protected override string ModuleTypeName
        {
            get
            {
                return typeof(SessionSummaryModule).FullName;
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents the class for the Session Summary module.
    /// </summary>
    public sealed class SessionSummaryModule : EliteModule
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SessionSummaryModule class.
        /// </summary>
        public SessionSummaryModule()
            : base(new SessionSummaryController(), true, false)
        {
        }
        #endregion
    }
}