using System;
using System.Collections.Generic;

namespace PatientMonitor
{
    public class Patient
    {
        private MonitorConstants.clinic clinic;

        public string PatientName { get; set; }
        public int Age { get; set; }
        public DateTime DateOfStudy { get; set; }
        public ECG ECG { get; set; }
        public MonitorConstants.clinic Clinic
        {
            get { return clinic; }
            set { clinic = value; }
        }
        public virtual string Room => "No room";

        public virtual string Type => "Ambulatory";
        public int ECGHarmonics { get => ECG.Harmonics; set => ECG.Harmonics = value; }
        public EEG EEG { get; set; }
        public EMG EMG { get; set; }
        public Resp Resp { get; set; }

        private List<double> samples;
        private const int maxSamples = 1024;
        public Patient(string name, int age, DateTime date, double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm, MonitorConstants.clinic clinic)
        {
            PatientName = name;
            Age = age;
            DateOfStudy = date;
            this.clinic = clinic;

            ECG = new ECG(amplitude, frequency, harmonics, lowAlarm, highAlarm);
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

        public void UpdateAlarms(MonitorConstants.Parameter parameter)
        {
            switch (parameter)
            {
                case MonitorConstants.Parameter.ECG:
                    ECG.DisplayLowAlarm();
                    ECG.DisplayHighAlarm();
                    break;

                case MonitorConstants.Parameter.EEG:
                    EEG.DisplayLowAlarm();
                    EEG.DisplayHighAlarm();
                    break;

                case MonitorConstants.Parameter.EMG:
                    EMG.DisplayLowAlarm();
                    EMG.DisplayHighAlarm();
                    break;

                case MonitorConstants.Parameter.Resp:
                    Resp.DisplayLowAlarm();
                    Resp.DisplayHighAlarm();
                    break;
            }
        }
    }
}
