using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConcentrator
{
    public static class Logger
    {
        // Fajl će se automatski kreirati u bin/Debug folderu tvoje aplikacije
        private static readonly string logFilePath = "system.log";

        public static void Log(string actionMessage)
        {
            try
            {
                // Formatiranje vremena prema zahtevu: vremenski trenutak + akcija
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {actionMessage}";

                // AppendAllText automatski pravi fajl ako ne postoji i dodaje novi red
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception)
            {
                // U slučaju da je fajl zaključan, ignorišemo grešku da ne bi srušila SCADA sistem
            }
        }
    }
}
