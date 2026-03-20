using System.ComponentModel.DataAnnotations;

namespace CSR_EquipmentManager.Models
{
    public class DeviceEmail
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        [Required]
        public string Email { get; set; }

        public virtual Device Device { get; set; }
    }
}