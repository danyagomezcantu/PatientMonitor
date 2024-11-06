using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    class Patient
    {
        ECG ecg;
        EMG emg;

        public double ECGAmplitude { set => ecg.Amplitude = value; }
        public double ECGFrequency { set => ecg.Frequency = value; }
        public double EMGAmplitude { set => emg.Amplitude = value; }
        public Patient(double amplitude, double frequency)
        {
            ecg = new ECG(amplitude, frequency);
            emg = new EMG(amplitude, frequency);
        }
        public double NextSample(double timeIndex, MonitorConstants.Parameter parameter)
        {
            double nextSample = 0.0;
            switch (parameter)
            {
                case MonitorConstants.Parameter.ECG:
                    nextSample = ecg.NextSample(timeIndex);
                    break;
                case MonitorConstants.Parameter.EMG:
                    nextSample = emg.NextSample(timeIndex);
                    break;
                default: break;
            }
            return nextSample;
        }
    }
}
