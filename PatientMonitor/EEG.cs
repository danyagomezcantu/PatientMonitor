using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    internal class EEG
    {
        private double amplitude;
        private double frequency;

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

        public EEG(double amplitude = 0.0, double frequency = 0.0)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
        }

        public double NextSample(double timeIndex)
        {
            return (frequency == 0 || amplitude == 0) ? 0.0 : amplitude * Math.Exp(-frequency * timeIndex / 60.0);
        }
    }

}
