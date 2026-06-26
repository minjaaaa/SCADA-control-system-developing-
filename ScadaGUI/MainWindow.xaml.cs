using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Threading;
using DataConcentrator;
using PLCSimulator;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
    public MainWindow()
        {
            InitializeComponent();

            //foreach (AnalogInput ai in ...)
            //{
            //    ai.AlarmActivated += OnAlarmActivated;
            //    ai.StartScan();
            //}

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        //    //abort input threads
        //    foreach(AnalogInput ai in ContextClass.Instance.AnalogInputs)
        //    {
        //        ai.StopScan();
        //    }
        //    foreach(DigitalInput di in ContextClass.Instance.DigitalInputs)
        //    {
        //        di.StopScan();
        //    }

        //    //abort simulator threads
        //    if (PLC.Instance != null)
        //    {
        //        PLC.Instance.Abort();
        //    }

        //    ContextClass.Instance.SaveChanges();
        //    ContextClass.Instance.Dispose();
        }

        //static void OnAlarmActivated(string alarmName)
        //{
        //    Application.Current.Dispatcher.BeginInvoke(
        //    DispatcherPriority.Background,
        //        new Action(() =>
        //        {
        //            ActivatedAlarm alarm = new ActivatedAlarm(ContextClass.Instance.Alarms.Find(alarmName));
        //            ContextClass.Instance.ActivatedAlarms.Add(alarm);
        //            ContextClass.Instance.SaveChanges();
        //        }));

        //}

    }
}
