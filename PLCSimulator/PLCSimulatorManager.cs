using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PLCSimulator
{
    /// <summary>
    /// PLC Simulator
    /// 
    /// 4 x ANALOG INPUT : ADDR001 - ADDR004
    /// 4 x ANALOG OUTPUT: ADDR005 - ADDR008
    /// 1 x DIGITAL INPUT: ADDR009, ADDR0011-ADDR0013
    /// 1 x DIGITAL OUTPUT: ADDR010, ADDR0014-ADDR0016
    /// </summary>
    public class PLCSimulatorManager
    {
        private Dictionary<string, double> addressValues;
        private object locker = new object();
        private Thread t1;
        private Thread t2;
        
        public PLCSimulatorManager()
        {
            addressValues = new Dictionary<string, double>();

            // TODO: dodati adrese
            // AI
            addressValues.Add("ADDR001", 0);

            // AO
            addressValues.Add("ADDR005", 0);

            // DI
            addressValues.Add("ADDR009", 0);

            // DO
            addressValues.Add("ADDR010", 0);
 
        }

        public void StartPLCSimulator()
        {
            t1 = new Thread(GeneratingAnalogInputs);
            t1.Start();

            t2 = new Thread(GeneratingDigitalInputs);
            t2.Start();
        }

        private void GeneratingAnalogInputs()
        {
            while (true)
            {
                
                Thread.Sleep(100);

                lock (locker)
                {
                    addressValues["ADDR001"] = 100 * Math.Sin((double)DateTime.Now.Second / 60 * Math.PI); //SINE
                    addressValues["ADDR002"] = 100 * DateTime.Now.Second / 60; //RAMP
                    addressValues["ADDR003"] = 50 * Math.Cos((double)DateTime.Now.Second / 60 * Math.PI); //COS
                    addressValues["ADDR004"] = RandomNumberBetween(0, 50);  //rand
                }
            }
        }

        private void GeneratingDigitalInputs()
        {
            while (true)
            {
                Thread.Sleep(1000);

                lock (locker)
                {
                    if (addressValues["ADDR009"] == 0)
                    {
                        addressValues["ADDR009"] = 1;
                    }
                    else
                    {
                        addressValues["ADDR009"] = 0;
                    }

                    if (addressValues["ADDR011"] == 0)
                    {
                        addressValues["ADDR011"] = 1;
                    }
                    else
                    {
                        addressValues["ADDR011"] = 0;
                    }

                    if (addressValues["ADDR012"] == 0)
                    {
                        addressValues["ADDR012"] = 1;
                    }
                    else
                    {
                        addressValues["ADDR012"] = 0;
                    }

                    if (addressValues["ADDR013"] == 0)
                    {
                        addressValues["ADDR013"] = 1;
                    }
                    else
                    {
                        addressValues["ADDR013"] = 0;
                    }
                }
            }
        }

        public double GetAnalogValue(string address)
        {
            if (addressValues.ContainsKey(address))
            {
                return addressValues[address];
            }
            else
            {
                return -1;
            }
            
        }

        public void SetAnalogValue(string address, double value)
        {
            if (addressValues.ContainsKey(address))
            {
                addressValues[address] = value;
            }
        }

        public void SetDigitalValue(string address, double value)
        {
            if (addressValues.ContainsKey(address))
            {
                addressValues[address] = value;
            }
        }

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            Random random = new Random();
            var next = random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        public void Abort()
        {
            t1.Abort();
            t2.Abort();
        }
    }
}
