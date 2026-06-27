using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConcentrator
{
    [Flags]
    public enum LogCategory
    {
        None = 0,
        AppStart = 1,       // Bit 0
        Alarms = 2,         // Bit 1
        TagManagement = 4,  // Bit 2
        ImportExport = 8,   // Bit 3
        Errors = 16         // Bit 4
    }

    public static class Logger
    {
        // Fajl će se automatski kreirati u bin/Debug folderu tvoje aplikacije
        private static readonly string logFilePath = "system.log";
        private static readonly string traceWordFilePath = "traceword.txt";

        // Trenutna vrednost maske (po defaultu 31, što znači svi bitovi su uključeni: 1+2+4+8+16)
        public static int TraceWord { get; set; } = 31;

        static Logger()
        {
            LoadTraceWord();
        }

        // Metoda proverava da li je bit za traženu kategoriju uključen u TraceWord-u
        public static void Log(string actionMessage, LogCategory category)
        {
            // Bitwise AND operacija: Ako rezultat nije 0, bit je uključen!
            if (((int)category & TraceWord) != 0)
            {
                try
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{category}] {actionMessage}";
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception) { }
            }
        }

        public static void SaveTraceWord(int newTraceWord)
        {
            TraceWord = newTraceWord;
            File.WriteAllText(traceWordFilePath, TraceWord.ToString());
        }

        private static void LoadTraceWord()
        {
            if (File.Exists(traceWordFilePath))
            {
                if (int.TryParse(File.ReadAllText(traceWordFilePath), out int savedWord))
                {
                    TraceWord = savedWord;
                }
            }
        }
    }
}
