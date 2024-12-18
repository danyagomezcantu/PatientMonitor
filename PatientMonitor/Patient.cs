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
        public double ECGAmplitude { get => ECG.Amplitude; set => ECG.Amplitude = value; }
        public double ECGFrequency { get => ECG.Frequency; set => ECG.Frequency = value; }
        public double ECGHighAlarm { get => ECG.HighAlarm; set => ECG.HighAlarm = value; }
        public double ECGLowAlarm { get => ECG.LowAlarm; set => ECG.LowAlarm = value; }
        public int EEGHarmonics { get => EEG.Harmonics; set => EEG.Harmonics = value; }
        public double EEGAmplitude { get => EEG.Amplitude; set => EEG.Amplitude = value; }
        public double EEGFrequency { get => EEG.Frequency; set => EEG.Frequency = value; }
        public double EEGHighAlarm { get => EEG.HighAlarm; set => EEG.HighAlarm = value; }
        public double EEGLowAlarm { get => EEG.LowAlarm; set => EEG.LowAlarm = value; }
        public int EMGHarmonics { get => EMG.Harmonics; set => EMG.Harmonics = value; }
        public double EMGAmplitude { get => EMG.Amplitude; set => EMG.Amplitude = value; }
        public double EMGFrequency { get => EMG.Frequency; set => EMG.Frequency = value; }
        public double EMGHighAlarm { get => EMG.HighAlarm; set => EMG.HighAlarm = value; }
        public double EMGLowAlarm { get => EMG.LowAlarm; set => EMG.LowAlarm = value; }
        public int RespHarmonics { get => Resp.Harmonics; set => Resp.Harmonics = value; }
        public double RespAmplitude { get => Resp.Amplitude; set => Resp.Amplitude = value; }
        public double RespFrequency { get => Resp.Frequency; set => Resp.Frequency = value; }
        public double RespHighAlarm { get => Resp.HighAlarm; set => Resp.HighAlarm = value; }
        public double RespLowAlarm { get => Resp.LowAlarm; set => Resp.LowAlarm = value; }
        public EEG EEG { get; set; }
        public EMG EMG { get; set; }
        public Resp Resp { get; set; }

        private List<double> samples;
        private const int maxSamples = 1024;
        public Patient(
            string name, int age, DateTime date,
            double ecgAmplitude, double ecgFrequency, int ecgHarmonics, double ecgLowAlarm, double ecgHighAlarm,
            double eegAmplitude, double eegFrequency, int eegHarmonics, double eegLowAlarm, double eegHighAlarm,
            double emgAmplitude, double emgFrequency, int emgHarmonics, double emgLowAlarm, double emgHighAlarm,
            double respAmplitude, double respFrequency, int respHarmonics, double respLowAlarm, double respHighAlarm,
            MonitorConstants.clinic clinic)
        {
            PatientName = name;
            Age = age;
            DateOfStudy = date;
            this.clinic = clinic;

            ECG = new ECG(ecgAmplitude, ecgFrequency, ecgHarmonics, ecgLowAlarm, ecgHighAlarm);
            EEG = new EEG(eegAmplitude, eegFrequency, eegHarmonics, eegLowAlarm, eegHighAlarm);
            EMG = new EMG(emgAmplitude, emgFrequency, emgHarmonics, emgLowAlarm, emgHighAlarm);
            Resp = new Resp(respAmplitude, respFrequency, respHarmonics, respLowAlarm, respHighAlarm);

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
