using System;
using System.Collections.Generic;
using System.IO;

namespace PatientMonitor
{
    public class Database
    {
        const int maxActivePatients = 100;
        string dataPath;

        private List<Patient> data = new List<Patient>();
        public List<Patient> Data { get { return data; } }
        
        public void AddPatient(Patient aPatient)
        {
            data.Add(aPatient);
        }

        public List<Patient> GetPatients()
        {
            return data;
        }

        public String DataPath
        {
            get { return dataPath; }
            set { dataPath = value; }
        }

        public Database()
        {
            dataPath = "";
        }

        public Database(string dataPath)
        {
            OpenData(dataPath);
        }

        public void SaveData(string dataPath)
        {
            int patientCount = 0;

            using (Stream ausgabe = File.Create(dataPath))
            {
                BinaryWriter writer = new BinaryWriter(ausgabe);
                patientCount = data.Count;
                writer.Write(patientCount);
                foreach (Patient patient in data)
                {
                    if (patient is Stationary)
                        writer.Write(true);
                    else
                        writer.Write(false);
                    writer.Write(patient.PatientName);
                    writer.Write(patient.Age);
                    writer.Write(patient.DateOfStudy.ToString());
                    writer.Write((int)patient.Clinic);
                    writer.Write(patient.ECGAmplitude);
                    writer.Write(patient.ECGFrequency);
                    writer.Write(patient.ECGHighAlarm);
                    writer.Write(patient.ECGLowAlarm);
                    writer.Write(patient.ECGHarmonics);
                    writer.Write(patient.EEGAmplitude);
                    writer.Write(patient.EEGFrequency);
                    writer.Write(patient.EEGHighAlarm);
                    writer.Write(patient.EEGLowAlarm);
                    writer.Write(patient.EEGHarmonics);
                    writer.Write(patient.EMGAmplitude);
                    writer.Write(patient.EMGFrequency);
                    writer.Write(patient.EMGHighAlarm);
                    writer.Write(patient.EMGLowAlarm);
                    writer.Write(patient.EMGHarmonics);
                    writer.Write(patient.RespAmplitude);
                    writer.Write(patient.RespFrequency);
                    writer.Write(patient.RespHighAlarm);
                    writer.Write(patient.RespLowAlarm);
                    writer.Write(patient.RespHarmonics);
                    if (patient is Stationary)
                    {
                        Stationary stationary;
                        stationary = patient as Stationary;
                        writer.Write(stationary.RoomNumber);
                    }
                }
            }
        }

        public void OpenData(string dataPath)
        {
            this.DataPath = dataPath;
            BinaryReader reader;
            int patientCount = 0;
            Patient patient;
            string patientName = "";
            int age = 0;
            DateTime dateOfStudy;
            MonitorConstants.clinic clinic;
            double ecgAmplitude;
            double ecgFrequency;
            double ecgHighAlarm;
            double ecgLowAlarm;
            double eegAmplitude;
            double eegFrequency;
            double eegHighAlarm;
            double eegLowAlarm;
            double emgAmplitude;
            double emgFrequency;
            double emgHighAlarm;
            double emgLowAlarm;
            double respAmplitude;
            double respFrequency;
            double respHighAlarm;
            double respLowAlarm;
            int ecgHarmonics;
            int eegHarmonics;
            int emgHarmonics;
            int respHarmonics;
            bool isStationary = false;
            int roomNum;

            int i;

            using (Stream eingabe = File.OpenRead(dataPath))
            {
                reader = new BinaryReader(eingabe);
                data.Clear();
                patientCount = reader.ReadInt32();
                for (i = 0; i < patientCount; i++)
                {
                    isStationary = reader.ReadBoolean();
                    patientName = reader.ReadString();
                    age = reader.ReadInt32();
                    dateOfStudy = Convert.ToDateTime(reader.ReadString());
                    clinic = (MonitorConstants.clinic)reader.ReadInt32();
                    ecgAmplitude = reader.ReadDouble();
                    ecgFrequency = reader.ReadDouble();
                    ecgHighAlarm = reader.ReadDouble();
                    ecgLowAlarm = reader.ReadDouble();
                    ecgHarmonics = reader.ReadInt32();
                    eegAmplitude = reader.ReadDouble();
                    eegFrequency = reader.ReadDouble();
                    eegHighAlarm = reader.ReadDouble();
                    eegLowAlarm = reader.ReadDouble();
                    eegHarmonics = reader.ReadInt32();
                    emgAmplitude = reader.ReadDouble();
                    emgFrequency = reader.ReadDouble();
                    emgHighAlarm = reader.ReadDouble();
                    emgLowAlarm = reader.ReadDouble();
                    emgHarmonics = reader.ReadInt32();
                    respAmplitude = reader.ReadDouble();
                    respFrequency = reader.ReadDouble();
                    respHighAlarm = reader.ReadDouble();
                    respLowAlarm = reader.ReadDouble();
                    respHarmonics = reader.ReadInt32();

                    if (isStationary)
                    {
                        roomNum = reader.ReadInt32();
                        patient = new Stationary(patientName, age, dateOfStudy,
                                    ecgAmplitude, ecgFrequency, ecgHarmonics, ecgLowAlarm, ecgHighAlarm,
                                    eegAmplitude, eegFrequency, eegHarmonics, eegLowAlarm, eegHighAlarm,
                                    emgAmplitude, emgFrequency, emgHarmonics, emgLowAlarm, emgHighAlarm,
                                    respAmplitude, respFrequency, respHarmonics, respLowAlarm, respHighAlarm,
                                    clinic, roomNum);
                    }
                    else
                        patient = new Patient(patientName, age, dateOfStudy,
                                    ecgAmplitude, ecgFrequency, ecgHarmonics, ecgLowAlarm, ecgHighAlarm,
                                    eegAmplitude, eegFrequency, eegHarmonics, eegLowAlarm, eegHighAlarm,
                                    emgAmplitude, emgFrequency, emgHarmonics, emgLowAlarm, emgHighAlarm,
                                    respAmplitude, respFrequency, respHarmonics, respLowAlarm, respHighAlarm,
                                    clinic);

                    data.Add(patient);
                }
            }
        }
    }
}