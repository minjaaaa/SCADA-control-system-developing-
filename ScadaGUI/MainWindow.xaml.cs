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
            // foreach (AnalogInput ai in ContextClass.Instance.Tags.OfType<AnalogInput>())
            // {
            //     ai.AlarmActivated += OnAlarmActivated;
            //     ai.StartScan();
            // }
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

        private void RefreshDataGrid()
        {
            dataGridTags.ItemsSource = ContextClass.Instance.Tags.ToList();
        }

        // --- ORIGINALNI KOSTUR KOJI SE BAVI NIKOVANJEM I ALARMIMA ---

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // OVO OTKOMENTARIŠEMO KADA IMPLEMENTIRAMO StopScan
            // abort input threads
            // foreach(AnalogInput ai in ContextClass.Instance.Tags.OfType<AnalogInput>())
            // {
            //     ai.StopScan();
            // }
            // foreach(DigitalInput di in ContextClass.Instance.Tags.OfType<DigitalInput>())
            // {
            //     di.StopScan();
            // }

            // abort simulator threads
            // if (PLC.Instance != null)
            // {
            //     PLC.Instance.StopSimulator(); // U tvom kosturu je ovde zvalo Abort(), ali preko omotaca je StopSimulator()
            // }

            // ContextClass.Instance.SaveChanges();
            // ContextClass.Instance.Dispose();
        }

        // OVO OTKOMENTARIŠEMO KADA DODAMO Alarm i ActivatedAlarm KLASE
        // static void OnAlarmActivated(string alarmName)
        // {
        //     Application.Current.Dispatcher.BeginInvoke(
        //     DispatcherPriority.Background,
        //         new Action(() =>
        //         {
        //             ActivatedAlarm alarm = new ActivatedAlarm(ContextClass.Instance.Alarms.Find(alarmName));
        //             ContextClass.Instance.ActivatedAlarms.Add(alarm);
        //             ContextClass.Instance.SaveChanges();
        //         }));
        // }
    }
}