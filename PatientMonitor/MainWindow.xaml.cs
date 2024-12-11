using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace PatientMonitor
{
    public partial class MainWindow : Window
    {
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
            InitializeComponent();
            database = new Database();
            InitializeDefaults();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += Timer_Tick;

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
            // ItemsSource también podían declararse en el XAML
            comboBoxClinic.ItemsSource = Enum.GetValues(typeof(MonitorConstants.clinic));
            comboBoxParameters.ItemsSource = Enum.GetValues(typeof(MonitorConstants.Parameter));
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
            sliderAmplitude.IsEnabled = true;
            textBoxFrequency.IsEnabled = true;
            textBoxLowAlarm.IsEnabled = true;
            textBoxHighAlarm.IsEnabled = true;
            comboBoxHarmonics.IsEnabled = true;
            startSimulationButton.IsEnabled = true;
            loadImagesButton.IsEnabled = true;
        }

        private void CreatePatientButton_Click(object sender, RoutedEventArgs e)
        {
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
            if (!double.TryParse(textBoxFrequency.Text, out double frequency)) frequency = 0.0;
            if (!double.TryParse(textBoxLowAlarm.Text, out double lowAlarm)) lowAlarm = 0.0;
            if (!double.TryParse(textBoxHighAlarm.Text, out double highAlarm)) highAlarm = 0.0;

            MonitorConstants.clinic clinic = (MonitorConstants.clinic)comboBoxClinic.SelectedIndex;

            if (radioButtonAmbulatory.IsChecked == true)
            {
                patient = new Patient(name, age, dateOfStudy, sliderAmplitude.Value, frequency, comboBoxHarmonics.SelectedIndex + 1, lowAlarm, highAlarm, clinic);
                comboBoxParameters.IsEnabled = true;
                loadImagesButton.IsEnabled = true;
                EnableUIElements();
                UpdateUIForSelectedParameter();
            }
            else if (radioButtonStationary.IsChecked == true)
            {
                if (!int.TryParse(textBoxRoom.Text, out int roomNumber) || roomNumber <= 0)
                {
                    MessageBox.Show("Please enter a valid room number for the stationary patient.");
                    return;
                }

                stationary = new Stationary(name, age, dateOfStudy, sliderAmplitude.Value, frequency, comboBoxHarmonics.SelectedIndex + 1, lowAlarm, highAlarm, clinic, roomNumber);
                comboBoxParameters.IsEnabled = true;
                loadImagesButton.IsEnabled = true;
                EnableUIElements();
                UpdateUIForSelectedParameter();

                patient = stationary;
            }
            else
            {
                MessageBox.Show("Please select if the patient is Ambulatory or Stationary.");
                return;
            }

            database.AddPatient(patient);
            DisplayDatabase();
        }

        private void DisplayDatabase()
        {
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = database.GetAllPatients();
        }

        private void ComboBoxParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timer == null) return;

            selectedParameter = (MonitorConstants.Parameter)comboBoxParameters.SelectedIndex;
            UpdateUIForSelectedParameter();

            patient?.UpdateAlarms(selectedParameter);
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
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
            timer.Stop();
            stopSimulationButton.IsEnabled = false;
        }

        private void DisplayTime()
        {
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
            if (patient == null) return;

            if (comboBoxHarmonics.SelectedIndex != -1)
            {
                patient.ECG.Harmonics = comboBoxHarmonics.SelectedIndex + 1;
            }
        }

        private void textBoxAge_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void SliderAmplitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
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
            if (patient == null) return;

            double[] lastSamples = patient.GetLastNSamples(512);
            if (lastSamples.Length < 512)
            {
                MessageBox.Show("Not enough samples collected for FFT calculation.");
                return;
            }

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
    }
}
