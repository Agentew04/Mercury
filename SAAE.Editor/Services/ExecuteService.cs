using System;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Editor.Services;

/// <summary>
/// Service responsible to enable controls and the application to interact
/// with the engine to execute code. 
/// </summary>
public class ExecuteService
{
    public void Load()
    {
    }
    
    public Machine GetCurrentMachine()
    {
        return new Machine();
    }
}
