﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MobileCompaniesMAUI.Services;
using MobileCompaniesMAUI.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MobileCompaniesMAUI.Models
{
    public partial class Antenna : ObservableObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int CompanyID { get; set; }
        public Company Company { get; set; }

        // Distance (with capital D) is automatically generated by the MVVM Toolkit
        // Changes to its value will be automatically updated on bound elements
        [ObservableProperty]
        private double distance;

        [ObservableProperty]
        public int zone;

    }
}
