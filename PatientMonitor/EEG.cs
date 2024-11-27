using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class EEG : PhysioParameter, IPhysioFunctions
    {
        public EEG(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
            : base(amplitude, frequency, harmonics, lowAlarm, highAlarm) { }

        public override double NextSample(double timeIndex)
        {
            return (Frequency == 0 || Amplitude == 0) ? 0.0 : Amplitude * Math.Exp(-Frequency * timeIndex / 60.0);
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
