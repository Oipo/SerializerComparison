using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoxPlotter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public SeriesCollection SeriesCollection { get; set; }

        public string[] Labels { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
                new CandleSeries
                {
                    Values = new ChartValues<OhlcPoint>
                    {
                    }
                }
            };

            Labels = new string[0];

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateAllOnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if(dialog.ShowDialog() == true)
            {
                var lines = File.ReadAllLines(dialog.FileName);
                int lineCounter = 0;

                do
                {
                    if (!lines[lineCounter].Contains("Starting measurements"))
                    {
                        lineCounter++;
                        continue;
                    }

                    lineCounter++;
                    break;
                } while (lineCounter < lines.Length);

                SeriesCollection.First().Values.Clear();

                if(lineCounter == lines.Length)
                {
                    return;
                }

                List<string> newLabels = new List<string>();

                do
                {
                    if (lines[lineCounter].Contains("Stopping measurements"))
                    {
                        break;
                    }

                    var lineSegments = lines[lineCounter].Split('|');

                    if(lineSegments.Length == 4)
                    {
                        var info = lineSegments[3].Split(':');
                        var name = info[0].Replace("Stream Serialization", "StrSer").Replace("Stream Deserialization", "StrDes").Replace("Serialization", "Ser").Replace("Deserialization", "Des");
                        var measurements = new List<double>();
                        newLabels.Add(name);

                        for (int i = 1; i < info.Length; i++)
                        {
                            measurements.Add(double.Parse(info[i], CultureInfo.InvariantCulture));
                        }

                        measurements.Sort();
                        var open = measurements.Skip((int)Math.Floor(measurements.Count*0.2)).First();
                        var close = measurements.Skip((int)Math.Floor(measurements.Count*0.8)).First();

                        SeriesCollection.First().Values.Add(new OhlcPoint(open, measurements.Max(), measurements.Min(), close));
                    }

                    lineCounter++;
                } while (lineCounter < lines.Length);

                Labels = newLabels.ToArray();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Labels"));
            }
        }
    }
}
