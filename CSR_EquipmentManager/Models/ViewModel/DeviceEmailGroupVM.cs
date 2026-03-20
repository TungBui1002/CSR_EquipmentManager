using System.Collections.Generic;

namespace CSR_EquipmentManager.Models.ViewModel
{
    public class DeviceEmailGroupVM
    {
        public Device Device { get; set; }
        public List<DeviceEmail> Emails { get; set; }
    }
}