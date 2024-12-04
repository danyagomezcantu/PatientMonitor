using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;
using MonitorImageStrings;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace PatientMonitor
{
    public partial class MainWindow : Window
    {
        private Patient patient;
        private MonitorConstants.Parameter selectedParameter = MonitorConstants.Parameter.ECG;
        private DispatcherTimer timer;
        private double timeIndex;
        private List<KeyValuePair<int, double>> dataPoints;
        private List<KeyValuePair<int, double>> dataPoints1;
        private int index;
        private double lastValidFrequency = 0.0;
        private Spektrum spektrum;
        MRImages mrImages;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDefaults();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;

            dataPoints = new List<KeyValuePair<int, double>>();
            dataPoints1 = new List<KeyValuePair<int, double>>();
            lineSeries.ItemsSource = dataPoints;
            lineSeries1.ItemsSource = dataPoints1;

            comboBoxParameters.SelectionChanged += ComboBoxParameters_SelectionChanged;

            spektrum = new Spektrum(128); // FFT con 128 samples en vez de 512 samples
            mrImages = new MRImages();

            DisableUIElements();
        }

        private void InitializeDefaults()
        {
            patient = null;
            comboBoxParameters.SelectedIndex = 0;

            buttonBack.IsEnabled = false;
            buttonForth.IsEnabled = false;
            numImages.IsEnabled = false;
            numImages.Text = "10";
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (patient == null) return;

            // Fetch the next time sample
            double nextSample = patient.NextSample(timeIndex, selectedParameter);

            // Add the sample to the time-based data list
            dataPoints.Add(new KeyValuePair<int, double>(index++, nextSample));

            // Remove oldest sample if list exceeds desired size
            if (dataPoints.Count > 200)
            {
                dataPoints.RemoveAt(0);
            }

            // Update the time line series
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

        private void CreatePatientButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text) || !int.TryParse(textBoxAge.Text, out int age) || datePickerMonitor.SelectedDate == null)
            {
                MessageBox.Show("Please enter valid patient details.");
                return;
            }

            double lowAlarm = double.TryParse(textBoxLowAlarm.Text, out double lAlarm) ? lAlarm : 0.0;
            double highAlarm = double.TryParse(textBoxHighAlarm.Text, out double hAlarm) ? hAlarm : 0.0;

            patient = new Patient(textBoxName.Text, age, datePickerMonitor.SelectedDate.Value, sliderAmplitude.Value, double.TryParse(textBoxFrequency.Text, out double freq) ? freq : 0.0, 1, lowAlarm, highAlarm);

            updatePatientButton.IsEnabled = true;
            comboBoxParameters.IsEnabled = true;
            loadImagesButton.IsEnabled = true; // Enable Load Images Button
            EnableUIElements(); // Enable controls after creating a patient
            UpdateUIForSelectedParameter();
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

        private void ComboBoxParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timer == null)
                return;

            selectedParameter = (MonitorConstants.Parameter)comboBoxParameters.SelectedIndex;
            UpdateUIForSelectedParameter();

            if (timer.IsEnabled)
            {
                // timeIndex = 0;
                // dataPoints.Clear();
            }
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
                lowAlarm = 0.0;  // Default to 0 if parsing fails
            }

            if (!double.TryParse(textBoxHighAlarm.Text, out highAlarm))
            {
                highAlarm = 0.0;  // Default to 0 if parsing fails
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

            // Fetch the last 512 samples for the FFT calculation
            double[] lastSamples = patient.GetLastNSamples(512);
            if (lastSamples.Length < 128)
            {
                MessageBox.Show("Not enough samples collected for FFT calculation.");
                return;
            }

            // Perform the FFT
            double[] fftOutput = spektrum.FFT(lastSamples, lastSamples.Length);

            // Update the frequency data points
            dataPoints1.Clear();
            for (int i = 0; i < fftOutput.Length; i++)
            {
                dataPoints1.Add(new KeyValuePair<int, double>(i, fftOutput[i]));
            }

            // Update the frequency line series
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

            // Show the dialog and check if the result is OK
            if (openFileDialog.ShowDialog() == true)
            {
                imageFile = openFileDialog.FileName;
                mrImages.LoadImages(imageFile);

                // Update the viewer with the first image if it exists
                if (mrImages.AnImage != null)
                {
                    imageViewer.Source = mrImages.AnImage;

                    // Enable other controls after loading images
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
    }
}
