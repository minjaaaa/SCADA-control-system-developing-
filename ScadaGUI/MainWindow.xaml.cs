using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Threading; // Potrebno za DispatcherPriority
using DataConcentrator;
using DataConcentrator.Model; 
using PLCSimulator;

namespace ScadaGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // OVO OTKOMENTARIŠEMO KADA DODAMO StartScan I ALARME U AnalogInput
             foreach (AnalogInput ai in ContextClass.Instance.Tags.OfType<AnalogInput>())
             {
                 ai.AlarmActivated += OnAlarmActivated;
                 ai.StartScan();
             }
        }

        // --- DEO KOJI SMO MI DODALI ZA TABELU I DUGMAD ---

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                RefreshDataGrid();

                // Pronalazimo taj novododati tag u bazi
                var newTag = ContextClass.Instance.Tags.ToList().LastOrDefault();

                // Odmah mu palimo tred i povezujemo alarm!
                if (newTag is AnalogInput ai)
                {
                    ai.AlarmActivated += OnAlarmActivated;
                    ai.StartScan();
                }
                else if (newTag is DigitalInput di)
                {
                    di.StartScan();
                }
            }
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTags.SelectedItem is Tag selectedTag)
            {
                MessageBoxResult result = MessageBox.Show($"Da li ste sigurni da želite da obrišete tag '{selectedTag.Name}'?",
                                                          "Potvrda brisanja", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var tagToRemove = ContextClass.Instance.Tags.Find(selectedTag.Name);
                        if (tagToRemove != null)
                        {
                            ContextClass.Instance.Tags.Remove(tagToRemove);
                            ContextClass.Instance.SaveChanges();

                            MessageBox.Show("Tag je uspešno obrisan.", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                            RefreshDataGrid();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Greška pri brisanju taga: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Molimo vas da prvo izaberete tag iz tabele koji želite da obrišete.",
                                "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddAlarm_Click(object sender, RoutedEventArgs e)
        {
            AddAlarmWindow alarmWindow = new AddAlarmWindow();
            bool? result = alarmWindow.ShowDialog();
            if (result == true)
            {
                RefreshDataGrid();
            }
        }

        private void RefreshDataGrid()
        {
            var tags = ContextClass.Instance.Tags.ToList();
            var alarms = ContextClass.Instance.Alarms.ToList();
            var activatedAlarms = ContextClass.Instance.ActivatedAlarms.ToList();

            foreach (var tag in tags)
            {
                if (tag is AnalogInput ai)
                {
                    // Pronalaženje svih alarma koji su vezani za ovaj konkretan analogni ulaz
                    var connectedAlarmNames = alarms.Where(a => a.TagName == ai.Name).Select(a => a.Name).ToList();

                    // Provera da li među aktiviranim instancama ima onih koje su u stanju Active ili Acknowledged
                    var activeInstances = activatedAlarms.Where(aa => connectedAlarmNames.Contains(aa.AlarmName)).ToList();

                    if (activeInstances.Any(aa => aa.State == AlarmState.Active))
                    {
                        tag.AlarmStatus = "Active"; // Pokreće crveni okidač u XAML-u
                    }
                    else if (activeInstances.Any(aa => aa.State == AlarmState.Acknowledged))
                    {
                        tag.AlarmStatus = "Acknowledged"; // Pokreće žuti okidač u XAML-u
                    }
                    else
                    {
                        tag.AlarmStatus = "Normal";
                    }
                }
            }

            dataGridTags.ItemsSource = tags;
        }
        private void AcknowledgeAlarm_Click(object sender, RoutedEventArgs e)
        {
            // Proveravamo da li je selektovan tag i da li je taj tag tipa AnalogInput
            if (dataGridTags.SelectedItem is AnalogInput ai)
            {
                // Pronalazimo sve alarme vezane za ovaj tag
                var connectedAlarmNames = ContextClass.Instance.Alarms
                    .Where(a => a.TagName == ai.Name)
                    .Select(a => a.Name).ToList();

                // Pronalazimo sve aktivirane instance tih alarma koje su trenutno u stanju 'Active'
                var activeAlarms = ContextClass.Instance.ActivatedAlarms
                    .Where(aa => connectedAlarmNames.Contains(aa.AlarmName) && aa.State == AlarmState.Active)
                    .ToList();

                if (activeAlarms.Count > 0)
                {
                    // Menjamo im stanje u Acknowledged
                    foreach (var aa in activeAlarms)
                    {
                        aa.State = AlarmState.Acknowledged;
                    }

                    ContextClass.Instance.SaveChanges();
                    RefreshDataGrid(); // Ovo će automatski prebojiti red u žuto!

                    MessageBox.Show("Alarm je uspešno potvrđen (Acknowledged).", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ovaj tag trenutno nema aktivnih (nepotvrđenih) alarma.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Molimo vas da iz tabele izaberete analogni ulaz (AI) koji ima aktivan alarm.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // --- ORIGINALNI KOSTUR KOJI SE BAVI NIKOVANJEM I ALARMIMA ---

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // OVO OTKOMENTARIŠEMO KADA IMPLEMENTIRAMO StopScan
             //abort input threads
             foreach(AnalogInput ai in ContextClass.Instance.Tags.OfType<AnalogInput>())
             {
                 ai.StopScan();
             }
             foreach(DigitalInput di in ContextClass.Instance.Tags.OfType<DigitalInput>())
             {
                 di.StopScan();
             }

             //abort simulator threads
             if (PLC.Instance != null)
             {
                 PLC.Instance.Abort(); // U tvom kosturu je ovde zvalo Abort(), ali preko omotaca je StopSimulator()
             }

             ContextClass.Instance.SaveChanges();
             ContextClass.Instance.Dispose();
        }

        // OVO OTKOMENTARIŠEMO KADA DODAMO Alarm i ActivatedAlarm KLASE
         private void OnAlarmActivated(string alarmName)
         {
             Application.Current.Dispatcher.BeginInvoke(
             DispatcherPriority.Background,
                 new Action(() =>
                 {
                     ActivatedAlarm alarm = new ActivatedAlarm(ContextClass.Instance.Alarms.Find(alarmName));
                     ContextClass.Instance.ActivatedAlarms.Add(alarm);
                     ContextClass.Instance.SaveChanges();

                     RefreshDataGrid();
                 }));
         }
    }
}