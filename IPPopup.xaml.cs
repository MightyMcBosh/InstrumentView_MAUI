using CommunityToolkit.Maui.Views;

namespace VersaMonitor;

public partial class IPPopup : Popup
{
	public EventHandler OnButtonClicked;

	public IPPopup(string ip = null) 
	{
		InitializeComponent();
		if (ip != null)
			IPEntry.Text = ip; 
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		Close(IPEntry.Text); 
	}
}