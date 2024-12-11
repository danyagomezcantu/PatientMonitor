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

        int stepCount = 0;
        double timeStep = 0.1;

        public override double NextSample(double timeIndex)
        {
            if (Frequency == 0 || Amplitude == 0)
                return 0.0;

            double period = 60.0 / Frequency;
            double currentTime = stepCount * timeStep;
            double phase = (currentTime % period) / period; // Normaliza de 0 a 1

            if (Math.Abs(phase - 1.0) < 1e-9)
                phase = 0.0;

            stepCount++;
            double value = Amplitude * (2 * phase - 1);

            // Confirma que esté entre Amplitud y -Amplitud
            value = Math.Max(-Amplitude, Math.Min(Amplitude, value));

            // Debug
            Console.WriteLine($"TimeIndex: {timeIndex}, Phase: {phase}, Value: {value}, Amplitude: {Amplitude}");
            return value;
        }
    }
}
