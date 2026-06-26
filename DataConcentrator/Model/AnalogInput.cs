using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
