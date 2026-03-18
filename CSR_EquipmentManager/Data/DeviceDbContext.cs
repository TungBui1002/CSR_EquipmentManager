using CSR_EquipmentManager.Models;
using System.Data.Entity;

namespace CSR_EquipmentManager.Data
{
    public class DeviceDbContext : DbContext
    {
        public DeviceDbContext() : base("name=DeviceDbContext")
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceEmail> DeviceEmails { get; set; }
    }
}