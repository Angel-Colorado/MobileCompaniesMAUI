using Microsoft.Maui.Controls;
using MobileCompaniesMAUI.Models;
using MobileCompaniesMAUI.ViewModels;
using MobileCompaniesMAUI.Services;
using System.Net.Http;
using Device = MobileCompaniesMAUI.Models.Device;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.Net;
using System.Diagnostics;

namespace MobileCompaniesMAUI.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel device;

    private readonly MobileCompaniesService mobileCompaniesService;

    public MultipartFormDataContent photoFile;

    IDispatcherTimer myTimer;
    Stopwatch stopwatch = new Stopwatch();

    public ProfilePage(ProfileViewModel profilesViewModel, MobileCompaniesService mobileCompaniesService)
    {
        InitializeComponent();
        BindingContext = profilesViewModel;
        device = profilesViewModel;
        this.mobileCompaniesService = mobileCompaniesService;

        myTimer = Dispatcher.CreateTimer();
        myTimer.Interval = TimeSpan.FromMilliseconds(3000);
        myTimer.Tick += async (s, e) =>     // Every 3 seconds will execute the following
        {
            // Add here the methods you want to trigger
            await device.TrackMeAsync();

        };

        GetDevice();

    }

    private async void GetDevice()
    {
        await device.GetCompaniesAsync();
        await device.GetDeviceExistsAsync();

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