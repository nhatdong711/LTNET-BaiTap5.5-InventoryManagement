using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    /// <summary>Mô hình kết nối (Connected Model): SqlConnection + SqlCommand + SqlDataReader.</summary>
    public class NhaCungCapDAL
    {
        public List<NhaCungCapDTO> Search(string tuKhoa)
        {
            List<NhaCungCapDTO> list = new List<NhaCungCapDTO>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT MaNCC, TenNCC, DienThoai, DiaChi FROM NhaCungCap
                  WHERE TenNCC LIKE @TuKhoa OR DienThoai LIKE @TuKhoa
                  ORDER BY MaNCC", conn))
            {
                cmd.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 160).Value = "%" + (tuKhoa ?? "") + "%";
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new NhaCungCapDTO
                        {
                            MaNCC = rd.GetInt32(0),
                            TenNCC = rd.GetString(1),
                            DienThoai = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            DiaChi = rd.IsDBNull(3) ? "" : rd.GetString(3)
                        });
                    }
                }
            }
            return list;
        }

        public bool Insert(NhaCungCapDTO n)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO NhaCungCap (TenNCC, DienThoai, DiaChi) VALUES (@Ten, @DT, @DC)",
                new SqlParameter("@Ten", n.TenNCC), DbNull("@DT", n.DienThoai), DbNull("@DC", n.DiaChi)) > 0;
        }

        public bool Update(NhaCungCapDTO n)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE NhaCungCap SET TenNCC = @Ten, DienThoai = @DT, DiaChi = @DC WHERE MaNCC = @Ma",
                new SqlParameter("@Ten", n.TenNCC), DbNull("@DT", n.DienThoai), DbNull("@DC", n.DiaChi),
                new SqlParameter("@Ma", n.MaNCC)) > 0;
        }

        public bool Delete(int maNCC)
        {
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM NhaCungCap WHERE MaNCC = @Ma", new SqlParameter("@Ma", maNCC)) > 0;
        }

        public bool TrungDienThoai(string dienThoai, int boQuaMaNCC)
        {
            return (int)DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM NhaCungCap WHERE DienThoai = @DT AND MaNCC <> @Ma",
                new SqlParameter("@DT", dienThoai), new SqlParameter("@Ma", boQuaMaNCC)) > 0;
        }

        public int CountPhieu(int maNCC)
        {
            return (int)DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM PhieuKho WHERE MaNCC = @Ma", new SqlParameter("@Ma", maNCC));
        }

        private static SqlParameter DbNull(string ten, string giaTri)
        {
            return new SqlParameter(ten, string.IsNullOrWhiteSpace(giaTri) ? (object)DBNull.Value : giaTri);
        }
    }
}
