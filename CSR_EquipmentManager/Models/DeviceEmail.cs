namespace CSR_EquipmentManager.Models
{
    public class DeviceEmail
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public string Email { get; set; }

        public virtual Device Device { get; set; }
    }
}