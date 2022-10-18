using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaMonitor; 

    ///
    /// broke out RDT into its own code file because a) it's easier to read and b) it will be easier to move this around when I get around to writing the instrument companion app. 
    /// 
    /// 


    ////Enumerators for all the different RDT statuses
    public enum StartStatus: uint
    {
        None,
        SoftwareInit,
        PrimaryPumpInit,
        RoughingPumpInit,
        SecondaryPumpInit,
        BackupFilamentInit,
        PrimaryFilamentInit,
        InitFinalizing,
    }
    public enum CalType : uint
    {
        None,
        Zero,
        Hydrogen,
        Concentration,
        HardVac,
        Characterization, 
        CalCheck,
        Sniff
    }
    public enum ZeroCalStatus : uint
    {
        Init,
        FirstSetup,
        FinalSetup,
        Temporization
    }
    public enum HydrogenCalStatus : uint
    {
        Init = 0 ,
        FirstSetup = 3,
        FinalSetup = 4,
        
    }
    public enum VacCalStatus : uint
    {
        Init, 
        ExternalLeakOpen,
        InternalLeakOpen,
        SpeedPicSearch,
        FinePicSearch,
        CloseCalLeak,
        ReadingBackground,
        Temporization,
    }
    public enum CharacterizationStats : uint
    {
        Init,
        ExternalLeakOpen,
        InternalLeakOpen,
        SpeedPicSearch,
        FinePicSearch,
        CloseCalLeak,
        WatchSignalDecrease,
        StoreVoltage,
    }

    public enum CalCheckStatus : uint
    {
        Init = 0,
        OpenInternalLeak = 2,
        ReadingSignal = 4,
        Check = 6,
        Temporization = 7,
    }
    public enum SniffCalStatus : uint
    {
        Init,
        SniffingCalibratedLeak,
        WaitSignalStable,
        FinePeakSearch,
        SniffingBg,
        WaitStableBg,
        ReadingBgSignal,
        Temporization,
    }
   

    public enum VacCycleStatus : uint
    {
        Atmosphere,
        GrossLeak,
        Normal,
        HighSensitivity,
        HighSpeed,
        SecondaryPumpSpeedChange,
        SyncPrinter,
        Standby,
        Roughing,
        Recovery,
        SyncForMemo, 
        SyncForAutotest,
    }
    public enum SniffCycleStatus : uint
    { 
        Init,
        OpenProbeValve,
        WaitForMassSpec,
        WaitSignal,
        InTest,
        Error1,
        Error2,
        Error3,
        InSmartTest,
        StopTest1,
        StopTest2,
        StopTest3,
        StopTest4,
        Recovery,
    }




    public static partial class LD
    {
        public static bool CalibrationActive => CalibrationType != CalType.None;

        

        //ST information
        public static bool Filament1Active;
        public static bool Filament2Active;
        public static bool FilamentOff; 
        public static uint InCycleStatus;
        public static bool InSniff;
        public static bool AutocalStatus;
        public static bool ControlPanelLocked;
        public static bool ErrorPresent;
        public static bool InletVentStatus;
        public static bool CycleStartAvailable;
        public static bool PumpSynchronism;
        public static bool SnifferClogged;
        


        public static StartStatus StartupStatus;
        public static CalType CalibrationType;
        public static uint CalibrationDetail;
        public static uint CycleStatusDetail;
    }

    public static partial class SerialCommands
    {
        public static void ParseRDT(string resp)
        {
            short part1, part2; 
            try
            {
                part1 = short.Parse(resp.Substring(0, 5));
                part2 = short.Parse(resp.Substring(5, 5));
            }
            catch 
            {
                return; 
            }

            int tmp = (int)part1;

            LD.StartupStatus = (StartStatus)(tmp & 7);
            tmp >>= 3;
            LD.CalibrationType = (CalType)(tmp & 7);
            tmp >>= 3;
            LD.CalibrationDetail = (uint)(tmp & 7);
            tmp >>= 3;
            LD.CycleStatusDetail = (uint)(tmp & 15);
                        
            

            //parse the second part of the RDT message
            ParseST(part2); 
        }

        public static void ParseST(short value)
        {
            bool tmp1, tmp2;
            int j = value; 
            tmp1 = (j & 1) != 0;
            j >>= 1; 
            tmp2 = (j & 1) != 0;
            j >>= 1; 
            LD.Filament1Active = !tmp1 && tmp2;
            LD.Filament2Active = tmp1 && tmp2;
            LD.FilamentOff = !tmp2; 

            LD.InCycle = (j & 1) != 0;
            j >>= 1;
            LD.InCycleStatus = (uint)(j & 3);
            j >>= 2;
            LD.InSniff = (j & 1) != 0;
            j >>= 1; 
            LD.AutocalStatus = (j & 1) != 0;
            j >>= 1; 
            LD.ControlPanelLocked = (j & 1) != 0;
            j >>= 1; 
            LD.ErrorPresent = (j & 1) != 0;
            j >>= 1; 
            LD.InletVentStatus = (j & 1) != 0; 
            j >>= 1; 
            LD.CycleStartAvailable = (j & 1) != 0;
            j >>= 1;
            LD.PumpSynchronism = (j & 1) != 0;
            j >>= 3;
            LD.SnifferClogged = (j & 1) != 0;


            if (!LD.InCycle)
                LD.TestMode = Mode.Standby;
            else
            {
                if(LD.InSniff)
                {
                    LD.TestMode = Mode.Sniff; 
                }
                else
                {
                    if(LD.InCycleStatus < 2)
                    LD.TestMode = (Mode)(LD.InCycleStatus + 1); 
                    else
                    {
                        LD.TestMode = (Mode)(LD.InCycleStatus + 2); 
                    }
                }
            }
        }
    }

