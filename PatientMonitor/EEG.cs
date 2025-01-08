using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{

    // Represents the EEG physiological parameter.
    // It inherits from PhysioParameter and implements IPhysioFunctions.
    // EEG samples are generated using an exponential decay formula.
    public class EEG : PhysioParameter, IPhysioFunctions
    {
        public EEG(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
            : base(amplitude, frequency, harmonics, lowAlarm, highAlarm) { } // Constructor
        // Constructs an EEG object with specified amplitude, frequency, harmonics, and alarm values.

        public override double NextSample(double timeIndex)
        {
            // Returns the next EEG sample using an exponential decay function.
            return (Frequency == 0 || Amplitude == 0) ? 0.0 : Amplitude * 100 * Math.Exp(-Frequency * timeIndex / 60.0);
        }
    }
}
