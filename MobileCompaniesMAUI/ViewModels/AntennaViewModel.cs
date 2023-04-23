using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Networking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileCompaniesMAUI.Models;
using MobileCompaniesMAUI.Services;
using MobileCompaniesMAUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using MobileCompaniesMAUI.Utilities;

namespace MobileCompaniesMAUI.ViewModels
{
    public partial class AntennaViewModel : BaseViewModel
    {
        MobileCompaniesService mobileCompaniesService;

        IConnectivity connectivity;
        IGeolocation geolocation;

        public ObservableCollection<Antenna> Antennas { get; set; } = new();
        public ObservableCollection<Company> Companies { get; set; } = new();

        bool firstRun = true;

        [ObservableProperty]
        private int companiesIndex = -1;

        public AntennaViewModel(MobileCompaniesService mobileCompaniesService, IConnectivity connectivity, IGeolocation geolocation)
        {
            Title = "Available Antennas";
            this.mobileCompaniesService = mobileCompaniesService;
            this.connectivity = connectivity;
            this.geolocation = geolocation;
        }

        public async Task GetCompaniesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var companiesC = await mobileCompaniesService.GetCompanies();

                Companies.Clear();                  // Empties the Collection

                if (companiesC.Count == 0)
                    await Application.Current.MainPage.DisplayAlert("Companies", "No Companies found", "OK");
                else
                {
                    Companies.Add(new Company { Name = "All Providers" });

                    foreach (var c in companiesC)   // Fills up the Collection again
                        Companies.Add(c);

                }
            }
            catch (ApiException apiEx)
            {
                string errMsg = "Errors:" + Environment.NewLine;
                foreach (var error in apiEx.Errors)
                {
                    errMsg += Environment.NewLine + "-" + error;
                }
                Helper.ShowMessage("Problem Accessing the Records:", errMsg);
            }
            catch (Exception ex)
            {
                if (ex.GetBaseException().Message.Contains("connection with the server"))
                {
                    Helper.ShowMessage("Error", "No connection with the server.");
                }
                else
                {
                    Helper.ShowMessage("Error", "Could not complete operation");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task GetAntennasAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var antennasC = await mobileCompaniesService.GetAntennas();

                Antennas.Clear();               // Empties the Collection

                foreach (var a in antennasC)    // Fills up the Collection again
                    Antennas.Add(a);
            }
            catch (ApiException apiEx)
            {
                string errMsg = "Errors:" + Environment.NewLine;
                foreach (var error in apiEx.Errors)
                {
                    errMsg += Environment.NewLine + "-" + error;

                    if (error.Contains("Error: No Antenna records"))
                        Antennas.Clear();       // Empties the Collection
                }
                Helper.ShowMessage("Problem Accessing the Records:", errMsg);

            }
            catch (Exception ex)
            {
                if (ex.GetBaseException().Message.Contains("connection with the server"))
                {
                    Helper.ShowMessage("Error", "No connection with the server.");
                }
                else
                {
                    Helper.ShowMessage("Error", "Could not complete operation");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Determines which antenna is the closest to the user's location
        [RelayCommand]
        public async Task GetClosestAntenna()
        {
            if (IsBusy || Antennas.Count == 0)
                return;

            try
            {
                IsBusy = true;

                var currentLocation = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(30)  // It's the time it theoretically waits to get the data before it throws the Exception
                });

                // Finds the closest antenna to us
                var first = Antennas.OrderBy(a => currentLocation.CalculateDistance(
                    new Location(a.Latitude, a.Longitude), DistanceUnits.Kilometers))
                    .FirstOrDefault();

                // Sends a message
                await Application.Current.MainPage.DisplayAlert(first.Name, $"{(currentLocation.CalculateDistance(
                    new Location(first.Latitude, first.Longitude), DistanceUnits.Kilometers) * 1000).ToString("F1")} m. away from the closest Antenna" +
                    $"\r\n\nCurrent location:\r\n Latitude: {currentLocation.Latitude.ToString("F8")}\r\n Longitude: {currentLocation.Longitude.ToString("F8")}", "OK");

            }
            catch (ApiException apiEx)
            {
                string errMsg = "Errors:" + Environment.NewLine;
                foreach (var error in apiEx.Errors)
                {
                    errMsg += Environment.NewLine + "-" + error;
                }
                Helper.ShowMessage("Problem Accessing the Records:", errMsg);
            }
            catch (Exception ex)
            {
                if (ex.GetBaseException().Message.Contains("connection with the server"))
                {
                    Helper.ShowMessage("Error", "No connection with the server.");
                }
                else
                {
                    Helper.ShowMessage("Error", "Could not complete operation");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateDistance()
        {
            if (IsBusy || Antennas.Count == 0)
                return;

            try
            {
                IsBusy = true;

                Location currentLocation;

                if (firstRun)   // Gets cached location
                {
                    currentLocation = await geolocation.GetLastKnownLocationAsync();
                    firstRun = false;

                }
                else            // Gets live location
                {
                    currentLocation = await geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Best,
                        Timeout = TimeSpan.FromSeconds(30)  // It's the time it theoretically waits to get the data before it throws the Exception
                    });
                }

                // Updates the Distance property for each Antenna
                foreach (Antenna ant in Antennas)
                {
                    ant.Distance = currentLocation
                        .CalculateDistance(new Location(ant.Latitude, ant.Longitude), DistanceUnits.Kilometers) * 1000;

                    double temp = ant.Distance;

                    if (temp >= 500)    // Values equal or above 500 are considered Level 5
                        temp = 400;

                    ant.Zone = Convert.ToInt32(Math.Truncate((temp / 100)) + 1);
                }

                // Creates the collection again, sorted by Distance
                List<Antenna> antennas = Antennas.OrderBy(a => a.Distance).ToList();

                if (Antennas.Count != 0)
                    Antennas.Clear();       // Empties the Collection

                foreach (var a in antennas) // Fills up the Collection again
                    Antennas.Add(a);

            }
            catch (ApiException apiEx)
            {
                string errMsg = "Errors:" + Environment.NewLine;
                foreach (var error in apiEx.Errors)
                {
                    errMsg += Environment.NewLine + "-" + error;
                }
                Helper.ShowMessage("Problem Accessing the Records:", errMsg);
            }
            catch (Exception ex)
            {
                if (ex.GetBaseException().Message.Contains("connection with the server"))
                {
                    Helper.ShowMessage("Error", "No connection with the server.");
                }
                else
                {
                    Helper.ShowMessage("Error", "Could not complete operation");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Code to handle the Companies picker
        async partial void OnCompaniesIndexChanged(int value)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                List<Antenna> antennasC;

                if (value == 0)     // All Providers
                {
                    antennasC = await mobileCompaniesService.GetAntennas();
                }
                else
                {
                    // Get the Companies IDs
                    int[] companiesID = Companies.Select(c => c.ID).ToArray();

                    antennasC = await mobileCompaniesService.GetAntennasByCompany(companiesID[value]);
                }

                Antennas.Clear();               // Empties the Collection

                foreach (var a in antennasC)    // Fills up the Collection again
                    Antennas.Add(a);

                IsBusy = false;
                await UpdateDistance();         // Update the Distances

            }
            catch (ApiException apiEx)
            {

                string errMsg = "Errors:" + Environment.NewLine;
                foreach (var error in apiEx.Errors)
                {
                    errMsg += Environment.NewLine + "-" + error;

                    if (error.Contains("Error: No Antenna records for that Company"))
                        Antennas.Clear();       // Empties the Collection
                }
                Helper.ShowMessage("Problem Accessing the Records:", errMsg);
            }
            catch (Exception ex)
            {
                if (ex.GetBaseException().Message.Contains("connection with the server"))
                {
                    Helper.ShowMessage("Error", "No connection with the server.");
                }
                else
                {
                    Helper.ShowMessage("Error", "Could not complete operation");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
