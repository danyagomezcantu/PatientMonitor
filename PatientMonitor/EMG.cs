﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    internal class EMG
    {
        private const double HzToBeatsPerMin = 60.0;
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

        public EMG(double amplitude = 0.0, double frequency = 0.0)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
        }

        public double NextSample(double timeIndex)
        {
            if (frequency == 0 || amplitude == 0)
                return 0.0;

            double signalLength = HzToBeatsPerMin / frequency;
            double stepIndex = timeIndex % signalLength;
            return stepIndex > signalLength / 2.0 ? amplitude : -amplitude;
        }
    }

}
