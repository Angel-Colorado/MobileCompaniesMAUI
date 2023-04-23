using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using MobileCompaniesMAUI.Models;
using MobileCompaniesMAUI.Services;
using MobileCompaniesMAUI.Utilities;
using MobileCompaniesMAUI.Views;
using Application = Microsoft.Maui.Controls.Application;
using Device = MobileCompaniesMAUI.Models.Device;

namespace MobileCompaniesMAUI.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        MobileCompaniesService mobileCompaniesService;

        IConnectivity connectivity;
        IGeolocation geolocation;

        public ObservableCollection<Company> Companies { get; set; } = new();

        [ObservableProperty]
        private int companiesIndex = -1;

        [ObservableProperty]
        public Device currentDevice;

        [ObservableProperty]
        private string profileStatus = "";

        [ObservableProperty]
        private bool positionFlag = false;

        [ObservableProperty]
        private bool crudFlag = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotRemoveImage))]
        private bool isRemoveImage = false;

        public bool IsNotRemoveImage => !IsRemoveImage; // This will hide locally the photo, before saving changes to the Server

        [ObservableProperty]
        public MultipartFormDataContent photoFile;

        public ProfileViewModel(MobileCompaniesService mobileCompaniesService, IConnectivity connectivity, IGeolocation geolocation)
        {
            Title = "Profile";
            this.mobileCompaniesService = mobileCompaniesService;
            this.connectivity = connectivity;
            this.geolocation = geolocation;

        }

        // Checks if the current device is already in the DB
        public async Task GetDeviceExistsAsync()
        {
            if (IsBusy)
                return;

            // Gets the current device info locally
            CurrentDevice = GetDeviceInfo();

            try
            {
                IsBusy = true;

                // Go get the current device
                var deviceG = await mobileCompaniesService.GetDeviceExists(CurrentDevice);

                if (deviceG != null)
                {
                    CurrentDevice = deviceG;    // Updates the object from the DB including Username among other properties

                    ProfileStatus = "Profile already in the DB";
                    CrudFlag = true;
                    PositionFlag = true;
                }

                // Get the Companies IDs
                int[] companiesID = Companies.Select(c => c.ID).ToArray();

                // Updates the value on the Picker
                CompaniesIndex = Array.IndexOf(companiesID, CurrentDevice.CompanyID);

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

        // Add Photo
        [RelayCommand]
        public async Task AddPhotoAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var fileResult = await MediaPicker.PickPhotoAsync();

                if (fileResult != null)
                {
                    var result = await ProcessFile(fileResult);

                    if (result)
                        await Application.Current.MainPage.DisplayAlert("Image", "Image loaded into memory\n\nClick the 'Create / Update' button to save changes to the Server", "OK");
                    else
                        await Application.Current.MainPage.DisplayAlert("Image", "Image not loaded", "OK");
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

        private async Task<bool> ProcessFile(FileResult fileResult)
        {
            if (fileResult == null)
                return false;

            using var fileStream = File.OpenRead(fileResult.FullPath);

            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                bytes = memoryStream.ToArray();
            }

            var filecontent = new ByteArrayContent(bytes);
            filecontent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            var form = new MultipartFormDataContent
            {
                { filecontent, "fileContent", Path.GetFileName(fileResult.FullPath) }
            };

            PhotoFile = form;    // References the image file to the ProfileViewModel PhotoFile

            return true;
        }

        [RelayCommand]
        private async Task UpdateCreate()
        {
            try
            {
                IsBusy = true;

                if (CurrentDevice.ID == 0)   // It means the Device is not in the DB yet
                    await mobileCompaniesService.CreateDevice(CurrentDevice, PhotoFile);                // Then Creates the Device
                else
                    await mobileCompaniesService.UpdateDevice(CurrentDevice, PhotoFile, IsRemoveImage); // Then Updates the Device

                await Application.Current.MainPage.DisplayAlert("Create / Update", "Executed", "OK");

                // Reloads the page
                var page = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                Application.Current.MainPage.Navigation.RemovePage(page);
                await Shell.Current.GoToAsync(nameof(ProfilePage), false);
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

        // This method saves the current position
        public async Task TrackMeAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                Location currentLocation;

                // Gets live location
                currentLocation = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(30)
                });

                Position currentPosition = new Position
                {
                    TimeStamp = DateTime.Now,
                    Latitude = currentLocation.Latitude,
                    Longitude = currentLocation.Longitude,
                    DeviceID = CurrentDevice.ID
                };

                await mobileCompaniesService.TrackMe(currentPosition);
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

        // Code to handle the Companies picker
        partial void OnCompaniesIndexChanged(int value)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                // Get the Companies IDs
                int[] companiesID = Companies.Select(c => c.ID).ToArray();

                // Updates the Company ID on the CurrentDevice
                CurrentDevice.CompanyID = companiesID[value];

                // Enables the Create/Update button
                CrudFlag = true;
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

        // Code to handle the changes on the 'Delete photo' checkbox
        async partial void OnIsRemoveImageChanged(bool value)
        {
            await Application.Current.MainPage.DisplayAlert("Delete photo?", "Click the 'Create / Update' button to save changes to the Server", "OK");

        }

        private Device GetDeviceInfo()
        {
            return new Device()
            {
                Name = DeviceInfo.Name,
                Model = DeviceInfo.Model,
                Manufacturer = DeviceInfo.Manufacturer,
                Type = DeviceInfo.Idiom.ToString()
            };
        }

    }
}
