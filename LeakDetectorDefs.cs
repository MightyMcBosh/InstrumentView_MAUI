using System;
using System.Collections.Generic;
using System.Collections; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace VersaMonitor
{
    public class AsciiCommand
    {
        public AsciiCommand(string cmd, string p, int responseLength, int parseLoc)
        {
            this.cmd = cmd;
            this.parameters = p;
            this.ackloc = responseLength;
            this.ParseOpt = parseLoc;
        }

        public string cmd;
        public string parameters;
        public int ackloc;
        public int ParseOpt;
    }
    public enum TestMethod
    {
        Vac,
        Sniff
    }
    
    public enum Units
    {
        mbar,
        Pa,
        Torr,
        atm,
        ppm,
        sccm,
        sccs
    }
    public enum Mode
    {
        Standby,
        Rough,
        Gross,
        Fine,
        Ultra,
        Sniff
    }
    public enum DetectorState
    {
        Standby,
        Rough,
        Gross,
        Fine,
        Ultra,
        Sniff,
        Calibration
    }

    public enum DetectorStatusCode
        {
        Startup, 
        InCal, 
        HardVac,
        Sniff
        }

    public enum DetectorProperty
    {
        LeakRate,
        Reject,
        Pressure,
        State,
        LRUnits
    }

    public class OnStatusChangeHandlerArgs
    {
        public DetectorProperty property;
        public object value;

        public OnStatusChangeHandlerArgs(DetectorProperty property, object value)
        {
            this.property = property;
            this.value = value;
        }
    }

    public delegate void OnConnectionChangeHandler(bool connected);
    public delegate void OnStatusChangeHandler(OnStatusChangeHandlerArgs args);

    /// <summary>
    /// this will function as the logical black box for the Titan Versa. It handles the socket connection internally and retains current information and passes it up the chain. 
    /// </summary>
    public partial class LD
    {      

        public static event OnConnectionChangeHandler ConnectionChanged;
        public static event OnStatusChangeHandler StatusChanged;

        //internal values, need to use Setters to fire events
        private static double _leakrate, _reject, _pressure;
        private static DetectorState _detectorState;
        private static string _lrunits = ""; 


        private static bool _connected;


        public static bool Connected
        {
            get => _connected;
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    ConnectionChanged?.Invoke(_connected);
                }
            }
        }

        public static string LRUnits
        {
            get => _lrunits;
            set
            {
                if (_lrunits != value)
                {
                    _lrunits = value;
                    StatusChanged?.Invoke(new OnStatusChangeHandlerArgs(DetectorProperty.LRUnits, value));
                }
            }
        }

        public static double LeakRate
        {
            get => _leakrate;
            internal set
            {
                if (value != _leakrate)
                {
                    _leakrate = value;
                    StatusChanged?.Invoke(new OnStatusChangeHandlerArgs(DetectorProperty.LeakRate, value));
                }
            }
        }

        public static double InletPressure
        {
            get => _pressure;
            internal set
            {
                if (value != _reject)
                {
                    _reject = value;
                    StatusChanged?.Invoke(new OnStatusChangeHandlerArgs(DetectorProperty.Reject, value));
                }
            }
        }

        public static DetectorState State
        {
            get => _detectorState;
            internal set
            {
                if (value != _detectorState)
                {
                    _detectorState = value;
                    StatusChanged?.Invoke(new OnStatusChangeHandlerArgs(DetectorProperty.State, value));
                }
            }
        }


        public static double RejectLimit
        {
            get => _reject;
            internal set
            {
                if (value != _reject)
                {
                    _reject = value;
                    StatusChanged?.Invoke(new OnStatusChangeHandlerArgs(DetectorProperty.Reject, value));
                }
            }
        }

        public static bool isCorrected = false;
        public static List<string> ErrorList = new List<string>();
        //?ST Data
        public static bool InCycle;
        public static Mode TestMode;
        public static TestMethod TestMethod; 
        public static Units Unit = Units.mbar;
        public static bool RejectCrossed = false;
        
        public static bool ZeroOn = false;
        public static bool CalInProgress;
        public static bool VRough, VGross, VFine, VUltra4, VUltra5, VVent, VCal, V8, V9, VSniff, VPurge; //Valves on the manifold

        public static string CFMatchString = @"\d{3}[-+]\d{2}";
        public static string[] modeString = { "Standby", "Rough", "Gross", "Fine", "Ultra", "Sniff" , "Startup", "Shutdown", "Error"};
        public static string[] unitStrings = { "mTor.l/s","mbar.l/s" ,"pa.m3/h" ,"torr.l/s" ,"atm.cc/s" ,"ppm" ,"sccm" ,"sccs" };
    }






    public static partial class SerialCommands
    {
        //public static Command StartCycle = new Command { cmd = "=CYE", parameters = "", ackloc = 0,ParseOpt = 0 };
        //public static Command StopCycle = new Command { cmd = "=CYD", parameters = "", ackloc = 0, ParseOpt = 0 }; 
        //public static Command GetStatus = new Command { cmd = "?HMI", parameters = "", ackloc = 28, ParseOpt = 1 };
        //public static Command GetCycleState = new Command { cmd = "?VA", parameters = "", ackloc = 6, ParseOpt = 2 };
        //public static Command GetRDT = new Command { cmd = "?RDT", parameters = "", ackloc = 11, ParseOpt = 3 };
        //public static Command GetLeakRate = new Command { cmd = "?LE", parameters = "", ackloc = 8, ParseOpt = 4 };
        //public static Command GetZeroStats = new Command { cmd = "?ER", parameters = "", ackloc = 255, ParseOpt = 5 }; 

        public static AsciiCommand StartCycle = new AsciiCommand("=CYE", "", 0, 0);
        public static AsciiCommand StopCycle = new AsciiCommand("=CYD", "", 0, 0);
        public static AsciiCommand StartAutoCal = new AsciiCommand("!AC", "", 0, 0);
        public static AsciiCommand GetStatus = new AsciiCommand("?HMI", "", 29, 1);
        public static AsciiCommand GetCycleState = new AsciiCommand("?VA", "", 6, 2);
        public static AsciiCommand GetRDT= new AsciiCommand("?RDT", "", 11, 3);
        public static AsciiCommand GetLeakRate = new AsciiCommand("?LE", "", 8, 4);
        public static AsciiCommand GetForelinePressure = new AsciiCommand("?PE", "", 7, 5);
        public static AsciiCommand GetErrorState = new AsciiCommand("?ER", "",255, 6);
        public static AsciiCommand GetZeroStatus = new AsciiCommand("?AZ", "", 2, 7); 
        public static AsciiCommand StartZero = new AsciiCommand("=AZE", "", 0, 0);
        public static AsciiCommand StopZero = new AsciiCommand("=AZD", "", 0, 0);
        public static AsciiCommand GetUnits = new AsciiCommand("?UN", "", 2, 8);
        public static AsciiCommand GetSniffLeakReject = new AsciiCommand("?S1S", "", 7, 9);
        public static AsciiCommand GetVacLeakReject = new AsciiCommand("?S1H", "", 7, 10);
        public static AsciiCommand GetTestMethod = new AsciiCommand("?TST", "", 2, 11);




        public static AsciiCommand[] cycle = { //Copied straight from the Atlas cycle. going to use for troubleshooting. 
        GetRDT,
        GetUnits,
        GetLeakRate,
        GetStatus,
        GetForelinePressure,
        GetCycleState,
        GetLeakRate,
        GetStatus,
        GetRDT,
         GetCycleState,
        GetLeakRate,
        GetForelinePressure,
        GetLeakRate,
        GetStatus,
        GetZeroStatus,
        GetErrorState,
         GetCycleState,
         GetSniffLeakReject,
         GetVacLeakReject,
         GetTestMethod,
        };


        public static void Parse(string resp, int opt)
        {
            

            switch (opt)
            {
                case 0:
                    break;
                case 1:

                    string lr = resp.Substring(0, 6);

                    string pres = resp.Substring(13, 6);

                    LD.LeakRate = GetCF(lr);

                    LD.InletPressure = GetCF(pres);
                    break;

                case 2:
                  
                    ParseValveStatus(resp.Substring(0,5)); 
                    break;
                case 3: //rdt 
                    

                    if (resp.Length > 10)
                    {
                        resp = resp.Substring(0, 10); //if there is an ack and or line break, just pull the first ten 10 characters
                    }

                    if (System.Text.RegularExpressions.Regex.IsMatch(resp, @"\d{10}"))  
                    {
                        ParseRDT(resp); 
                    }
                 
                    
                    break;

                case 4: // ?LE: Leak rate. REsponse 
                    string lr2 = resp.Substring(0, 6);
                    LD.LeakRate = GetCF(lr2); 
                    LD.isCorrected = resp.Substring(6, 1) == "C";                    

                    break;

                case 5: //?PE get foreline pressure
                    LD.InletPressure = GetCF(resp.Substring(0, 6)); 
                    break;

                case 6: //?ER error stuff
                    int NumErrors = 0;
                    int.TryParse(resp.Substring(0, 1), out NumErrors);

                    if (NumErrors > 0)
                    {
                        string errorString = resp.Substring(1, resp.Length - 2); // grab all the error codes in one chunk minus the \r and 0x06 on the end

                        if (errorString.Length / 4 != LD.ErrorList.Count)
                        {
                            LD.ErrorList.Clear();

                            while (errorString.Length > 4)
                            {
                                LD.ErrorList.Add(errorString.Substring(0, 4)); //populate the error list with the error codes, to be manipulated at a later date. I don't need them now but it may be useful when we flesh this program out.  
                                errorString = errorString.Substring(4, errorString.Length - 4);
                            }
                        } 
                    }                    
                    break;
                case 7: //zero Status
                    LD.ZeroOn = resp.Substring(0, 1) == "E"; 
                    break;
                    
                case 8: //?UN
                    LD.LRUnits = LD.unitStrings[int.Parse(resp.Substring(0, 1))];                    
                    break;

                case 9: //?S1S
                    double rej1 = GetCF(resp.Substring(0, 6));
                    if (LD.TestMethod == TestMethod.Vac)
                        LD.RejectLimit = rej1; 
                    break;
                    
                case 10: //?S1H
                    double rej2 = GetCF(resp.Substring(0, 6));
                    if (LD.TestMethod == TestMethod.Sniff)
                        LD.RejectLimit = rej2; 
                        break;
                case 11:
                    if (resp[0] == '2')
                        LD.TestMethod = TestMethod.Sniff;
                    else
                        LD.TestMethod = TestMethod.Vac; 
                    break; 
            }

        }
        
        public static double GetCF(string cf)
        {
            if (cf == null || cf.Length != 6)
                return 0;

            double mant = 0;
            double exp = 0; 
            double.TryParse(cf.Substring(0, 3), out mant);
            double.TryParse(cf.Substring(3, 3), out exp);

            return (mant * Math.Pow(10, exp));               
           
        }
        private static void ParseValveStatus(string value)
        {
            DetectorState tmpState = DetectorState.Standby; 
            try
            {

                
                int N = 0;
                int j = int.Parse(value);       // get the integer status value
                string ss = "";
                LD.VRough = ((j & (1 << N++)) != 0);            // V1
                                                                // Stat.SetStatusValue(Vals.V_Rough, ss);
                LD.VGross = ((j & (1 << N++)) != 0);         // V2
                                                             // Stat.SetStatusValue(Vals.V_Gross, ss);
                LD.VFine = ((j & (1 << N++)) != 0);            // V3
               // Stat.SetStatusValue(Vals.V_Fine, ss);
                LD.VUltra4 = ((j & (1 << N++)) != 0);          // V4
               // Stat.SetStatusValue(Vals.V_Ultra4, ss);
                LD.VUltra5 = ((j & (1 << N++)) != 0);         // V5
                                                              // Stat.SetStatusValue(Vals.V_Ultra5, ss);
                LD.VVent = ((j & (1 << N++)) != 0);    // V6
                                                       // Stat.SetStatusValue(Vals.V_Vent, ss);
                LD.VCal = ((j & (1 << N++)) != 0);           // V7
                                                             // Stat.SetStatusValue(Vals.V_Calibration, ss);
                LD.V8 = ((j & (1 << N++)) != 0);           // V8

                LD.V9 = ((j & (1 << N++)) != 0);// V9

                LD.VSniff = ((j & (1 << N++)) != 0);         // V10
                                                             // Stat.SetStatusValue(Vals.V_Sniff, ss);
                LD.VPurge = ((j & (1 << N++)) != 0);          // V11
                //Stat.SetStatusValue(Vals.V_Purge, ss);
            }
            catch
            { return; }

            //Debug.WriteLine("R" + Stat.GetStatusValue(Stat.Vals.V_Rough));
            //Debug.WriteLine("G" + Stat.GetStatusValue(Stat.Vals.V_Gross));
            //Debug.WriteLine("F" + Stat.GetStatusValue(Stat.Vals.V_Fine));
            //Debug.WriteLine("4" + Stat.GetStatusValue(Stat.Vals.V_Ultra4));
            //Debug.WriteLine("5" + Stat.GetStatusValue(Stat.Vals.V_Ultra5));
            //Debug.WriteLine("V" + Stat.GetStatusValue(Stat.Vals.V_Vent));
            // determine cycle state:
            //////////////////////////////////////////////////////// STANDBY:
            if (!LD.VRough &&
                LD.VGross &&
                !LD.VFine &&
              !LD.VUltra4 && 
                !LD.VUltra5 &&
                !LD.VSniff
                )
            {
                LD.TestMode = Mode.Standby;
                tmpState = DetectorState.Standby; 
            }

            //////////////////////////////////////////////////////// ROUGHING:
            //else if (Stat.GetStatusValue(Vals.V_Rough) == "1"
            //    && Stat.GetStatusValue(Vals.V_Gross) == "0"
            //    && Stat.GetStatusValue(Vals.V_Fine) == "0"
            //    && Stat.GetStatusValue(Vals.V_Ultra4) == "0"
            //    && Stat.GetStatusValue(Vals.V_Ultra5) == "0"
            //    && Stat.GetStatusValue(Vals.V_Vent) == "0"
            //    )
            //{
            //    Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("rgh00001")));
            //    //                Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("roughing99")).ToUpper());
            //    ModuleStatus.CycleState = TestMode.Rough;
            //}
            else if (LD.VRough &&
               !LD.VGross &&
               !LD.VFine &&
             !LD.VUltra4 &&
               !LD.VUltra5 &&
               !LD.VSniff
               )
            {
                LD.TestMode = Mode.Rough;
                tmpState = DetectorState.Rough;
            }

            //////////////////////////////////////////////////////// GROSS LEAK:
            //else if (Stat.GetStatusValue(Vals.V_Rough) == "1"
            //   && Stat.GetStatusValue(Vals.V_Gross) == "1"
            //   && Stat.GetStatusValue(Vals.V_Fine) == "0"
            //   && Stat.GetStatusValue(Vals.V_Ultra4) == "0"
            //   && Stat.GetStatusValue(Vals.V_Ultra5) == "0"
            //   && Stat.GetStatusValue(Vals.V_Vent) == "0"
            //   )
            //{
            //    Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("grs00001")));
            //    //                Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("gross99")).ToUpper());
            //    ModuleStatus.CycleState = TestMode.Gross;
            //}
            if (LD.VRough &&
               LD.VGross &&
               !LD.VFine &&
             !LD.VUltra4 &&
               !LD.VUltra5 &&
               !LD.VSniff
               )
            {
                LD.TestMode = Mode.Gross;
                tmpState = DetectorState.Gross; 
            }

            ///////////////////////////////////////////////////////// FINE
            //else if (Stat.GetStatusValue(Vals.V_Rough) == "0"
            //   && Stat.GetStatusValue(Vals.V_Gross) == "1"
            //   && Stat.GetStatusValue(Vals.V_Fine) == "1"
            //   && Stat.GetStatusValue(Vals.V_Ultra4) == "0"
            //   && Stat.GetStatusValue(Vals.V_Ultra5) == "0"
            //   && Stat.GetStatusValue(Vals.V_Vent) == "0"
            //   )
            //{
            //    Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("fin00001")));
            //    //                Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("fine99")).ToUpper());
            //    ModuleStatus.CycleState = TestMode.Fine;
            //}
            if (!LD.VRough &&
               LD.VGross &&
               LD.VFine &&
             !LD.VUltra4 &&
               !LD.VUltra5 &&
               !LD.VSniff
               )
            {
                LD.TestMode = Mode.Fine;
                tmpState = DetectorState.Fine; 
            }

            //////////////////////////////////////////////////////////// ULTRA:
            //else if (Stat.GetStatusValue(Vals.V_Rough) == "0"
            //   && Stat.GetStatusValue(Vals.V_Gross) == "1"
            //   && Stat.GetStatusValue(Vals.V_Fine) == "0"
            //   && Stat.GetStatusValue(Vals.V_Ultra4) == "1"
            //   && Stat.GetStatusValue(Vals.V_Ultra5) == "1"
            //   && Stat.GetStatusValue(Vals.V_Vent) == "0"
            //   )
            //{
            //    Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("ult00001")));
            //    //                Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("ultra99")).ToUpper());
            //    ModuleStatus.CycleState = TestMode.Ultra;
            //}
            if (!LD.VRough &&
               LD.VGross &&
               !LD.VFine &&
             LD.VUltra4 &&
               LD.VUltra5 &&
               !LD.VSniff
               )
            {
                LD.TestMode = Mode.Ultra;
                tmpState = DetectorState.Ultra; 
            }

            ////////////////////////////////////////////////////////////// Sniff:
            //else if (Stat.GetStatusValue(Vals.V_Rough) == "0"
            //   && Stat.GetStatusValue(Vals.V_Gross) == "1"
            //   && Stat.GetStatusValue(Vals.V_Fine) == "0"
            //   && Stat.GetStatusValue(Vals.V_Ultra4) == "0"
            //   && Stat.GetStatusValue(Vals.V_Ultra5) == "0"
            //   && Stat.GetStatusValue(Vals.V_Sniff) == "1"
            //   )
            //{
            //    Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("snf00001")));
            //    //                Stat.SetStatusValue(Vals.CycleStatebyValves, (StringLibrary.RetString("sniffing99")).ToUpper());
            //    ModuleStatus.CycleState = TestMode.Sniff;
            //}
            if (!LD.VRough &&
               LD.VGross &&
               !LD.VFine &&
             !LD.VUltra4 &&
               !LD.VUltra5 &&
               LD.VSniff
               )
            {
                LD.TestMode = Mode.Sniff;
                tmpState = DetectorState.Sniff; 
            }
            //////////////////////////////////////////////////////// ROUGHING:



            if (LD.CalibrationActive)
                tmpState = DetectorState.Calibration;

            LD.State = tmpState; 
        }
    }
}
