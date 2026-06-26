using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConcentrator
{
    public class ContextClass : DbContext
    {
        //singleton pattern
        private static ContextClass instance;

        public static ContextClass Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ContextClass();
                }
                return instance;
            }
        }

        public DbSet<Tag> Tags { get; set; }

        //public DbSet<Alarm> Alarms { get; set; }

        //public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }

    }
}
