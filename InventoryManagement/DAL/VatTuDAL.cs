using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class VatTuDAL
    {
        /// <summary>
        /// Tìm vật tư theo mã hoặc tên. Từ khóa truyền qua SqlParameter nên chuỗi như
        /// "' OR 1=1 --" chỉ được coi là dữ liệu, không thể chèn lệnh SQL.
        /// </summary>
        public List<VatTuDTO> Search(string tuKhoa)
        {
            List<VatTuDTO> list = new List<VatTuDTO>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT MaVatTu, TenVatTu, DVT, SoLuongTon, DinhMucToiThieu FROM VatTu
                  WHERE MaVatTu LIKE @TuKhoa OR TenVatTu LIKE @TuKhoa
                  ORDER BY MaVatTu", conn))
            {
                cmd.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 160).Value = "%" + (tuKhoa ?? "") + "%";
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read()) list.Add(Map(rd));
                }
            }
            return list;
        }

        public VatTuDTO GetByMa(string ma)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT MaVatTu, TenVatTu, DVT, SoLuongTon, DinhMucToiThieu FROM VatTu WHERE MaVatTu = @Ma", conn))
            {
                cmd.Parameters.Add("@Ma", SqlDbType.VarChar, 20).Value = ma;
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    return rd.Read() ? Map(rd) : null;
                }
            }
        }

        public bool Insert(VatTuDTO v)
        {
            return DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO VatTu (MaVatTu, TenVatTu, DVT, SoLuongTon, DinhMucToiThieu)
                  VALUES (@Ma, @Ten, @DVT, 0, @DinhMuc)",
                new SqlParameter("@Ma", v.MaVatTu), new SqlParameter("@Ten", v.TenVatTu),
                new SqlParameter("@DVT", (object)v.DVT ?? DBNull.Value), new SqlParameter("@DinhMuc", v.DinhMucToiThieu)) > 0;
        }

        /// <summary>Sửa thông tin vật tư. Số lượng tồn chỉ thay đổi qua phiếu nhập / xuất.</summary>
        public bool Update(VatTuDTO v)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE VatTu SET TenVatTu = @Ten, DVT = @DVT, DinhMucToiThieu = @DinhMuc WHERE MaVatTu = @Ma",
                new SqlParameter("@Ma", v.MaVatTu), new SqlParameter("@Ten", v.TenVatTu),
                new SqlParameter("@DVT", (object)v.DVT ?? DBNull.Value), new SqlParameter("@DinhMuc", v.DinhMucToiThieu)) > 0;
        }

        public bool Delete(string ma)
        {
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM VatTu WHERE MaVatTu = @Ma", new SqlParameter("@Ma", ma)) > 0;
        }

        public int CountChiTiet(string ma)
        {
            return (int)DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM ChiTietPhieuKho WHERE MaVatTu = @Ma", new SqlParameter("@Ma", ma));
        }

        /// <summary>Vật tư có tồn kho dưới ngưỡng an toàn (DinhMucToiThieu).</summary>
        public DataTable GetDuoiDinhMuc()
        {
            return DatabaseHelper.ExecuteQuery(
                @"SELECT MaVatTu AS [Mã VT], TenVatTu AS [Tên vật tư], DVT AS [ĐVT],
                         SoLuongTon AS [Tồn kho], DinhMucToiThieu AS [Định mức],
                         DinhMucToiThieu - SoLuongTon AS [Cần nhập thêm]
                  FROM VatTu WHERE SoLuongTon < DinhMucToiThieu
                  ORDER BY (DinhMucToiThieu - SoLuongTon) DESC");
        }

        /// <summary>
        /// Tổng giá trị hàng tồn kho (ExecuteScalar) = Σ SoLuongTon × đơn giá nhập gần nhất của vật tư.
        /// </summary>
        public decimal GetTongGiaTriTon()
        {
            object kq = DatabaseHelper.ExecuteScalar(
                @"SELECT ISNULL(SUM(v.SoLuongTon * ISNULL(g.DonGia, 0)), 0)
                  FROM VatTu v
                  OUTER APPLY (SELECT TOP 1 ct.DonGia
                               FROM ChiTietPhieuKho ct JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                               WHERE ct.MaVatTu = v.MaVatTu AND p.LoaiPhieu = 'NHAP'
                               ORDER BY p.NgayLap DESC, p.MaPhieu DESC) g");
            return Convert.ToDecimal(kq);
        }

        public decimal GetDonGiaNhapGanNhat(string ma)
        {
            object kq = DatabaseHelper.ExecuteScalar(
                @"SELECT TOP 1 ct.DonGia FROM ChiTietPhieuKho ct JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                  WHERE ct.MaVatTu = @Ma AND p.LoaiPhieu = 'NHAP' ORDER BY p.NgayLap DESC, p.MaPhieu DESC",
                new SqlParameter("@Ma", ma));
            return kq == null || kq == DBNull.Value ? 0 : Convert.ToDecimal(kq);
        }

        private static VatTuDTO Map(SqlDataReader rd)
        {
            return new VatTuDTO
            {
                MaVatTu = rd.GetString(0),
                TenVatTu = rd.GetString(1),
                DVT = rd.IsDBNull(2) ? "" : rd.GetString(2),
                SoLuongTon = rd.GetDecimal(3),
                DinhMucToiThieu = rd.GetDecimal(4)
            };
        }
    }
}
