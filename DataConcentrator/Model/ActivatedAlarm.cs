using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator.Model
{//predstavlja istoriju aktiviranih alarma na sistemu
    // Stanja u kojima aktivirani alarm može da se nađe prema specifikaciji
    public enum AlarmState
    {
        Active,
        Acknowledged,
        Inactive
    }

    public class ActivatedAlarm
    {
        [Key]
        public int Id { get; set; }

        public string AlarmName { get; set; }

        public DateTime Timestamp { get; set; }

        public AlarmState State { get; set; }

        public double TriggerValue { get; set; }

        // Podrazumevani konstruktor za Entity Framework
        public ActivatedAlarm()
        {
            Timestamp = DateTime.Now;
            State = AlarmState.Active;
        }

        // Konstruktor koji se koristi u MainWindow.xaml.cs kosturu
        public ActivatedAlarm(Alarm alarm) : this()
        {
            if (alarm != null)
            {
                this.AlarmName = alarm.Name;
            }
        }
    }
}
