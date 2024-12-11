using System.Collections.Generic;

namespace PatientMonitor
{
    public class Database
    {
        const int maxActivePatients = 100;
        public List<Patient> data = new List<Patient>();

        public List<Patient> GetAllPatients()
        {
            return data;
        }

        public void AddPatient(Patient patient)
        {
            if (data.Count < maxActivePatients)
            {
                data.Add(patient);
            }
            else
            {
                throw new System.InvalidOperationException("Maximum number of active patients reached.");
            }
        }

        public void RemovePatient(Patient patient)
        {
            if (data.Contains(patient))
            {
                data.Remove(patient);
            }
        }

        public List<Patient> GetPatientsByType(string type)
        {
            if (type == "Ambulatory")
                return data.FindAll(p => !(p is Stationary));
            else if (type == "Stationary")
                return data.FindAll(p => p is Stationary);
            return new List<Patient>();
        }

        public List<Patient> GetPatientsByClinic(MonitorConstants.clinic clinic)
        {
            return data.FindAll(patient => patient.Clinic == clinic);
        }

        public int GetPatientCount()
        {
            return data.Count;
        }
    }
}