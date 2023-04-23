using Microsoft.Extensions.Logging;
using MobileCompaniesMAUI.Views;
using MobileCompaniesMAUI.Services;
using MobileCompaniesMAUI.ViewModels;

namespace MobileCompaniesMAUI;

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
			});


		builder.Services.AddSingleton<MobileCompaniesService>();

        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);

        builder.Services.AddTransient<AntennaPage>();
        builder.Services.AddTransient<AntennaViewModel>();

        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Singleton will create kind of a global, whereas
        // Transient will create and destroy every time you navigate to the page

        return builder.Build();
	}
}
