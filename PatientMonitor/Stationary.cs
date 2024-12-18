using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class Stationary : Patient
    {
        public int RoomNumber { get; set; }

        public override string Room => RoomNumber.ToString(); // Para "No room", que es el default para todos los (ambulatory) patients
        public override string Type => "Stationary";

        public Stationary(
            string name, int age, DateTime date,
            double ecgAmplitude, double ecgFrequency, int ecgHarmonics, double ecgLowAlarm, double ecgHighAlarm,
            double eegAmplitude, double eegFrequency, int eegHarmonics, double eegLowAlarm, double eegHighAlarm,
            double emgAmplitude, double emgFrequency, int emgHarmonics, double emgLowAlarm, double emgHighAlarm,
            double respAmplitude, double respFrequency, int respHarmonics, double respLowAlarm, double respHighAlarm,
            MonitorConstants.clinic clinic, int roomNumber)
            : base(
            name, age, date,
            ecgAmplitude, ecgFrequency, ecgHarmonics, ecgLowAlarm, ecgHighAlarm,
            eegAmplitude, eegFrequency, eegHarmonics, eegLowAlarm, eegHighAlarm,
            emgAmplitude, emgFrequency, emgHarmonics, emgLowAlarm, emgHighAlarm,
            respAmplitude, respFrequency, respHarmonics, respLowAlarm, respHighAlarm,
            clinic)
        {
            RoomNumber = roomNumber;
        }
    }
}
