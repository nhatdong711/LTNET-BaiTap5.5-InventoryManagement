using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class VatTuBUS
    {
        private readonly VatTuDAL dal = new VatTuDAL();

        public List<VatTuDTO> TimKiem(string tuKhoa) { return dal.Search((tuKhoa ?? "").Trim()); }
        public List<VatTuDTO> GetAll() { return dal.Search(""); }
        public DataTable GetDuoiDinhMuc() { return dal.GetDuoiDinhMuc(); }
        public decimal GetTongGiaTriTon() { return dal.GetTongGiaTriTon(); }
        public decimal GetDonGiaNhapGanNhat(string ma) { return dal.GetDonGiaNhapGanNhat(ma); }

        public bool Them(VatTuDTO v)
        {
            KiemTra(v);
            if (dal.GetByMa(v.MaVatTu) != null) throw new ArgumentException("Mã vật tư " + v.MaVatTu + " đã tồn tại.");
            return dal.Insert(v);
        }

        public bool Sua(VatTuDTO v)
        {
            KiemTra(v);
            if (dal.GetByMa(v.MaVatTu) == null) throw new ArgumentException("Không tìm thấy vật tư " + v.MaVatTu + ".");
            return dal.Update(v);
        }

        public bool Xoa(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng chọn vật tư cần xóa.");
            if (dal.CountChiTiet(ma) > 0) throw new InvalidOperationException("Vật tư đã phát sinh phiếu nhập/xuất, không thể xóa.");
            return dal.Delete(ma);
        }

        private static void KiemTra(VatTuDTO v)
        {
            v.MaVatTu = (v.MaVatTu ?? "").Trim().ToUpper();
            if (v.MaVatTu.Length == 0) throw new ArgumentException("Mã vật tư không được để trống.");
            if (v.MaVatTu.Length > 20 || !v.MaVatTu.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                throw new ArgumentException("Mã vật tư tối đa 20 ký tự, chỉ gồm chữ, số, '-' hoặc '_'.");
            if (string.IsNullOrWhiteSpace(v.TenVatTu)) throw new ArgumentException("Tên vật tư không được để trống.");
            v.TenVatTu = v.TenVatTu.Trim();
            v.DVT = (v.DVT ?? "").Trim();
            if (v.DinhMucToiThieu < 0) throw new ArgumentException("Định mức tối thiểu không được âm.");
        }
    }
}
