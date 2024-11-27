using System;
using System.Collections.Generic;

namespace PatientMonitor
{
    public class Patient
    {
        private ECG eCG;

        public string PatientName { get; set; }
        public int Age { get; set; }
        public DateTime DateOfStudy { get; set; }

        public ECG ECG { get => eCG; set => eCG = value; }

        public int ECGHarmonics { get => ECG.Harmonics; set => ECG.Harmonics = value; }
        public EEG EEG { get; set; }
        public EMG EMG { get; set; }
        public Resp Resp { get; set; }

        private List<double> samples;
        private const int maxSamples = 1024;

        public Patient(string name, int age, DateTime date, double ecgAmplitude, double ecgFrequency, int harmonics, double lowAlarm, double highAlarm)
        {
            PatientName = name;
            Age = age;
            DateOfStudy = date;

            ECG = new ECG(ecgAmplitude, ecgFrequency, harmonics, lowAlarm, highAlarm);
            EEG = new EEG(0, 0, 1, lowAlarm, highAlarm);
            EMG = new EMG(0, 0, 1, lowAlarm, highAlarm);
            Resp = new Resp(0, 0, 1, lowAlarm, highAlarm);

            samples = new List<double>();
        }

        public double NextSample(double timeIndex, MonitorConstants.Parameter parameter)
        {
            double nextSample = 0.0;

            switch (parameter)
            {
                case MonitorConstants.Parameter.ECG:
                    nextSample = ECG.NextSample(timeIndex);
                    break;
                case MonitorConstants.Parameter.EEG:
                    nextSample = EEG.NextSample(timeIndex);
                    break;
                case MonitorConstants.Parameter.EMG:
                    nextSample = EMG.NextSample(timeIndex);
                    break;
                case MonitorConstants.Parameter.Resp:
                    nextSample = Resp.NextSample(timeIndex);
                    break;
            }

            AddSample(nextSample);
            return nextSample;
        }

        private void AddSample(double sample)
        {
            if (samples.Count >= maxSamples)
            {
                samples.RemoveAt(0);
            }
            samples.Add(sample);
        }

        public double[] GetLastNSamples(int n)
        {
            if (n > samples.Count)
                n = samples.Count;

            return samples.GetRange(samples.Count - n, n).ToArray();
        }
    }
}
