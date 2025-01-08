using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace PatientMonitor
{
    // This file contains the main logic for the Patient Monitor application,
    // managing the UI and interactions between the user and the system.
    // It handles creating patients, displaying data, and managing simulations.
    public partial class MainWindow : Window
    {
        // Declare class variables
        private Patient patient;
        private Stationary stationary;
        private Database database;
        private MonitorConstants.Parameter selectedParameter;
        private DispatcherTimer timer;
        private double timeIndex;
        private List<KeyValuePair<int, double>> dataPoints;
        private List<KeyValuePair<int, double>> dataPoints1;
        private int index;
        private double lastValidFrequency = 0.0;
        private Spektrum spektrum;
        MRImages mrImages;
        public MonitorConstants.clinic clinic;

        public MainWindow()
        {
            // Initialize the main window and its components
            InitializeComponent();
            database = new Database();
            InitializeDefaults();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += Timer_Tick;

            // Initialize the data points for the line series
            dataPoints = new List<KeyValuePair<int, double>>();
            dataPoints1 = new List<KeyValuePair<int, double>>();
            lineSeries.ItemsSource = dataPoints;
            lineSeries1.ItemsSource = dataPoints1;

            spektrum = new Spektrum(512);
            mrImages = new MRImages();

            DisableUIElements();
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            // Set default values for the UI elements
            comboBoxClinic.ItemsSource = Enum.GetValues(typeof(MonitorConstants.clinic));
            comboBoxParameters.ItemsSource = Enum.GetValues(typeof(MonitorConstants.Parameter));
            comboBoxSort.ItemsSource = Enum.GetValues(typeof(MonitorConstants.compareAfter));
            comboBoxSort.SelectedIndex = 0;
            comboBoxClinic.SelectedIndex = 0;
            comboBoxParameters.SelectedIndex = 0;
            selectedParameter = MonitorConstants.Parameter.ECG;

            patient = null;
            buttonBack.IsEnabled = false;
            buttonForth.IsEnabled = false;
            numImages.IsEnabled = false;
            numImages.Text = "10";

            DisableUIElements();
        }

        private void DisableUIElements()
        {
            // Disable the UI elements to prevent user interaction
            sliderAmplitude.IsEnabled = false;
            textBoxFrequency.IsEnabled = false;
            textBoxLowAlarm.IsEnabled = false;
            textBoxHighAlarm.IsEnabled = false;
            comboBoxHarmonics.IsEnabled = false;
            startSimulationButton.IsEnabled = false;
            stopSimulationButton.IsEnabled = false;
            loadImagesButton.IsEnabled = false;
        }

        private void EnableUIElements()
        {
            // Enable the UI elements to allow user interaction
            sliderAmplitude.IsEnabled = true;
            textBoxFrequency.IsEnabled = true;
            textBoxLowAlarm.IsEnabled = true;
            textBoxHighAlarm.IsEnabled = true;
            comboBoxHarmonics.IsEnabled = true;
            startSimulationButton.IsEnabled = true;
            loadImagesButton.IsEnabled = true;
            saveDBButton.IsEnabled = true;
        }

        private void CreatePatientButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new patient based on the user input
            string name = textBoxName.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a valid patient name.");
                return;
            }

            if (!int.TryParse(textBoxAge.Text, out int age) || age <= 0)
            {
                MessageBox.Show("Please enter a valid age.");
                return;
            }

            if (!datePickerMonitor.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a valid date.");
                return;
            }

            DateTime dateOfStudy = datePickerMonitor.SelectedDate.Value;
            if (!double.TryParse(textBoxFrequency.Text, out double ecgFrequency)) ecgFrequency = 0.0;
            if (!double.TryParse(textBoxLowAlarm.Text, out double ecgLowAlarm)) ecgLowAlarm = 0.0;
            if (!double.TryParse(textBoxHighAlarm.Text, out double ecgHighAlarm)) ecgHighAlarm = 0.0;

            double amplitude = sliderAmplitude.Value;
            int harmonics = comboBoxHarmonics.SelectedIndex + 1;

            MonitorConstants.clinic clinic = (MonitorConstants.clinic)comboBoxClinic.SelectedIndex;

            // Set default values for the other parameters
            double defaultFrequency = 0.0;
            int defaultHarmonics = 1;
            double defaultLowAlarm = 0.0;
            double defaultHighAlarm = 0.0;

            Patient newPatient;

            // Handle is the patient is stationary or ambulatory
            if (radioButtonAmbulatory.IsChecked == true)
            {
                newPatient = new Patient(
                    name, age, dateOfStudy,
                    amplitude, ecgFrequency, harmonics, ecgLowAlarm, ecgHighAlarm,
                    amplitude, defaultFrequency, defaultHarmonics, defaultLowAlarm, defaultHighAlarm,
                    amplitude, defaultFrequency, defaultHarmonics, defaultLowAlarm, defaultHighAlarm,
                    amplitude, defaultFrequency, defaultHarmonics, defaultLowAlarm, defaultHighAlarm,
                    clinic
                );
            }
            else if (radioButtonStationary.IsChecked == true)
            {
                if (!int.TryParse(textBoxRoom.Text, out int roomNumber) || roomNumber <= 0)
                {
                    MessageBox.Show("Please enter a valid room number for the stationary patient.");
                    return;
                }

                newPatient = new Stationary(
                    name, age, dateOfStudy,
                    amplitude, ecgFrequency, harmonics, ecgLowAlarm, ecgHighAlarm,
                    amplitude, defaultFrequency, defaultHarmonics, defaultLowAlarm, defaultHighAlarm,
                    amplitude, defaultFrequency, defaultHarmonics, defaultLowAlarm, defaultHighAlarm,
                    amplitude, defaultFrequency, defaultHarmonics, defaultLowAlarm, defaultHighAlarm,
                    clinic, roomNumber
                );
            }
            else
            {
                MessageBox.Show("Please select if the patient is Ambulatory or Stationary.");
                return;
            }

            // Check for duplicate patient
            bool isDuplicate = database.GetPatients().Exists(p => ArePatientsEqual(p, newPatient));
            if (isDuplicate)
            {
                MessageBox.Show("Patient already registered.");
                return;
            }

            patient = newPatient;
            database.AddPatient(patient);
            DisplayDatabase();

            startSimulationButton.IsEnabled = true;
        }

        private bool ArePatientsEqual(Patient p1, Patient p2)
        {
            // Check if two patients are equal based on their properties
            if (p1 == null || p2 == null) return false;

            bool basicEquality = p1.PatientName == p2.PatientName &&
                                 p1.Age == p2.Age &&
                                 p1.DateOfStudy == p2.DateOfStudy &&
                                 p1.Clinic == p2.Clinic &&
                                 p1.ECGAmplitude == p2.ECGAmplitude &&
                                 p1.ECGFrequency == p2.ECGFrequency &&
                                 p1.ECGHarmonics == p2.ECGHarmonics &&
                                 p1.ECGLowAlarm == p2.ECGLowAlarm &&
                                 p1.ECGHighAlarm == p2.ECGHighAlarm;

            if (p1 is Stationary stationary1 && p2 is Stationary stationary2)
            {
                return basicEquality && stationary1.RoomNumber == stationary2.RoomNumber;
            }

            return basicEquality && !(p1 is Stationary) && !(p2 is Stationary);
        }

        private void DisplayDatabase()
        {
            // Display the database of patients in the data grid
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = database.GetPatients();
        }

        private void ComboBoxParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the selected parameter based on the user input
            if (timer == null) return;

            selectedParameter = (MonitorConstants.Parameter)comboBoxParameters.SelectedIndex;
            UpdateUIForSelectedParameter();

            patient?.UpdateAlarms(selectedParameter);
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Update the UI based on the user input: displays either the database or the chart
            if (radioButtonParameter.IsChecked == true)
            {
                chartParameters.Visibility = Visibility.Visible;
                dataGrid.Visibility = Visibility.Collapsed;

                if (labelHighAlarm != null) labelHighAlarm.Visibility = Visibility.Visible;
                if (labelLowAlarm != null) labelLowAlarm.Visibility = Visibility.Visible;

                textBoxFrequency.IsEnabled = true;
                textBoxLowAlarm.IsEnabled = true;
                textBoxHighAlarm.IsEnabled = true;
                sliderAmplitude.IsEnabled = true;
            }
            else if (radioButtonDatabase.IsChecked == true)
            {
                chartParameters.Visibility = Visibility.Collapsed;
                dataGrid.Visibility = Visibility.Visible;

                if (labelHighAlarm != null) labelHighAlarm.Visibility = Visibility.Collapsed;
                if (labelLowAlarm != null) labelLowAlarm.Visibility = Visibility.Collapsed;

                textBoxFrequency.IsEnabled = false;
                textBoxLowAlarm.IsEnabled = false;
                textBoxHighAlarm.IsEnabled = false;
                sliderAmplitude.IsEnabled = false;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the simulation based on the timer tick
            if (patient == null) return;
            double nextSample = patient.NextSample(timeIndex, selectedParameter);

            dataPoints.Add(new KeyValuePair<int, double>(index++, nextSample));

            if (dataPoints.Count > 200)
            {
                dataPoints.RemoveAt(0);
            }

            lineSeries.ItemsSource = null;
            lineSeries.ItemsSource = dataPoints;

            timeIndex += 0.1;
        }

        private void StartSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            // Start the simulation based on the user input
            if (patient == null)
            {
                MessageBox.Show("Please create a patient before starting the simulation.");
                return;
            }

            timeIndex = 0;
            index = 0;
            dataPoints.Clear();
            timer.Start();

            DisplayTime();

            stopSimulationButton.IsEnabled = true;
        }

        private void StopSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop the simulation based on the user input
            timer.Stop();
            stopSimulationButton.IsEnabled = false;
        }

        private void DisplayTime()
        {
            // Display the time chart in the UI
            if (patient == null) return;

            if (dataPoints == null)
            {
                dataPoints = new List<KeyValuePair<int, double>>();
            }

            dataPoints.Clear();
            double[] samples = patient.GetLastNSamples(200);
            for (int i = 0; i < samples.Length; i++)
            {
                dataPoints.Add(new KeyValuePair<int, double>(i, samples[i]));
            }

            lineSeries.ItemsSource = null;
            lineSeries.ItemsSource = dataPoints;
        }

        private void DisplayFrequency()
        {
            // Display the FFT analysis
            if (patient == null) return;

            double[] lastSamples = patient.GetLastNSamples(512);
            double[] fftOutput = spektrum.FFT(lastSamples, lastSamples.Length);

            dataPoints1.Clear();
            for (int i = 0; i < fftOutput.Length; i++)
            {
                dataPoints1.Add(new KeyValuePair<int, double>(i, fftOutput[i]));
            }

            lineSeries1.ItemsSource = null;
            lineSeries1.ItemsSource = dataPoints1;
        }
        
        private void UpdateUIForSelectedParameter()
        {
            // Update the UI based on the selected parameter
            if (patient == null) return;

            switch (selectedParameter)
            {
                case MonitorConstants.Parameter.ECG:
                    sliderAmplitude.Value = patient.ECG.Amplitude;
                    comboBoxHarmonics.IsEnabled = true;
                    comboBoxHarmonics.SelectedIndex = patient.ECG.Harmonics - 1;
                    break;

                case MonitorConstants.Parameter.EEG:
                    sliderAmplitude.Value = patient.EEG.Amplitude;
                    comboBoxHarmonics.IsEnabled = false;
                    break;

                case MonitorConstants.Parameter.EMG:
                    sliderAmplitude.Value = patient.EMG.Amplitude;
                    comboBoxHarmonics.IsEnabled = false;
                    break;

                case MonitorConstants.Parameter.Resp:
                    sliderAmplitude.Value = patient.Resp.Amplitude;
                    comboBoxHarmonics.IsEnabled = false;
                    break;
            }
            patient.UpdateAlarms(selectedParameter);
            UpdateAlarmLabels();
        }


        private void ComboBoxHarmonics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the harmonics based on the user input
            if (patient == null) return;

            if (comboBoxHarmonics.SelectedIndex != -1)
            {
                patient.ECG.Harmonics = comboBoxHarmonics.SelectedIndex + 1;
            }
        }

        private void textBoxAge_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Validate the user input as an Integer for the age text box
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void SliderAmplitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the amplitude based on the user input
            if (patient == null) return;

            switch (selectedParameter)
            {
                case MonitorConstants.Parameter.ECG: patient.ECG.Amplitude = sliderAmplitude.Value; break;
                case MonitorConstants.Parameter.EEG: patient.EEG.Amplitude = sliderAmplitude.Value; break;
                case MonitorConstants.Parameter.EMG: patient.EMG.Amplitude = sliderAmplitude.Value; break;
                case MonitorConstants.Parameter.Resp: patient.Resp.Amplitude = sliderAmplitude.Value; break;
            }
        }

        private void textBoxFrequency_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the frequency based on the user input
            if ((bool)radioButtonDatabase.IsChecked || patient == null) return;

            if (double.TryParse(textBoxFrequency.Text, out double frequency))
            {
                lastValidFrequency = frequency;

                switch (selectedParameter)
                {
                    case MonitorConstants.Parameter.ECG: patient.ECG.Frequency = frequency; break;
                    case MonitorConstants.Parameter.EEG: patient.EEG.Frequency = frequency; break;
                    case MonitorConstants.Parameter.EMG: patient.EMG.Frequency = frequency; break;
                    case MonitorConstants.Parameter.Resp: patient.Resp.Frequency = frequency; break;
                }
                patient.UpdateAlarms(selectedParameter);
                UpdateAlarmLabels();
            }
            else
            {
                MessageBox.Show("Please enter a valid non-negative frequency.");
                textBoxFrequency.Text = lastValidFrequency.ToString();
            }
        }

        private void textBoxLowAlarm_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the low alarm based on the user input
            if ((bool)radioButtonDatabase.IsChecked || patient == null) return;

            if (double.TryParse(textBoxLowAlarm.Text, out double lowAlarm))
            {
                switch (selectedParameter)
                {
                    case MonitorConstants.Parameter.ECG: patient.ECG.LowAlarm = lowAlarm; break;
                    case MonitorConstants.Parameter.EEG: patient.EEG.LowAlarm = lowAlarm; break;
                    case MonitorConstants.Parameter.EMG: patient.EMG.LowAlarm = lowAlarm; break;
                    case MonitorConstants.Parameter.Resp: patient.Resp.LowAlarm = lowAlarm; break;
                }
                patient.UpdateAlarms(selectedParameter);
                UpdateAlarmLabels();
            }
        }

        private void textBoxHighAlarm_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the high alarm based on the user input
            if ((bool)radioButtonDatabase.IsChecked || patient == null) return;

            if (double.TryParse(textBoxHighAlarm.Text, out double highAlarm))
            {
                switch (selectedParameter)
                {
                    case MonitorConstants.Parameter.ECG: patient.ECG.HighAlarm = highAlarm; break;
                    case MonitorConstants.Parameter.EEG: patient.EEG.HighAlarm = highAlarm; break;
                    case MonitorConstants.Parameter.EMG: patient.EMG.HighAlarm = highAlarm; break;
                    case MonitorConstants.Parameter.Resp: patient.Resp.HighAlarm = highAlarm; break;
                }
                patient.UpdateAlarms(selectedParameter);
                UpdateAlarmLabels();
            }
        }
        private void UpdateAlarmLabels()
        {
            // Update the alarm labels based on the user input
            if (patient == null)
            {
                labelLowAlarm.Content = string.Empty;
                labelHighAlarm.Content = string.Empty;
                return;
            }

            if ((bool)radioButtonDatabase.IsChecked)
            {
                labelLowAlarm.Content = string.Empty;
                labelHighAlarm.Content = string.Empty;
                return;
            }

            string lowAlarm = string.Empty;
            string highAlarm = string.Empty;

            switch (selectedParameter)
            {
                case MonitorConstants.Parameter.ECG:
                    lowAlarm = patient.ECG.LowAlarmString;
                    highAlarm = patient.ECG.HighAlarmString;
                    break;

                case MonitorConstants.Parameter.EEG:
                    lowAlarm = patient.EEG.LowAlarmString;
                    highAlarm = patient.EEG.HighAlarmString;
                    break;

                case MonitorConstants.Parameter.EMG:
                    lowAlarm = patient.EMG.LowAlarmString;
                    highAlarm = patient.EMG.HighAlarmString;
                    break;

                case MonitorConstants.Parameter.Resp:
                    lowAlarm = patient.Resp.LowAlarmString;
                    highAlarm = patient.Resp.HighAlarmString;
                    break;
            }

            labelLowAlarm.Content = lowAlarm;
            labelHighAlarm.Content = highAlarm;

            Console.WriteLine($"Low Alarm: {lowAlarm}, High Alarm: {highAlarm}");
        }

        private void fftButton_Click(object sender, RoutedEventArgs e)
        {
            // Perform FFT analysis based on the user input
            if (patient == null)
            {
                MessageBox.Show("Please create a patient before performing FFT analysis.");
                return;
            }

            double[] lastSamples = patient.GetLastNSamples(512);

            // If there are fewer than 512 samples, pad with zeros
            if (lastSamples.Length < 512)
            {
                double[] paddedSamples = new double[512];
                Array.Copy(lastSamples, paddedSamples, lastSamples.Length);
                lastSamples = paddedSamples;
            }

            // Perform FFT
            double[] fftOutput = spektrum.FFT(lastSamples, lastSamples.Length);

            dataPoints1.Clear();
            for (int i = 0; i < fftOutput.Length; i++)
            {
                dataPoints1.Add(new KeyValuePair<int, double>(i, fftOutput[i]));
            }

            lineSeries1.ItemsSource = null;
            lineSeries1.ItemsSource = dataPoints1;
        }

        private void LoadImagesButton_Click(object sender, RoutedEventArgs e)
        {
            // Load images based on the user input
            string imageFile;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png|All files (*.*)|*.*";

            MessageBox.Show("Valid name format for image files: " +
                "\nBASE**.ext\nBASE is an arbitrary string\n** are 2 digits\n.ext is the image format");

            if (openFileDialog.ShowDialog() == true)
            {
                imageFile = openFileDialog.FileName;
                mrImages.LoadImages(imageFile);

                if (mrImages.AnImage != null)
                {
                    imageViewer.Source = mrImages.AnImage;

                    buttonBack.IsEnabled = true;
                    buttonForth.IsEnabled = true;
                    numImages.IsEnabled = true;
                }
            }
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            mrImages.BackImages();
            imageViewer.Source = mrImages.AnImage;
        }

        private void numImages_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mrImages == null)
            {
                return;
            }

            if (int.TryParse(numImages.Text, out int max))
            {
                mrImages.MaxImages = max;
            }
            else
            {
                MessageBox.Show("Please enter a valid number for max images.");
            }
        }
        private void buttonForth_Click(object sender, RoutedEventArgs e)
        {
            mrImages.ForwardImages();
            imageViewer.Source = mrImages.AnImage;
        }

        private void radioButtonAmbulatory_Checked(object sender, RoutedEventArgs e)
        {
            textBoxRoom.IsEnabled = false;
        }

        private void radioButtonStationary_Checked(object sender, RoutedEventArgs e)
        {
            textBoxRoom.IsEnabled = true;
        }

        private void saveDBButton_Click(object sender, RoutedEventArgs e)
        {
            string patientFile = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All files(*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                patientFile = saveFileDialog.FileName;
                database.SaveData(patientFile);
            }
        }

        private void loadDBButton_Click(object sender, RoutedEventArgs e)
        {
            string patientFile = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All files(*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                patientFile = openFileDialog.FileName;
                database.OpenData(patientFile);
                clearDataGrid();
                database.GetPatients();
            }
            DisplayDatabase();
        }

        void clearDataGrid()
        {
            dataGrid.ItemsSource = null;
            index = 0;
        }

        private void comboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxSort.SelectedItem == null) return;

            MonitorConstants.compareAfter selectedSort = (MonitorConstants.compareAfter)comboBoxSort.SelectedItem;

            PatientComparer pc = new PatientComparer { CA = selectedSort };

            database.GetPatients().Sort(pc);

            DisplayDatabase();
        }

        private void quitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
