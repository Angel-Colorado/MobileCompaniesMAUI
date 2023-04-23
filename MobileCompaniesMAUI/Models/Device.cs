using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MobileCompaniesMAUI.Models
{
    public partial class Device : ObservableObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Type { get; set; }
        public string Username { get; set; }
        public DeviceUserPhoto DeviceUserPhoto { get; set; }
        public int CompanyID { get; set; }
        public Company Company { get; set; }
        public ICollection<Position> Positions { get; set; }
    }
}
