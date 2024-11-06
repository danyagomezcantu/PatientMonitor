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


namespace PatientMonitor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<KeyValuePair<int, double>> dataPoints;
        private DispatcherTimer timer;
        private int index = 0;
        Patient patient;
        MonitorConstants.Parameter parameter = MonitorConstants.Parameter.ECG;

        public MainWindow()
        {
            InitializeComponent();
            patient = new Patient(100, (double)1.0);
            dataPoints = new ObservableCollection<KeyValuePair<int, double>>();
            lineSeriesECG.ItemsSource = dataPoints; // Bind the series to the data points


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.025); // Set timer to tick every second
            timer.Tick += Timer_Tick;
            timer.Start();

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Generate a new data point
            dataPoints.Add(new KeyValuePair<int, double>(index++, patient.NextSample(index, parameter)));

            // Optional: Remove old points to keep the chart clean
            if (dataPoints.Count > 200) // Maximum number of points
            {
                dataPoints.RemoveAt(0); // Remove the oldest point
            }
        }

        private void buttonCreatePatient_Click(object sender, RoutedEventArgs e)
        {
            buttonParameter.IsEnabled = true;
        }

        private void buttonParameter_Click(object sender, RoutedEventArgs e)
        {
            sliderECG.IsEnabled = true;
            comboBoxParameters.IsEnabled = true;
        }

        private void sliderECG_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch (parameter)
            {
                case (MonitorConstants.Parameter.ECG):
                    patient.ECGAmplitude = sliderECG.Value;
                    break;
                case (MonitorConstants.Parameter.EMG):
                    patient.EMGAmplitude = sliderECG.Value;
                    break;
                default: break;
            }

        }

        private void sliderECG_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider.IsEnabled) slider.ValueChanged += sliderECG_ValueChanged;
            else slider.ValueChanged -= sliderECG_ValueChanged;
        }

        private void comboBoxParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            parameter = (MonitorConstants.Parameter)comboBoxParameters.SelectedIndex;
        }
        private void comboBoxParameters_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if (combo.IsEnabled)
                combo.SelectionChanged += comboBoxParameters_SelectionChanged;
            else
                combo.SelectionChanged -= comboBoxParameters_SelectionChanged;
        }
    }
}
