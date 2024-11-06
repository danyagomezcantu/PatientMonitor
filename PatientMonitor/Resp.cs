using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    internal class Resp
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

        public Resp(double amplitude = 0.0, double frequency = 0.0)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
        }

        public double NextSample(double timeIndex)
        {
            if (frequency == 0 || amplitude == 0)
                return 0.0;

            double period = 60.0 / frequency;
            double phase = (timeIndex % period) / period;
            return amplitude * (2 * phase - 1);
        }
    }

}
