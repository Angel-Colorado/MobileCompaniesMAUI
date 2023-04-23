using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileCompaniesMAUI.Models
{
    public partial class DeviceUserPhoto : ObservableObject
    {
        public int ID { get; set; }

        // Summary Property to recreate the Photo from the Content
        public ImageSource Photo
        {
            get
            {
                string imageBase64 = Convert.ToBase64String(Content);
                MemoryStream stream = new MemoryStream(Convert.FromBase64String(imageBase64));
                
                return ImageSource.FromStream(() => stream);
            }
        }

        public byte[] Content { get; set; }
        public string MimeType { get; set; }
        public int DeviceID { get; set; }
        public Device Device { get; set; }
    }
}
