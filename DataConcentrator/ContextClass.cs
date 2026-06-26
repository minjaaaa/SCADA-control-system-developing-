using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataConcentrator.Model;

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
        public ContextClass() : base()
        {
            // Ova linija govori EF-u: "Ako primetiš da smo dodali nove klase ili propertije, obriši staru bazu i napravi novu sa novim tabelama."
            Database.SetInitializer<ContextClass>(new DropCreateDatabaseIfModelChanges<ContextClass>());
        }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<Alarm> Alarms { get; set; }

        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }

        //tabela za istoriju vrednosti tagova (za svaki tag se beleži vreme i vrednost)
        public DbSet<TagValue> TagValues { get; set; }
    }
}
