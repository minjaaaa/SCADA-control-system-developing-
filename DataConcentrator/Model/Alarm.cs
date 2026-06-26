using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConcentrator.Model
{
    // Enumeracija za smer aktivacije alarma
    public enum AlarmType
    {
        HIGH, // Aktivira se kada vrednost pređe IZNAD granice
        LOW   // Aktivira se kada vrednost padne ISPOD granice
    }

    public class Alarm
    {
        [Key]
        public string Name { get; set; }

        public double Limit { get; set; }

        public AlarmType Type { get; set; }

        public string Message { get; set; }

        // Strani ključ koji povezuje alarm sa određenim analognim ulazom (AI)
        public string TagName { get; set; }
    }
}
