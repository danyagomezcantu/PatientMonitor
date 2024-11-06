using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    internal static class MonitorConstants
    {
        public enum Parameter
        {
            ECG = 0,
            EEG = 1,
            EMG = 2,
            Resp = 3
        }
    }
}
