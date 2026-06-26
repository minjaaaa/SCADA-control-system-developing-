using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConcentrator.Model
{
    public class DigitalOutput : Tag
    {
        private double initialValue;

        public double InitialValue { get { return initialValue; } set { initialValue = value; OnPropertyChanged("InitialValue"); } }
    }
}
