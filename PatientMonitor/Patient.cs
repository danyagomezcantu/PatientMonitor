using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class Patient
    {
        private string patientName;
        private int age;
        private DateTime dateOfStudy;
        private ECG ecg;

        public Patient(string name, int age, DateTime dateOfStudy, double amplitude, double frequency, int harmonics)
        {
            this.patientName = name;
            this.age = age;
            this.dateOfStudy = dateOfStudy;
            this.ecg = new ECG(amplitude, frequency, harmonics);
        }

        public string PatientName
        {
            get { return patientName; }
            set { patientName = value; }
        }

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        public DateTime DateOfStudy
        {
            get { return dateOfStudy; }
            set { dateOfStudy = value; }
        }

        public double ECGAmplitude
        {
            get { return ecg.Amplitude; }
            set { ecg.Amplitude = value; }
        }

        public double ECGFrequency
        {
            get { return ecg.Frequency; }
            set { ecg.Frequency = value; }
        }

        public int ECGHarmonics
        {
            get { return ecg.Harmonics; }
            set { ecg.Harmonics = value; }
        }

        public double NextSample(double timeIndex)
        {
            return ecg.NextSample(timeIndex);
        }
    }
}