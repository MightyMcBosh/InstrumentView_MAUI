


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
		Vm = new ViewModel();
		this.BindingContext = Vm;
          
	}

	

    public async Task TryConnect()
    {
        string result = await DisplayPromptAsync("Network Config", "Please Enter IP Address", "Connect");

        IPAddress ip = null;

        if (IPAddress.TryParse((result as string), out ip))
        {
            await Vm.TryStart(ip);
        }
    }

    private void ConnectButtonClick(object sender, EventArgs e)
    {
        TryConnect(); 
    }
}

