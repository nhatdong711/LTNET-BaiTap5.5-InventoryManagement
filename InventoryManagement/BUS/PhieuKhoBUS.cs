using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class PhieuKhoBUS
    {
        private readonly PhieuKhoDAL dal = new PhieuKhoDAL();

        /// <summary>
        /// Thêm một dòng vào danh sách chi tiết đang soạn. Nếu vật tư đã có thì cộng dồn số lượng
        /// (vì khóa chính ChiTietPhieuKho là MaPhieu + MaVatTu).
        /// </summary>
        public void ThemDong(List<ChiTietPhieuKhoDTO> ds, VatTuDTO vt, decimal soLuong, decimal donGia)
        {
            if (vt == null) throw new ArgumentException("Vui lòng chọn vật tư.");
            if (soLuong <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            if (donGia < 0) throw new ArgumentException("Đơn giá không được âm.");

            ChiTietPhieuKhoDTO daCo = ds.FirstOrDefault(x => x.MaVatTu == vt.MaVatTu);
            if (daCo != null)
            {
                daCo.SoLuong += soLuong;
                daCo.DonGia = donGia;
            }
            else
            {
                ds.Add(new ChiTietPhieuKhoDTO { MaVatTu = vt.MaVatTu, TenVatTu = vt.TenVatTu, DVT = vt.DVT, SoLuong = soLuong, DonGia = donGia });
            }
        }

        public int LuuPhieu(PhieuKhoDTO phieu)
        {
            if (phieu.LoaiPhieu != PhieuKhoDTO.NHAP && phieu.LoaiPhieu != PhieuKhoDTO.XUAT)
                throw new ArgumentException("Loại phiếu phải là NHAP hoặc XUAT.");
            if (phieu.LoaiPhieu == PhieuKhoDTO.NHAP && !phieu.MaNCC.HasValue)
                throw new ArgumentException("Phiếu nhập phải chọn nhà cung cấp.");
            if (phieu.LoaiPhieu == PhieuKhoDTO.XUAT) phieu.MaNCC = null;
            if (phieu.ChiTiet == null || phieu.ChiTiet.Count == 0)
                throw new ArgumentException("Phiếu chưa có vật tư nào.");
            if (phieu.ChiTiet.Any(c => c.SoLuong <= 0)) throw new ArgumentException("Số lượng mỗi dòng phải lớn hơn 0.");

            phieu.TongTien = phieu.ChiTiet.Sum(c => c.ThanhTien);
            phieu.NgayLap = DateTime.Now;
            return dal.LuuPhieuKho(phieu);
        }

        public DataTable GetDanhSachPhieu(DateTime tu, DateTime den, string loai)
        {
            if (tu.Date > den.Date) throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
            return dal.GetDanhSachPhieu(tu, den, loai);
        }

        public DataTable GetChiTiet(int maPhieu) { return dal.GetChiTiet(maPhieu); }
    }
}
