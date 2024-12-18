using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientMonitor
{
    public class PatientComparer : IComparer<Patient>
    {
        MonitorConstants.compareAfter ca;

        public MonitorConstants.compareAfter CA
        {
            set { ca = value; }
            get { return ca; }
        }

        public int Compare(Patient x, Patient y)
        {
            int result = 0;

            switch (ca)
            {
                case MonitorConstants.compareAfter.Age:
                    result = x.Age.CompareTo(y.Age);
                    break;

                case MonitorConstants.compareAfter.Name:
                    result = string.Compare(x.PatientName, y.PatientName);
                    break;

                case MonitorConstants.compareAfter.Clinic:
                    result = x.Clinic.CompareTo(y.Clinic);
                    break;

                case MonitorConstants.compareAfter.Ambulatory:
                    result = x.Type.CompareTo(y.Type);
                    break;

                case MonitorConstants.compareAfter.Stationary:
                    result = y.Type.CompareTo(x.Type);
                    break;

                default:
                    result = 0;
                    break;
            }

            return result;
        }
    }
}

