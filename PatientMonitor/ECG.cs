using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class ECG : PhysioParameter, IPhysioFunctions
    {
        public ECG(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
            : base(amplitude, frequency, harmonics, lowAlarm, highAlarm) { }

        public override double NextSample(double timeIndex)
        {
            double sample = 0.0;
            if (Frequency == 0 || Amplitude == 0) return 0.0;

            for (int i = 1; i <= Harmonics; i++)
            {
                sample += Math.Cos(2 * Math.PI * (Frequency / 60.0) * timeIndex * i);
            }

            return sample * Amplitude;
        }

        public void DisplayLowAlarm()
        {
            base.DisplayLowAlarm();
        }

        public void DisplayHighAlarm()
        {
            base.DisplayHighAlarm();
        }
    }
}