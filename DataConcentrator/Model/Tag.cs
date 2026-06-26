using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataConcentrator
{
    public abstract class Tag : INotifyPropertyChanged
    {
        private string name;
        private string description;
        private string ioAddress;
        private string alarmStatus = "Normal";

        [Key]
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }

        [NotMapped]
        public string AlarmStatus
        {
            get { return alarmStatus; }
            set { alarmStatus = value; OnPropertyChanged("AlarmStatus"); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged("Description"); }
        }

        public string IOAddress
        {
            get { return ioAddress; }
            set { ioAddress = value; OnPropertyChanged("IOAddress"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
