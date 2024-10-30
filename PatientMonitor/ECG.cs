using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class ECG
    {
        private double amplitude;
        private double frequency;
        private int harmonics;

        public ECG(double amplitude, double frequency, int harmonics)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.harmonics = harmonics;
        }

        public double Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }

        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        public int Harmonics
        {
            get { return harmonics; }
            set { harmonics = value; }
        }

        public double NextSample(double timeIndex)
        {
            const double HzToBeatsPerMin = 100.0;
            double sample = 0.0;

            for (int i = 1; i <= harmonics; i++)
            {
                sample += Math.Cos(2 * Math.PI * (frequency / HzToBeatsPerMin) * timeIndex * i);
            }

            return sample * amplitude;
        }
    }
}
