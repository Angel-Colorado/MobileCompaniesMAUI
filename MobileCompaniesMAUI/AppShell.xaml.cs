using MobileCompaniesMAUI.Views;

namespace MobileCompaniesMAUI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(IndexPage), typeof(IndexPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(AntennaPage), typeof(AntennaPage));
    }
}
