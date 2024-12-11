using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class EMG : PhysioParameter, IPhysioFunctions
    {
        public EMG(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
            : base(amplitude, frequency, harmonics, lowAlarm, highAlarm) { }

        public override double NextSample(double timeIndex)
        {
            if (Frequency == 0 || Amplitude == 0)
                return 0.0;

            double signalLength = 60.0 / Frequency;
            double stepIndex = timeIndex % signalLength;
            return stepIndex > signalLength / 2.0 ? Amplitude : -Amplitude;
        }
    }
}
