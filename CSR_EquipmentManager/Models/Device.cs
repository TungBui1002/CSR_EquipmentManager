using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSR_EquipmentManager.Models
{
    public class Device
    {
        public int Id { get; set; }

        public string HoSoCode { get; set; }
        public string Factory { get; set; }
        public string Building { get; set; }
        public string Floor { get; set; }
        public string STT { get; set; }
        public string PositionCode { get; set; }
        public string DeviceCode { get; set; }
        [Required]
        public string DeviceName { get; set; }
        public string Capacity { get; set; }
        public string ModelCode { get; set; }

        public string InspectionReport { get; set; }
        public DateTime? InspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }

        public string ManufactureYear { get; set; }
        public string Location { get; set; }

        public virtual ICollection<DeviceEmail> Emails { get; set; }
    }
}