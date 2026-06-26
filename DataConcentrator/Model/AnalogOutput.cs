using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConcentrator.Model
{
    public class AnalogOutput : Tag
    {
        private double initialValue;
        private double lowLimit;
        private double highLimit;
        private string units;

        public double InitialValue { get { return initialValue; } set { initialValue = value; OnPropertyChanged("InitialValue"); } }
        public double LowLimit { get { return lowLimit; } set { lowLimit = value; OnPropertyChanged("LowLimit"); } }
        public double HighLimit { get { return highLimit; } set { highLimit = value; OnPropertyChanged("HighLimit"); } }
        public string Units { get { return units; } set { units = value; OnPropertyChanged("Units"); } }
    }
}
