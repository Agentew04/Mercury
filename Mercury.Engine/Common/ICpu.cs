﻿using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Common;

public interface ICpu : IAsyncClockable {
    
    public Machine Machine { get; set; }
    
    public uint DropoffAddress { get; set; }
    
    public RegisterBank RegisterBank { get; }
    
    public int ExitCode { get; }
    
    public ValueTask Halt(int code = 0);
}