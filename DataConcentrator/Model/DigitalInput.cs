using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataConcentrator.Model
{
    public class DigitalInput : Tag
    {
        private double scanTime;
        private bool isScanOn;

        public double ScanTime { get { return scanTime; } set { scanTime = value; OnPropertyChanged("ScanTime"); } }
        public bool IsScanOn { get { return isScanOn; } set { isScanOn = value; OnPropertyChanged("IsScanOn"); } }

        private bool keepScanning;

        public void StartScan()
        {
            if (IsScanOn)
            {
                keepScanning = true;
                Thread scanThread = new Thread(ScanLoop);
                scanThread.IsBackground = true;

                PLC.tagThreads[this.Name] = scanThread;
                scanThread.Start();
            }
        }

        public void StopScan()
        {
            keepScanning = false;
            if (PLC.tagThreads.ContainsKey(this.Name))
            {
                Thread threadToStop = PLC.tagThreads[this.Name];
                if (threadToStop != null && threadToStop.IsAlive)
                {
                    threadToStop.Abort();
                }
                PLC.tagThreads.Remove(this.Name);
            }
        }

        private void ScanLoop()
        {
            while (keepScanning)
            {
                // PLCSimulatorManager iz kostura koristi GetAnalogValue i za digitalne vrednosti (koje su 0 ili 1)
                double currentValue = PLC.Instance.GetAnalogValue(this.IOAddress);

                // Trenutno samo očitavamo vrednost. Ukoliko zadatak zahteva, ovde možemo upisati tu vrednost u bazu ili istoriju

                Thread.Sleep((int)(ScanTime * 1000));
            }
        }
    }
        
}
