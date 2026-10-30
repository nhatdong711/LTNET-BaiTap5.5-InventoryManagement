using System;
using System.Collections.Generic;

namespace InventoryManagement.DTO
{
    /// <summary>Dữ liệu phiếu nhập / xuất kho (bảng PhieuKho) kèm danh sách chi tiết.</summary>
    public class PhieuKhoDTO
    {
        public const string NHAP = "NHAP";
        public const string XUAT = "XUAT";

        public int MaPhieu { get; set; }
        public string LoaiPhieu { get; set; }
        public DateTime NgayLap { get; set; }
        public int? MaNCC { get; set; }
        public decimal TongTien { get; set; }

        public List<ChiTietPhieuKhoDTO> ChiTiet { get; set; }

        public PhieuKhoDTO()
        {
            ChiTiet = new List<ChiTietPhieuKhoDTO>();
            NgayLap = DateTime.Now;
        }
    }
}
