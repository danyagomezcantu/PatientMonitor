using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public abstract class PhysioParameter
    {
        const double HzToBeatsPerMin = 60.0;
        private double amplitude;
        private double frequency;
        private int harmonics;

        // New Variables for Alarms
        private double lowAlarm;
        private double highAlarm;
        private string lowAlarmString = " ";
        private string highAlarmString = " ";

        // Constructor
        public PhysioParameter(double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.harmonics = harmonics;
            this.lowAlarm = lowAlarm;
            this.highAlarm = highAlarm;
            DisplayLowAlarm();
            DisplayHighAlarm();
        }

        // Properties
        public double Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }

        public double Frequency
        {
            get { return frequency; }
            set
            {
                frequency = value;
                DisplayLowAlarm();
                DisplayHighAlarm();
            }
        }

        public int Harmonics
        {
            get { return harmonics; }
            set { harmonics = value; }
        }

        public double LowAlarm
        {
            get { return lowAlarm; }
            set
            {
                lowAlarm = value;
                DisplayLowAlarm();
            }
        }

        public double HighAlarm
        {
            get { return highAlarm; }
            set
            {
                highAlarm = value;
                DisplayHighAlarm();
            }
        }

        public string LowAlarmString => lowAlarmString;
        public string HighAlarmString => highAlarmString;

        // Methods to Display Alarms
        public void DisplayLowAlarm()
        {
            if (frequency <= lowAlarm)
            {
                lowAlarmString = "LOW ALARM: " + frequency;
            }
            else
            {
                lowAlarmString = " ";
            }
        }

        public void DisplayHighAlarm()
        {
            if (frequency >= highAlarm)
            {
                highAlarmString = "HIGH ALARM: " + frequency;
            }
            else
            {
                highAlarmString = " ";
            }
        }

        // Abstract Method
        public abstract double NextSample(double timeIndex);
    }
}


