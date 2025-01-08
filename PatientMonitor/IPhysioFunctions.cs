using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    // This interface defines the structure for physiological parameter classes.
    // It provides property declarations for amplitude, frequency, and harmonics,
    // and method signatures for generating samples and displaying alarms.

    public interface IPhysioFunctions
    {
        double Amplitude { get; set; }
        double Frequency { get; set; }
        int Harmonics { get; set; }
        double NextSample(double timeIndex); // Calculates the next signal sample based on time. Is overridden by the specific physiological parameter class.
        void DisplayLowAlarm(); // Displays a low alarm message. Is overridden by the specific physiological parameter class.
        void DisplayHighAlarm(); // Displays a high alarm message. Is overridden by the specific physiological parameter class.
    }
}


