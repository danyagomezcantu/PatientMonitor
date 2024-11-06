using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    internal class ECG
    {
        private double amplitude;
        private double frequency;
        private int harmonics;

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

        public ECG(double amplitude = 0.0, double frequency = 0.0, int harmonics = 1)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.harmonics = harmonics;
        }

        public double NextSample(double timeIndex)
        {
            if (frequency == 0 || amplitude == 0)
                return 0.0;

            double sample = 0.0;
            for (int i = 1; i <= harmonics; i++)
            {
                sample += amplitude * Math.Cos(2 * Math.PI * frequency * timeIndex * i / 60.0);
            }
            return sample;
        }
    }
}

