using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;

namespace VersaMonitor;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("Lato-Black.ttf", "LatoBlack");
				fonts.AddFont("Lato-BlackItalic.ttf", "LatoBlackItalic");
				fonts.AddFont("Lato-Bold.ttf", "LatoBold");
				fonts.AddFont("Lato-BoldItalic.ttf", "LatoBoldItalic");
				fonts.AddFont("Lato-Light.ttf", "LatoLight");
				fonts.AddFont("Lato-LightItalic.ttf", "LatoLightItalic");
				fonts.AddFont("Lato-Thin.ttf", "LatoThin");
				fonts.AddFont("Lato-ThinItalic.ttf", "LatoThinItalic");
				fonts.AddFont("Lato-Regular.ttf", "LatoLight");
			})
			.ConfigureAnimations()
			.UseMauiCommunityToolkit()
			.UseSkiaSharp(true); 

		return builder.Build();
	}
}
