using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PatientMonitor
{
    public partial class MainWindow : Window
    {
        private Patient patient;
        private DispatcherTimer timer;
        private List<Point> ecgDataPoints;

        public MainWindow()
        {
            InitializeComponent();
            ecgDataPoints = new List<Point>();
            LineSeriesECG.ItemsSource = ecgDataPoints;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;

            sliderAmplitude.Value = 5;
            textBoxFrequency.Text = "0";
            ToggleControls(false);
        }

        private void ToggleControls(bool isEnabled)
        {
            buttonUpdatePatient.IsEnabled = isEnabled;
            buttonStartSimulation.IsEnabled = isEnabled;
            sliderAmplitude.IsEnabled = false;
            textBoxFrequency.IsEnabled = false;
            comboBoxHarmonics.IsEnabled = false;
            buttonStopSimulation.IsEnabled = false;
        }

        private void ButtonCreatePatient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = textBoxName.Text;
                if (!int.TryParse(textBoxAge.Text, out int age))
                {
                    MessageBox.Show("Please enter a valid age.");
                    return;
                }
                DateTime date = datePickerMonitor.SelectedDate ?? DateTime.Now; // Por si hay error en el DatePicker
                patient = new Patient(name, age, date, amplitude: 5, frequency: 0, harmonics: 1);

                ToggleControls(true);
                buttonCreatePatient.IsEnabled = false;
                textBoxFrequency.IsEnabled = true;
                sliderAmplitude.IsEnabled = true;
                comboBoxHarmonics.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating patient: " + ex.Message);
            }
        }

        private void ButtonUpdatePatient_Click(object sender, RoutedEventArgs e) { }

        private void TextBoxFrequency_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxFrequency.Text == "0" || string.IsNullOrEmpty(textBoxFrequency.Text))
            {
                textBoxFrequency.Text = "0";
                if (patient != null) patient.ECGFrequency = 0;
            }
            else if (patient != null && double.TryParse(textBoxFrequency.Text, out double frequency))
            {
                patient.ECGFrequency = frequency;
            }
        }

        private void ComboBoxHarmonics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (patient != null)
            {
                patient.ECGHarmonics = comboBoxHarmonics.SelectedIndex + 1;
            }
        }

        private void SliderAmplitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (patient != null)
            {
                patient.ECGAmplitude = sliderAmplitude.Value;
            }
        }

        private void ButtonStartSimulation_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(textBoxFrequency.Text, out double frequency) || frequency == 0)
            {
                MessageBox.Show("Please enter a valid frequency to start the simulation.");
                return;
            }

            patient.ECGFrequency = frequency;
            timer.Start();
            buttonStopSimulation.IsEnabled = true;
            ToggleControls(false);
            sliderAmplitude.IsEnabled = true;
            textBoxFrequency.IsEnabled = true;
            comboBoxHarmonics.IsEnabled = true;
        }

        private void ButtonStopSimulation_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            buttonStopSimulation.IsEnabled = false;
            ToggleControls(true);

            ecgDataPoints.Clear();
            LineSeriesECG.ItemsSource = null;
            LineSeriesECG.ItemsSource = ecgDataPoints;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (patient != null)
            {
                double timeIndex = ecgDataPoints.Count;
                double sample = patient.NextSample(timeIndex);
                ecgDataPoints.Add(new Point(timeIndex, sample));

                LineSeriesECG.Refresh();
            }
        }

        private void textBoxAge_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int result);
        }
    }
}