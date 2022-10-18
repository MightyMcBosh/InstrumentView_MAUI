using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VersaMonitor
{
    /// <summary>
    /// this file handles the hooking of the LD model to the UI views. 
    /// </summary>
    public partial class ViewModel
    {
        private double _lr, _reject; 
        public string LeakRateMantissa { get; internal set; }
        public string LeakRateExponents { get; internal set; }
        public string LeakRateUnits { get; internal set; }
        public string CurrentCycleState { get; internal set; } = "Standby";
        public string StartStopButtonText { get; internal set; } = "Start";

        public Color StartStopButtonColor { get; internal set; } = Colors.ForestGreen; 
        public bool SSButtonEnabled { get; internal set; } = true;


        public bool Passing { get; internal set; } = false; 
        public bool InCycle { get; internal set; } = false; 
        public bool Connected { get; internal set; } = false;
        public bool Disconnected { get; internal set; } = true; 
       


        private CommunicationController controller; 
        

        public void InitializeLDMap()
        {
            LD.ConnectionChanged += OnLDConnectionChanged;
            LD.StatusChanged += LD_StatusChanged;
            controller = new CommunicationController(); 
        }

        private void LD_StatusChanged(OnStatusChangeHandlerArgs args)
        {
            switch(args.property)
            {
                case DetectorProperty.LeakRate:
                    double num = (double)args.value;
                    var t  = GetLeakRateMantissaAndExponent(num);
                    LeakRateMantissa = t.mant;
                    LeakRateExponents = t.exp;
                    Passing = num < _reject; 
                    break;

                case DetectorProperty.Pressure:
                    
                    break;


                case DetectorProperty.Reject:
                    num = (double)args.value;
                    Passing = num < _reject; 
                    break;

                case DetectorProperty.State:
                    CurrentCycleState = LD.modeString[((int)args.value)]; 
                    if(LD.State == DetectorState.Standby)
                    {
                        StartStopButtonText = "Start";
                        StartStopButtonColor = Colors.ForestGreen;
                        SSButtonEnabled = true;

                    }
                    else if(LD.CalibrationActive)
                    {
                        StartStopButtonText = "Calibrating";
                        StartStopButtonColor = Colors.DarkGoldenrod;
                        SSButtonEnabled = false;
                    }
                    else
                    {
                        StartStopButtonText = "Stop";
                        StartStopButtonColor = Colors.DarkRed;
                        SSButtonEnabled = true;
                    }
                    break;
            }
        }

        private void OnLDConnectionChanged(bool connected)
        {
            //If Connected, make the dropdown go away (map visibility to the LD connected Variable) and vice versa if it disconnects
            //
            this.Connected = connected;
            this.Disconnected = !connected; 
        }

        public async Task TryStart(IPAddress add)
        {
            await controller.Connect(add); 
        }
        public void StartStopButtonPress()
        {
            if (Connected)
                controller.ToggleLD(null); 
        }

        public void CalibrateButtonPress()
        {
            if (Connected)
                controller.StartCal(null); 
        }

        private (string mant, string exp) GetLeakRateMantissaAndExponent(double value)
        {
            
            double tmpIn = value;
            string r1 = "", r2 = ""; 
            int baseExp;
            // convert mantissa/exp first:
            if (value == 0.0)
            {
                
            }
            else if (value > 0.0 & value < 1.0)      // positive, < 1
            {
                baseExp = 0;
                while (tmpIn < 0.999) //two digits of precision, have issues with this parsing to like 0.9999999999999976, which is > .999 but smaller than 1
                {
                    tmpIn *= 10;
                    --baseExp;
                }
                for (int I = 0; I < 2; I++)     // add two -> implied decimal
                {
                    tmpIn *= 10;
                    --baseExp;
                }
                r1 = tmpIn.ToString("D");
                r2 =  baseExp.ToString("d2");
            }            
            // ..: negatives

           return (r1,r2);
        }
    }
}
