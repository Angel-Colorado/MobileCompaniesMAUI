using Microsoft.Maui.Controls;
using MobileCompaniesMAUI.Models;
using MobileCompaniesMAUI.ViewModels;
using MobileCompaniesMAUI.Services;
using System.Net.Http;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.Net;
using System.Diagnostics;

namespace MobileCompaniesMAUI.Views;

public partial class IndexPage : ContentPage
{
    public IndexPage()
    {
        InitializeComponent();

    }

    private async void BtnProfile_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));

    }

    private async void BtnAntennas_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AntennaPage));

    }

}