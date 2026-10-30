namespace InventoryManagement.DTO
{
    /// <summary>Dữ liệu vật tư / sản phẩm (bảng VatTu).</summary>
    public class VatTuDTO
    {
        public string MaVatTu { get; set; }
        public string TenVatTu { get; set; }
        public string DVT { get; set; }
        public decimal SoLuongTon { get; set; }
        public decimal DinhMucToiThieu { get; set; }

        public override string ToString()
        {
            return MaVatTu + " - " + TenVatTu + " (tồn " + SoLuongTon.ToString("N0") + " " + DVT + ")";
        }
    }
}
