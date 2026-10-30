using System;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class PhieuKhoDAL
    {
        /// <summary>
        /// Lưu phiếu kho trong MỘT SqlTransaction (nguyên tắc ACID):
        ///   1. Chèn bản ghi PhieuKho
        ///   2. Chèn các bản ghi ChiTietPhieuKho
        ///   3. Cập nhật SoLuongTon: NHAP thì cộng, XUAT thì trừ (thiếu hàng → Rollback, báo "Không đủ hàng trong kho")
        ///   4. Commit khi tất cả thành công
        /// Trả về mã phiếu vừa tạo.
        /// </summary>
        public int LuuPhieuKho(PhieuKhoDTO phieu)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // Bước 1: PhieuKho
                    SqlCommand cmdPhieu = new SqlCommand(
                        @"INSERT INTO PhieuKho (LoaiPhieu, NgayLap, MaNCC, TongTien) VALUES (@Loai, @Ngay, @MaNCC, @Tong);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans);
                    cmdPhieu.Parameters.Add("@Loai", SqlDbType.NVarChar, 10).Value = phieu.LoaiPhieu;
                    cmdPhieu.Parameters.Add("@Ngay", SqlDbType.DateTime).Value = phieu.NgayLap;
                    cmdPhieu.Parameters.Add("@MaNCC", SqlDbType.Int).Value = phieu.MaNCC.HasValue ? (object)phieu.MaNCC.Value : DBNull.Value;
                    cmdPhieu.Parameters.Add("@Tong", SqlDbType.Decimal).Value = phieu.TongTien;
                    int maPhieu = (int)cmdPhieu.ExecuteScalar();

                    foreach (ChiTietPhieuKhoDTO ct in phieu.ChiTiet)
                    {
                        // Bước 2: ChiTietPhieuKho
                        SqlCommand cmdCT = new SqlCommand(
                            "INSERT INTO ChiTietPhieuKho (MaPhieu, MaVatTu, SoLuong, DonGia) VALUES (@MaPhieu, @MaVT, @SL, @Gia)",
                            conn, trans);
                        cmdCT.Parameters.Add("@MaPhieu", SqlDbType.Int).Value = maPhieu;
                        cmdCT.Parameters.Add("@MaVT", SqlDbType.VarChar, 20).Value = ct.MaVatTu;
                        cmdCT.Parameters.Add("@SL", SqlDbType.Decimal).Value = ct.SoLuong;
                        cmdCT.Parameters.Add("@Gia", SqlDbType.Decimal).Value = ct.DonGia;
                        cmdCT.ExecuteNonQuery();

                        // Bước 3: cập nhật tồn kho
                        if (phieu.LoaiPhieu == PhieuKhoDTO.NHAP)
                        {
                            SqlCommand cmdNhap = new SqlCommand(
                                "UPDATE VatTu SET SoLuongTon = SoLuongTon + @SL WHERE MaVatTu = @MaVT", conn, trans);
                            cmdNhap.Parameters.Add("@SL", SqlDbType.Decimal).Value = ct.SoLuong;
                            cmdNhap.Parameters.Add("@MaVT", SqlDbType.VarChar, 20).Value = ct.MaVatTu;
                            cmdNhap.ExecuteNonQuery();
                        }
                        else
                        {
                            // Đọc tồn kho hiện tại (khóa dòng để tránh 2 phiếu xuất cùng lúc)
                            SqlCommand cmdTon = new SqlCommand(
                                "SELECT SoLuongTon FROM VatTu WITH (UPDLOCK, ROWLOCK) WHERE MaVatTu = @MaVT", conn, trans);
                            cmdTon.Parameters.Add("@MaVT", SqlDbType.VarChar, 20).Value = ct.MaVatTu;
                            decimal soLuongTon = Convert.ToDecimal(cmdTon.ExecuteScalar());

                            if (soLuongTon < ct.SoLuong)
                            {
                                trans.Rollback();
                                throw new InvalidOperationException(string.Format(
                                    "Không đủ hàng trong kho!\nVật tư {0}: tồn {1:N2}, cần xuất {2:N2}.\nToàn bộ phiếu đã được hủy (Rollback).",
                                    ct.MaVatTu, soLuongTon, ct.SoLuong));
                            }

                            SqlCommand cmdXuat = new SqlCommand(
                                "UPDATE VatTu SET SoLuongTon = SoLuongTon - @SL WHERE MaVatTu = @MaVT", conn, trans);
                            cmdXuat.Parameters.Add("@SL", SqlDbType.Decimal).Value = ct.SoLuong;
                            cmdXuat.Parameters.Add("@MaVT", SqlDbType.VarChar, 20).Value = ct.MaVatTu;
                            cmdXuat.ExecuteNonQuery();
                        }
                    }

                    // Bước 4
                    trans.Commit();
                    return maPhieu;
                }
                catch (InvalidOperationException)
                {
                    throw; // đã Rollback ở trên
                }
                catch
                {
                    try { trans.Rollback(); } catch { /* giao dịch có thể đã bị hủy */ }
                    throw;
                }
            }
        }

        public DataTable GetDanhSachPhieu(DateTime tu, DateTime den, string loai)
        {
            return DatabaseHelper.ExecuteQuery(
                @"SELECT p.MaPhieu AS [Mã phiếu], p.LoaiPhieu AS [Loại], p.NgayLap AS [Ngày lập],
                         ISNULL(n.TenNCC, N'(Xuất kho)') AS [Nhà cung cấp], p.TongTien AS [Tổng tiền]
                  FROM PhieuKho p LEFT JOIN NhaCungCap n ON p.MaNCC = n.MaNCC
                  WHERE p.NgayLap >= @Tu AND p.NgayLap < @Den AND (@Loai = '' OR p.LoaiPhieu = @Loai)
                  ORDER BY p.NgayLap DESC",
                new SqlParameter("@Tu", tu.Date), new SqlParameter("@Den", den.Date.AddDays(1)),
                new SqlParameter("@Loai", loai ?? ""));
        }

        public DataTable GetChiTiet(int maPhieu)
        {
            return DatabaseHelper.ExecuteQuery(
                @"SELECT ct.MaVatTu AS [Mã VT], v.TenVatTu AS [Tên vật tư], v.DVT AS [ĐVT],
                         ct.SoLuong AS [Số lượng], ct.DonGia AS [Đơn giá], ct.SoLuong * ct.DonGia AS [Thành tiền]
                  FROM ChiTietPhieuKho ct JOIN VatTu v ON ct.MaVatTu = v.MaVatTu
                  WHERE ct.MaPhieu = @Ma",
                new SqlParameter("@Ma", maPhieu));
        }
    }
}
