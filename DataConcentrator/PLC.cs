using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PLCSimulator;

namespace DataConcentrator
{
    public class PLC
    {
        public static PLCSimulatorManager instance;

        public static Dictionary<string, Thread> tagThreads = new Dictionary<string, Thread>();

        public static PLCSimulatorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PLCSimulatorManager();
                    instance.StartPLCSimulator();
                }
                return instance;
            }
        }

        public void StopSimulator()
        {
            instance.Abort();
        }
         
    }
}
