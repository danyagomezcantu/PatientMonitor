using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public interface IPhysioFunctions
    {
        double Amplitude { get; set; }
        double Frequency { get; set; }
        int Harmonics { get; set; }
        double NextSample(double timeIndex);
        void DisplayLowAlarm();
        void DisplayHighAlarm();
    }
}


