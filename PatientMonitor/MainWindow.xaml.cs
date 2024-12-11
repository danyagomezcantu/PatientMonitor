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
            if (timer == null)
                return;
            selectedParameter = (MonitorConstants.Parameter)comboBoxParameters.SelectedIndex;
            UpdateUIForSelectedParameter();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (radioButtonParameter.IsChecked == true)
            {
                chartParameters.Visibility = Visibility.Visible;
                dataGrid.Visibility = Visibility.Collapsed;
                if (labelHighAlarm != null) labelHighAlarm.Visibility = Visibility.Visible;
                if (labelLowAlarm != null) labelLowAlarm.Visibility = Visibility.Visible;
            }
            else if (radioButtonDatabase.IsChecked == true)
            {
                chartParameters.Visibility = Visibility.Collapsed;
                dataGrid.Visibility = Visibility.Visible;
                if (labelHighAlarm != null) labelHighAlarm.Visibility = Visibility.Collapsed;
                if (labelLowAlarm != null) labelLowAlarm.Visibility = Visibility.Collapsed;
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
                    textBoxFrequency.Text = patient.ECG.Frequency.ToString();
                    textBoxLowAlarm.Text = patient.ECG.LowAlarm.ToString();
                    textBoxHighAlarm.Text = patient.ECG.HighAlarm.ToString();
                    comboBoxHarmonics.IsEnabled = true;
                    comboBoxHarmonics.SelectedIndex = patient.ECG.Harmonics - 1;
                    break;
                case MonitorConstants.Parameter.EEG:
                    sliderAmplitude.Value = patient.EEG.Amplitude;
                    textBoxFrequency.Text = patient.EEG.Frequency.ToString();
                    textBoxLowAlarm.Text = patient.EEG.LowAlarm.ToString();
                    textBoxHighAlarm.Text = patient.EEG.HighAlarm.ToString();
                    comboBoxHarmonics.IsEnabled = false;
                    break;
                case MonitorConstants.Parameter.EMG:
                    sliderAmplitude.Value = patient.EMG.Amplitude;
                    textBoxFrequency.Text = patient.EMG.Frequency.ToString();
                    textBoxLowAlarm.Text = patient.EMG.LowAlarm.ToString();
                    textBoxHighAlarm.Text = patient.EMG.HighAlarm.ToString();
                    comboBoxHarmonics.IsEnabled = false;
                    break;
                case MonitorConstants.Parameter.Resp:
                    sliderAmplitude.Value = patient.Resp.Amplitude;
                    textBoxFrequency.Text = patient.Resp.Frequency.ToString();
                    textBoxLowAlarm.Text = patient.Resp.LowAlarm.ToString();
                    textBoxHighAlarm.Text = patient.Resp.HighAlarm.ToString();
                    comboBoxHarmonics.IsEnabled = false;
                    break;
            }
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
            if (patient == null) return;

            bool isValidFreq = double.TryParse(textBoxFrequency.Text, out double parameterFreq);

            if (isValidFreq && parameterFreq >= 0)
            {
                lastValidFrequency = parameterFreq;
                switch (selectedParameter)
                {
                    case MonitorConstants.Parameter.ECG:
                        patient.ECG.Frequency = parameterFreq;
                        break;
                    case MonitorConstants.Parameter.EEG:
                        patient.EEG.Frequency = parameterFreq;
                        break;
                    case MonitorConstants.Parameter.EMG:
                        patient.EMG.Frequency = parameterFreq;
                        break;
                    case MonitorConstants.Parameter.Resp:
                        patient.Resp.Frequency = parameterFreq;
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid non-negative frequency.");
                textBoxFrequency.Text = lastValidFrequency.ToString();
            }

            double lowAlarm = 0.0;
            double highAlarm = 0.0;

            if (!double.TryParse(textBoxLowAlarm.Text, out lowAlarm))
            {
                lowAlarm = 0.0;
            }

            if (!double.TryParse(textBoxHighAlarm.Text, out highAlarm))
            {
                highAlarm = 0.0;
            }

            if (double.TryParse(textBoxFrequency.Text, out double frequency) && frequency < lowAlarm)
            {
                labelLowAlarm.Content = "Low Alarm: " + frequency;
            }
            else if (frequency > highAlarm)
            {
                labelHighAlarm.Content = "High Alarm: " + frequency;
            }
        }

        private void textBoxLowAlarm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (patient == null) return;

            if (double.TryParse(textBoxLowAlarm.Text, out double lowAlarm))
            {
                switch (selectedParameter)
                {
                    case MonitorConstants.Parameter.ECG:
                        patient.ECG.LowAlarm = lowAlarm;
                        break;
                    case MonitorConstants.Parameter.EEG:
                        patient.EEG.LowAlarm = lowAlarm;
                        break;
                    case MonitorConstants.Parameter.EMG:
                        patient.EMG.LowAlarm = lowAlarm;
                        break;
                    case MonitorConstants.Parameter.Resp:
                        patient.Resp.LowAlarm = lowAlarm;
                        break;
                }
            }

            if (double.TryParse(textBoxFrequency.Text, out double frequency) && frequency < lowAlarm)
            {
                labelLowAlarm.Content = "Low Alarm: " + frequency;
            }
            else
            {
                labelLowAlarm.Content = "";
            }
        }

        private void textBoxHighAlarm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (patient == null) return;

            if (double.TryParse(textBoxHighAlarm.Text, out double highAlarm))
            {
                switch (selectedParameter)
                {
                    case MonitorConstants.Parameter.ECG:
                        patient.ECG.HighAlarm = highAlarm;
                        break;
                    case MonitorConstants.Parameter.EEG:
                        patient.EEG.HighAlarm = highAlarm;
                        break;
                    case MonitorConstants.Parameter.EMG:
                        patient.EMG.HighAlarm = highAlarm;
                        break;
                    case MonitorConstants.Parameter.Resp:
                        patient.Resp.HighAlarm = highAlarm;
                        break;
                }
            }

            if (double.TryParse(textBoxFrequency.Text, out double frequency) && frequency > highAlarm) { 
                labelHighAlarm.Content = "High Alarm: " + frequency;
            }
            else
            {
                labelHighAlarm.Content = "";
            }
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
