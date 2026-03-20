namespace CSR_EquipmentManager.Models.ViewModel
{
    public class DashboardVM
    {
        public int TotalDevices { get; set; }
        public int ExpiredDevices { get; set; }
        public int ExpiringSoonDevices { get; set; }
        public int ValidDevices { get; set; }

        // Dùng cho biểu đồ
        public string StatusLabels => "[\"Quá hạn\", \"Sắp hết hạn\", \"Hoạt động tốt\"]";
        public string StatusData => $"[{ExpiredDevices}, {ExpiringSoonDevices}, {ValidDevices}]";
        public string StatusColors => "[\"#dc3545\", \"#ffc107\", \"#198754\"]"; // đỏ, vàng, xanh
    }
}