


using LiveChartsCore.Defaults;
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

        public event OnConnectionChangeHandler OnConnectionChange; 
        private double _lr, _reject;
        private string _cyclestate = "Standby"; 
        public string LeakRateMantissa { get; internal set; }
        public string LeakRateExponents { get; internal set; }
        public string LeakRateUnits { get; internal set; }
        public string CurrentCycleState { get => _cyclestate;
            internal set {
                if (_cyclestate != value)
                {
                    _cyclestate = value;
                    OnPropertyChanged(nameof(CurrentCycleState)); 
                }
            } 
        }
        public string StartStopButtonText { get; internal set; } = "Start";

        public Color StartStopButtonColor { get; internal set; } = Colors.ForestGreen; 
        public bool SSButtonEnabled { get; internal set; } = true;

        public bool _passing = false; 
        public bool Passing { 
            get => _passing; 
            internal set
            {
                if (_passing != value)
                {
                    _passing = value;
                    OnPropertyChanged(nameof(Passing));

                    CurrentBackground = (LD.State == DetectorState.Standby) ? standby : this.Passing ? pass : fail;
        
                     
                }
            
            }

        } 
        public bool InCycle { get; internal set; } = false; 
        public bool Connected { get; internal set; } = false;
        public bool Disconnected { get; internal set; } = true;


        public bool InUse = false; 
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
                    this.OnPropertyChanged(nameof(LeakRateMantissa)); 
                    this.OnPropertyChanged(nameof(LeakRateExponents));
                    break;

                case DetectorProperty.Pressure:
                    
                    break;


                case DetectorProperty.Reject:
                    _reject = (double)args.value;
                    Passing =  LD.LeakRate < _reject;

                    break;

                case DetectorProperty.LRUnits:
                    LeakRateUnits = args.value as string;
                    this.OnPropertyChanged(nameof(LeakRateUnits)); 
                    break;
                case DetectorProperty.State:
                    CurrentCycleState = LD.modeString[((int)args.value)]; 
                    if(LD.State == DetectorState.Standby)
                    {
                        StartStopButtonText = "Start";
                        StartStopButtonColor = Colors.ForestGreen;
                        SSButtonEnabled = true;
                        CurrentBackground = standby; 
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


                    if(LD.State != DetectorState.Standby)
                    {
                        CurrentBackground = Passing ? pass : fail; 
                    }

                    this.OnPropertyChanged(nameof(StartStopButtonText));
                    this.OnPropertyChanged(nameof(StartStopButtonColor));
                    this.OnPropertyChanged(nameof(SSButtonEnabled));
                    break;
            }
        }

        private void OnLDConnectionChanged(bool connected)
        {
            //If Connected, make the dropdown go away (map visibility to the LD connected Variable) and vice versa if it disconnects
            //
            this.Connected = connected;
            this.Disconnected = !connected;

            this.OnPropertyChanged(nameof(Connected));
            this.OnPropertyChanged(nameof(Disconnected));

            OnConnectionChange?.Invoke(connected); 
        }

        public async Task TryStart(IPAddress add)
        {
            if (!InUse)
            {
                InUse = true; 
                var t =  controller.Connect(add);
                await t;

                if (t.GetAwaiter().GetResult())
                {
                    await controller.RunEthernet();
                    //disconnected
                    LD.Connected = false;
                    InUse = false;  
                }
            }
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
            string tmp = string.Format("{0:#.##E+00}", tmpIn);
            try
            {
                

                int e_index = tmp.IndexOf('E');



                if (e_index >= 0)
                {   string r1 = tmp.Substring(0, e_index);
                    e_index += 1; 
                    string r2 = tmp.Substring(e_index, tmp.Length - e_index);
                    return (r1, r2);
                }

                Console.WriteLine("Failed to convert leak rate: " + tmp);
                return ("1.00", "-12");

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to convert leak rate: " + tmp);
                return ("1.00", "-12");
                
            }
            // ..: negatives

           
        }
    }
}
