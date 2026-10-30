namespace InventoryManagement.DTO
{
    /// <summary>Dữ liệu nhà cung cấp (bảng NhaCungCap).</summary>
    public class NhaCungCapDTO
    {
        public int MaNCC { get; set; }
        public string TenNCC { get; set; }
        public string DienThoai { get; set; }
        public string DiaChi { get; set; }

        public override string ToString() { return TenNCC; }
    }
}
