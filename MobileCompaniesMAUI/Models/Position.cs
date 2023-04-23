using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileCompaniesMAUI.Models
{
    public partial class Position : ObservableObject
    {
        public int ID { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int DeviceID { get; set; }
        public Device Device { get; set; }

    }
}
