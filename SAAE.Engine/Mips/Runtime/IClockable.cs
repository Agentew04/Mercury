using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Runtime {
    internal interface IClockable {
        void Clock();
        
        bool IsClockingFinished();
    }
}
