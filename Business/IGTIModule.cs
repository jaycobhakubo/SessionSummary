#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using System.Runtime.InteropServices;

namespace GameTech.Elite.Client.Modules.SessionSummary.Business
{
    /// <summary>
    /// The interface that all module assemblies must implement in order to be
    /// loaded by the system.
    /// </summary>
    [
        ComVisible(true),
        Guid("66DEAFED-10D8-49DB-9204-C1AD19D64138"),
        InterfaceType(ComInterfaceType.InterfaceIsDual)
    ]
    public interface IGTIModule
    {
        [DispId(1)]
        void StartModule(int moduleId);
        [DispId(2)]
        void StopModule();
        [DispId(3)]
        string QueryModuleName();
        [DispId(4)]
        void ComeToFront();
        [DispId(5)]
        void MessageReceived(int commandId, object msgData);
    }

    /// <summary>
    /// The event source interface that all module DLLs must expose in order 
    /// to be loaded by the system.
    /// </summary>
    [
        ComVisible(true),
        Guid("55545724-E843-4ff4-96D1-3DC5FF4D6A94"),
        InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
    ]
    public interface _IGTIModuleEvents
    {
        [DispId(1)]
        void Stopped(int moduleId);
    }
}
