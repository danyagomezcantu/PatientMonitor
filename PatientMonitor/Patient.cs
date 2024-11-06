using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    class Patient
    {
        private string patientName;
        private DateTime dateOfStudy;
        private int age;

        private ECG ecg;
        private EEG eeg;
        private EMG emg;
        private Resp resp;

        public string PatientName
        {
            get { return patientName; }
            set { patientName = value; }
        }

        public DateTime DateOfStudy
        {
            get { return dateOfStudy; }
            set { dateOfStudy = value; }
        }

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        public double ECGAmplitude { get => ecg.Amplitude; set => ecg.Amplitude = value; }
        public double ECGFrequency { get => ecg.Frequency; set => ecg.Frequency = value; }
        public int ECGHarmonics { get => ecg.Harmonics; set => ecg.Harmonics = value; }

        public double EEGAmplitude { get => eeg.Amplitude; set => eeg.Amplitude = value; }
        public double EEGFrequency { get => eeg.Frequency; set => eeg.Frequency = value; }

        public double EMGAmplitude { get => emg.Amplitude; set => emg.Amplitude = value; }
        public double EMGFrequency { get => emg.Frequency; set => emg.Frequency = value; }

        public double RespAmplitude { get => resp.Amplitude; set => resp.Amplitude = value; }
        public double RespFrequency { get => resp.Frequency; set => resp.Frequency = value; }

        public Patient(string name, int age, DateTime dateOfStudy, double amplitude, double frequency, int harmonics = 1)
        {
            this.patientName = name;
            this.age = age;
            this.dateOfStudy = dateOfStudy;

            ecg = new ECG(amplitude, frequency, harmonics);
            eeg = new EEG(amplitude, frequency);
            emg = new EMG(amplitude, frequency);
            resp = new Resp(amplitude, frequency);
        }

        public double NextSample(double timeIndex, MonitorConstants.Parameter parameter)
        {
            double nextSample = 0.0;

            switch (parameter)
            {
                case MonitorConstants.Parameter.ECG:
                    nextSample = ecg.NextSample(timeIndex);
                    break;
                case MonitorConstants.Parameter.EEG:
                    nextSample = eeg.NextSample(timeIndex);
                    break;
                case MonitorConstants.Parameter.EMG:
                    nextSample = emg.NextSample(timeIndex);
                    break;
                case MonitorConstants.Parameter.Resp:
                    nextSample = resp.NextSample(timeIndex);
                    break;
            }
            return nextSample;
        }
    }

}
