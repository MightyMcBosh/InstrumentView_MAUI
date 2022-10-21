


using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using LiveChartsCore.Defaults;
using System.Net;
using System.Runtime.CompilerServices;

namespace VersaMonitor;

public partial class MainPage : ContentPage
{
	int count = 0;
	ViewModel Vm;
	Popup pp;
    Timer getLRTimer;
    


    public MainPage()
	{
		InitializeComponent();
        Vm = this.BindingContext as ViewModel;
        Vm.OnConnectionChange += OnConnectionChange;
        getLRTimer = new Timer(GetLeakRatePoint, null, Timeout.Infinite, Timeout.Infinite);
        
    }

    
    public async Task TryConnect()
    {
        string result = await DisplayPromptAsync("Network Config", "Please Enter IP Address", "Connect");

        IPAddress ip = null;

        if (IPAddress.TryParse((result as string), out ip))
        {
            new Task(async () => await Vm.TryStart(ip)).Start();
        }
    }

    private void ConnectButtonClick(object sender, EventArgs e)
    {
        var args = (e as ClickedEventArgs);
        
        TryConnect(); 
    }
    private void OnConnectionChange(bool connected)
    {
        if(connected)
        {
            getLRTimer.Change(1000, 1000);
        }
        else
        {
            getLRTimer.Change(-1, -1); 
        }
    }

    private void GetLeakRatePoint(object state)
    {
       
        Vm.AddItem(LD.LeakRate);
    
    }

    private void StartStopButton_Clicked(object sender, EventArgs e)
    {
        Vm.StartStopButtonPress(); 
    }

    private void CalibrateButton_Clicked(object sender, EventArgs e)
    {
        Vm.CalibrateButtonPress(); 
    }
}

