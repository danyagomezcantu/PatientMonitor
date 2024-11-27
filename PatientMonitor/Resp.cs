using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class Resp : PhysioParameter, IPhysioFunctions
    {
        public Resp(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
            : base(amplitude, frequency, harmonics, lowAlarm, highAlarm) { }

        public override double NextSample(double timeIndex)
        {
            if (Frequency == 0 || Amplitude == 0)
                return 0.0;

            double period = 60.0 / Frequency;
            double phase = (timeIndex % period) / period;
            return Amplitude * (2 * phase - 1);
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
