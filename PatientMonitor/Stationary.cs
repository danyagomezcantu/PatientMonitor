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

        public Stationary(string name, int age, DateTime date, double amplitude, double frequency, int harmonics, double lowAlarm, double highAlarm, MonitorConstants.clinic clinic, int roomNumber)
            : base(name, age, date, amplitude, frequency, harmonics, lowAlarm, highAlarm, clinic)
        {
            RoomNumber = roomNumber;
        }
    }
}
