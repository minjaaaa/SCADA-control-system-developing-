using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConcentrator.Model
{
    public class DigitalInput : Tag
    {
        private double scanTime;
        private bool isScanOn;

        public double ScanTime { get { return scanTime; } set { scanTime = value; OnPropertyChanged("ScanTime"); } }
        public bool IsScanOn { get { return isScanOn; } set { isScanOn = value; OnPropertyChanged("IsScanOn"); } }
    }
}
