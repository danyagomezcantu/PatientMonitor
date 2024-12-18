using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class MonitorConstants
    {
        public enum Parameter
        {
            ECG = 0,
            EEG = 1,
            EMG = 2,
            Resp = 3
        }

        public enum clinic
        {
            Cardiology = 0,
            Neurology = 1,
            Orthopedics = 2,
            Surgery = 3,
            Dermatology = 4,
            Urology = 5,
            Oftalmology = 6,
            Pediatrics = 7,
            Radiology = 8
        }

        public enum compareAfter
        {
            Name = 1,
            Age = 2,
            Clinic = 3,
            Ambulatory = 4,
            Stationary = 5
        }
    }
}

