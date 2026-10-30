using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class NhaCungCapBUS
    {
        private readonly NhaCungCapDAL dal = new NhaCungCapDAL();

        public List<NhaCungCapDTO> TimKiem(string tuKhoa) { return dal.Search((tuKhoa ?? "").Trim()); }
        public List<NhaCungCapDTO> GetAll() { return dal.Search(""); }

        public bool Them(NhaCungCapDTO n) { KiemTra(n); return dal.Insert(n); }

        public bool Sua(NhaCungCapDTO n)
        {
            if (n.MaNCC <= 0) throw new ArgumentException("Vui lòng chọn nhà cung cấp cần sửa.");
            KiemTra(n);
            return dal.Update(n);
        }

        public bool Xoa(int maNCC)
        {
            if (maNCC <= 0) throw new ArgumentException("Vui lòng chọn nhà cung cấp cần xóa.");
            if (dal.CountPhieu(maNCC) > 0) throw new InvalidOperationException("Nhà cung cấp đã có phiếu nhập, không thể xóa.");
            return dal.Delete(maNCC);
        }

        private void KiemTra(NhaCungCapDTO n)
        {
            if (string.IsNullOrWhiteSpace(n.TenNCC)) throw new ArgumentException("Tên nhà cung cấp không được để trống.");
            n.TenNCC = n.TenNCC.Trim();
            n.DienThoai = (n.DienThoai ?? "").Trim();
            n.DiaChi = (n.DiaChi ?? "").Trim();
            if (n.DienThoai.Length > 0)
            {
                if (!n.DienThoai.All(char.IsDigit) || n.DienThoai.Length < 9 || n.DienThoai.Length > 15)
                    throw new ArgumentException("Số điện thoại chỉ gồm 9–15 chữ số.");
                if (dal.TrungDienThoai(n.DienThoai, n.MaNCC))
                    throw new ArgumentException("Số điện thoại đã thuộc về nhà cung cấp khác.");
            }
        }
    }
}
