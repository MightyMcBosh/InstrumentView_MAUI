


using CommunityToolkit.Maui.Views;
using Java.Security;
using System.Net;
using System.Runtime.CompilerServices;

namespace VersaMonitor;

public partial class MainPage : ContentPage
{
	int count = 0;
	ViewModel Vm; 
	public MainPage()
	{
		InitializeComponent();
		LD.ConnectionChanged += LD_ConnectionChanged;		
		Vm = new ViewModel();
		this.BindingContext = Vm; 
		
		this.Appearing += MainPage_Appearing;
	}

	private void MainPage_Appearing(object sender, EventArgs e)
	{
        new Task(TryConnect).Start();
    }

	private void LD_ConnectionChanged(bool connected)
	{
		
	}

	private async void TryConnect()
	{

		while (!Vm.Connected)
        {
            await GetIPAddress();
        }
    }

    public async Task GetIPAddress()
    {
        bool cont = false;
        IPAddress ip = null;

        while (!cont)
        {
            string entry = CommunicationController.IP == null ? "0.0.0.0" : CommunicationController.IP.ToString();

            var popup = new IPPopup(entry);

            var result = await this.ShowPopupAsync(popup);


            if (IPAddress.TryParse((result as string), out ip))
                cont = true;

        }


        await Vm.TryStart(ip); 
		
	}
	
}

