


using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using System.Net;
using System.Runtime.CompilerServices;

namespace VersaMonitor;

public partial class MainPage : ContentPage
{
	int count = 0;
	ViewModel Vm;
	Popup pp; 
	public MainPage()
	{
		InitializeComponent();
        Vm = this.BindingContext as ViewModel; 
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
}

