using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    // Defines the EMG class, which models the EMG physiological parameter.
    // It extends PhysioParameter and implements IPhysioFunctions.
    // The EMG signal alternates between positive and negative amplitude.
    public class EMG : PhysioParameter, IPhysioFunctions
    {
        public EMG(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
            : base(amplitude, frequency, harmonics, lowAlarm, highAlarm) { } // Constructor
        // Initializes an EMG object with amplitude, frequency, harmonics, and alarm values.

        public override double NextSample(double timeIndex)
        {
            // Generates the next EMG sample by alternating amplitude values based on the signal frequency.
            if (Frequency == 0 || Amplitude == 0)
                return 0.0;

            double signalLength = 60.0 / Frequency;
            double stepIndex = timeIndex % signalLength;
            return stepIndex > signalLength / 2.0 ? Amplitude : -Amplitude;
        }
    }
}
