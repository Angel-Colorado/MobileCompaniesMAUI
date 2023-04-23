using Microsoft.Maui.Controls.PlatformConfiguration;
using MobileCompaniesMAUI.Models;
using MobileCompaniesMAUI.ViewModels;
using System.Diagnostics;
using System.Text;

namespace MobileCompaniesMAUI.Views;

public partial class AntennaPage : ContentPage
{
	private readonly AntennaViewModel antennas;

    IDispatcherTimer myTimer;
    Stopwatch stopwatch = new Stopwatch();

    public AntennaPage(AntennaViewModel antennasViewModel)
	{
		InitializeComponent();

		BindingContext = antennasViewModel;
        antennas = antennasViewModel;

        GetDisplayAntennas();

        myTimer = Dispatcher.CreateTimer();
        myTimer.Interval = TimeSpan.FromMilliseconds(3000);
        myTimer.Tick += async (s, e) =>     // Every 3 seconds will execute the following
        {
            await antennas.UpdateDistance();

        };

    }

    private async void GetDisplayAntennas()
	{
        await antennas.GetAntennasAsync();  // Gets and Displays the Antennas
        await antennas.UpdateDistance();    // Calculates the Distance Properties
        await antennas.GetCompaniesAsync();

    }

    // Event to handle the toggle of the switch
    void OnToggled(object sender, ToggledEventArgs e)
    {
        // Depending on the state of the swith it'll start or stop the timer
        if (swRefresh.IsToggled)
        {
            stopwatch.Start();
            myTimer.Start();
        }
        else
        {
            stopwatch.Stop();
            myTimer.Stop();
        }
    }


}

