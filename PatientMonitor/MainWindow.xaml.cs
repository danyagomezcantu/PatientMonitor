using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Controls.DataVisualization.Charting.Compatible;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;


namespace PatientMonitor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private Patient patient;
        private MonitorConstants.Parameter selectedParameter = MonitorConstants.Parameter.ECG;
        private DispatcherTimer timer;
        private double timeIndex;
        private List<KeyValuePair<int, double>> dataPoints;
        private int index;
        private double lastValidFrequency = 0.0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDefaults();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;

            dataPoints = new List<KeyValuePair<int, double>>();
            lineSeries.ItemsSource = dataPoints;

            // Attach ComboBox event handler after full initialization to prevent premature calls
            comboBoxParameters.SelectionChanged += ComboBoxParameters_SelectionChanged;
        }

        private void InitializeDefaults()
        {
            patient = new Patient("Default", 0, DateTime.Now, 0, 0, 1);
            comboBoxParameters.SelectedIndex = 0; // Default to ECG
            UpdateControlsForParameter();
        }

        private void textBoxAge_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void CreatePatientButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text) || !int.TryParse(textBoxAge.Text, out int age) || datePickerMonitor.SelectedDate == null)
            {
                MessageBox.Show("Please enter valid patient details.");
                return;
            }

            patient = new Patient(textBoxName.Text, age, datePickerMonitor.SelectedDate.Value, sliderAmplitude.Value, double.TryParse(textBoxFrequency.Text, out double freq) ? freq : 0.0);
            updatePatientButton.IsEnabled = true;
            comboBoxParameters.IsEnabled = true;
            startSimulationButton.IsEnabled = true;
            UpdateControlsForParameter();
        }

        private void ComboBoxHarmonics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (patient != null)
            {
                patient.ECGHarmonics = comboBoxHarmonics.SelectedIndex + 1;
            }
        }

        private void ComboBoxParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timer == null)
                return;

            selectedParameter = (MonitorConstants.Parameter)comboBoxParameters.SelectedIndex;
            UpdateControlsForParameter();

            if (timer.IsEnabled)
            {
                // timeIndex = 0;
                // dataPoints.Clear();
            }
        }

        private void UpdateControlsForParameter()
        {
            if (patient == null) return;

            switch (selectedParameter)
            {
                case MonitorConstants.Parameter.ECG:
                    sliderAmplitude.Value = patient.ECGAmplitude;
                    textBoxFrequency.Text = patient.ECGFrequency.ToString();
                    comboBoxHarmonics.IsEnabled = true;
                    comboBoxHarmonics.SelectedIndex = patient.ECGHarmonics - 1;
                    break;
                case MonitorConstants.Parameter.EEG:
                    sliderAmplitude.Value = patient.EEGAmplitude;
                    textBoxFrequency.Text = patient.EEGFrequency.ToString();
                    comboBoxHarmonics.IsEnabled = false;
                    break;
                case MonitorConstants.Parameter.EMG:
                    sliderAmplitude.Value = patient.EMGAmplitude;
                    textBoxFrequency.Text = patient.EMGFrequency.ToString();
                    comboBoxHarmonics.IsEnabled = false;
                    break;
                case MonitorConstants.Parameter.Resp:
                    sliderAmplitude.Value = patient.RespAmplitude;
                    textBoxFrequency.Text = patient.RespFrequency.ToString();
                    comboBoxHarmonics.IsEnabled = false;
                    break;
            }
        }

        private void SliderAmplitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (patient == null) return;

            switch (selectedParameter)
            {
                case MonitorConstants.Parameter.ECG: patient.ECGAmplitude = sliderAmplitude.Value; break;
                case MonitorConstants.Parameter.EEG: patient.EEGAmplitude = sliderAmplitude.Value; break;
                case MonitorConstants.Parameter.EMG: patient.EMGAmplitude = sliderAmplitude.Value; break;
                case MonitorConstants.Parameter.Resp: patient.RespAmplitude = sliderAmplitude.Value; break;
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
                        patient.ECGFrequency = parameterFreq;
                        break;
                    case MonitorConstants.Parameter.EEG:
                        patient.EEGFrequency = parameterFreq;
                        break;
                    case MonitorConstants.Parameter.EMG:
                        patient.EMGFrequency = parameterFreq;
                        break;
                    case MonitorConstants.Parameter.Resp:
                        patient.RespFrequency = parameterFreq;
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid non-negative frequency.");
                textBoxFrequency.Text = lastValidFrequency.ToString(); // Revert to last valid frequency
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

            if (sliderAmplitude.Value == 0 || string.IsNullOrWhiteSpace(textBoxFrequency.Text) || double.TryParse(textBoxFrequency.Text, out double frequency) && frequency == 0)
            {
                MessageBox.Show("Amplitude and Frequency cannot be zero. Please adjust the values.");
                return;
            }

            timeIndex = 0;
            index = 0;
            dataPoints.Clear();
            timer.Start();

            // Enable the Stop button once the simulation is running
            stopSimulationButton.IsEnabled = true;
        }

        private void StopSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            stopSimulationButton.IsEnabled = false; // Disable stop button after stopping
        }
    }
}
