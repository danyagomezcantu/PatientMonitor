using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    // Defines the ECG class, which represents the ECG physiological parameter.
    // It extends the PhysioParameter abstract class and implements the IPhysioFunctions interface.
    // The ECG class calculates sample data for the ECG signal using a cosine wave.
    public class ECG : PhysioParameter, IPhysioFunctions
    {
        public ECG(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
            : base(amplitude, frequency, harmonics, lowAlarm, highAlarm) { } // Constructor
        // Initializes an ECG object with specified amplitude, frequency, harmonics, and alarm values.

        public override double NextSample(double timeIndex)
        {
            // Calculates the next ECG sample value at a given time using a cosine wave and harmonics.
            double sample = 0.0;
            if (Frequency == 0 || Amplitude == 0) return 0.0;

            for (int i = 1; i <= Harmonics; i++)
            {
                sample += Math.Cos(2 * Math.PI * (Frequency / 60.0) * timeIndex * i);
            }

            return sample * Amplitude;
        }
    }
}