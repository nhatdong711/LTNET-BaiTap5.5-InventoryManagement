namespace InventoryManagement.DTO
{
    /// <summary>Một dòng chi tiết phiếu kho (bảng ChiTietPhieuKho).</summary>
    public class ChiTietPhieuKhoDTO
    {
        public string MaVatTu { get; set; }
        public string TenVatTu { get; set; }
        public string DVT { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get { return SoLuong * DonGia; } }
    }
}
