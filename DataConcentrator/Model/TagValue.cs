using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator.Model
{
    public class TagValue
    {
        [Key]
        public int Id { get; set; }

        public string TagName { get; set; }

        public double Value { get; set; }

        public DateTime Timestamp { get; set; }

        public TagValue()
        {
            Timestamp = DateTime.Now;
        }
    }
}
