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
        
        public string LeakRateMantissa { get; internal set; }
        public string LeakRateExponents { get; internal set; }
        public string LeakRateUnits { get; internal set; }

        public bool Passing { get; internal set; }
        public bool InCycle { get; internal set; }
        public bool Connected { get; internal set; }


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
                    break; 
            }
        }

        private void OnLDConnectionChanged(bool connected)
        {
            //If Connected, make the dropdown go away (map visibility to the LD connected Variable) and vice versa if it disconnects
            //
            this.Connected = connected;
        }

        public async void TryStart(IPAddress add)
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
    }
}
