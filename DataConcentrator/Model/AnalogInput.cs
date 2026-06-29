using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using static System.Net.Mime.MediaTypeNames;
//using System.Windows;

namespace DataConcentrator.Model
{
    public class AnalogInput : Tag
    {
        private double scanTime;
        private bool isScanOn;
        private double lowLimit;
        private double highLimit;
        private string units;
        private double deadband;
        private double hysteresis;

        public double ScanTime { get { return scanTime; } set { scanTime = value; OnPropertyChanged("ScanTime"); } }
        public bool IsScanOn { get { return isScanOn; } set { isScanOn = value; OnPropertyChanged("IsScanOn"); } }
        public double LowLimit { get { return lowLimit; } set { lowLimit = value; OnPropertyChanged("LowLimit"); } }
        public double HighLimit { get { return highLimit; } set { highLimit = value; OnPropertyChanged("HighLimit"); } }
        public string Units { get { return units; } set { units = value; OnPropertyChanged("Units"); } }
        public double Deadband { get { return deadband; } set { deadband = value; OnPropertyChanged("Deadband"); } }
        public double Hysteresis { get { return hysteresis; } set { hysteresis = value; OnPropertyChanged("Hysteresis"); } }

        // Alarmi ce biti dodati kasnije kada napravimo klasu Alarm
        public event Action<Alarm, double, string> AlarmActivated;

        private bool keepScanning;

        public void StartScan()
        {
            // Ako je korisnik čekirao "Scan On", pokrećemo tred
            if (IsScanOn)
            {
                keepScanning = true;
                Thread scanThread = new Thread(ScanLoop);
                scanThread.IsBackground = true; // Da bi se tred ugasio kada se ugasi aplikacija

                // Čuvamo tred u PLC rečniku iz kostura
                PLC.tagThreads[this.Name] = scanThread;
                scanThread.Start();
            }
        }

        public void StopScan()
        {
            keepScanning = false;
            // Prekidamo tred ako postoji u rečniku
            if (PLC.tagThreads.ContainsKey(this.Name))
            {
                PLC.tagThreads.Remove(this.Name);
            }
        }

        private void ScanLoop()
        {
            while (keepScanning)
            {
                // 1. Očitavanje vrednosti sa PLC-a za zadatu I/O adresu
                double currentValue = PLC.Instance.GetAnalogValue(this.IOAddress);
                List<Alarm> tagAlarms = new List<Alarm>();

                // SIGURNOSNA MREŽA: Hvata greške ako je baza u međuvremenu obrisala tag
                try
                {
                    // 2. Bezbedno čitanje alarma i UPIS ISTORIJE u bazu
                    lock (ContextClass.Instance)
                    {
                        // Ako je nit u međuvremenu zaustavljena (ugašen tag), izlazi iz petlje pre upisa
                        if (!keepScanning) break;

                        tagAlarms = ContextClass.Instance.Alarms.Where(a => a.TagName == this.Name).ToList();

                        TagValue newRecord = new TagValue
                        {
                            TagName = this.Name,
                            Value = currentValue,
                            Timestamp = DateTime.Now
                        };
                        ContextClass.Instance.TagValues.Add(newRecord);
                        ContextClass.Instance.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    // Ako je tag obrisan iz baze, Entity Framework će baciti grešku,
                    // mi je ovde hvatamo i tiho gasimo nit bez pucanja SCADA sistema!
                    break;
                }

                // 3. Provera da li je vrednost prešla granice alarma
                foreach (var alarm in tagAlarms)
                {
                    if (alarm.Type == AlarmType.HIGH && currentValue > alarm.Limit)
                    {
                        AlarmActivated?.Invoke(alarm, currentValue, this.Units);
                    }
                    else if (alarm.Type == AlarmType.LOW && currentValue < alarm.Limit)
                    {
                        AlarmActivated?.Invoke(alarm, currentValue, this.Units);
                    }
                }

                // 4. Uspavljivanje treda na ScanTime sekundi
                Thread.Sleep((int)(ScanTime * 1000));
            }
        }
    }
}
