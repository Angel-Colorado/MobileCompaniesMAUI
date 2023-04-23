using MobileCompaniesMAUI.Models;
using MobileCompaniesMAUI.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Device = MobileCompaniesMAUI.Models.Device;

namespace MobileCompaniesMAUI.Services
{
    public class MobileCompaniesService
    {
        HttpClient httpClient = new();
        List<Antenna> antennaList = new();
        List<Company> companiesList = new();
        Device deviceGet = new();

        public MobileCompaniesService()
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = Helper.DBUri;

        }

        public async Task<List<Company>> GetCompanies()
        {
            var response = await httpClient.GetAsync("api/Companies");

            if (response.IsSuccessStatusCode)
            {
                companiesList = await response.Content.ReadFromJsonAsync<List<Company>>();
            }
            else
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }

            return companiesList;
        }

        public async Task<List<Antenna>> GetAntennas()
        {
            var response = await httpClient.GetAsync("api/Antennas");
            
            if (response.IsSuccessStatusCode)
            {
                antennaList = await response.Content.ReadFromJsonAsync<List<Antenna>>();
            }
            else
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }

            return antennaList;
        }

        public async Task<List<Antenna>> GetAntennasByCompany(int CompanyID)
        {
            var response = await httpClient.GetAsync($"api/Antennas/ByCompany/{CompanyID}");

            if (response.IsSuccessStatusCode)
            {
                antennaList = await response.Content.ReadFromJsonAsync<List<Antenna>>();
            }
            else
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }

            return antennaList;
        }

        public async Task<Device> GetDeviceExists(Device device)
        {
            var response = await httpClient.GetAsync(
                    $"api/Devices/Exists?Name={device.Name}&Model={device.Model}&Manufacturer={device.Manufacturer}&Type={device.Type}");

            if (response.IsSuccessStatusCode)
            {
                deviceGet = await response.Content.ReadFromJsonAsync<Device>();
            }
            else
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }

            return deviceGet;
        }

        public async Task AddDevice(Device deviceToAdd)
        {
            var response = await httpClient.PostAsJsonAsync("/api/Devices", deviceToAdd);

            if (!response.IsSuccessStatusCode)
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }
        }

        // It'll send the Name, Model, Manufacturer, Type , Username & Photo
        public async Task CreateDevice(Device deviceToAdd, MultipartFormDataContent photo)
        {
            var response = await httpClient.PostAsync(
                    $"api/Devices?Name={deviceToAdd.Name}&Model={deviceToAdd.Model}&Manufacturer={deviceToAdd.Manufacturer}" +
                    $"&Type={deviceToAdd.Type}&Username={deviceToAdd.Username}&CompanyID={deviceToAdd.CompanyID}", photo);

            if (!response.IsSuccessStatusCode)
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }
        }

        // It'll send the ID, Username and the Photo
        public async Task UpdateDevice(Device deviceToUpdate, MultipartFormDataContent photo, bool RemoveImage)
        {
            var response = await httpClient.PutAsync(
                    $"/api/Devices?ID={deviceToUpdate.ID}&Username={deviceToUpdate.Username}&CompanyID={deviceToUpdate.CompanyID}" +
                    $"&RemoveImage={RemoveImage}", photo);

            if (!response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Error!", response.ToString(), "OK");

            }
        }

        public async Task TrackMe(Position positionToAdd)
        {
            var response = await httpClient.PostAsJsonAsync($"/api/Positions", positionToAdd);

            if (!response.IsSuccessStatusCode)
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }
        }

        public async Task UploadPhoto(MultipartFormDataContent photo)
        {
            var response = await httpClient.PostAsync($"/api/DeviceUserPhotos", photo);

            if (!response.IsSuccessStatusCode)
            {
                var ex = Helper.CreateApiException(response);
                throw ex;
            }
        }

    }
}
