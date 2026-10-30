using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace InventoryManagement.DAL
{
    /// <summary>
    /// Tự khởi tạo CSDL ở lần chạy đầu tiên: nếu database trong chuỗi kết nối chưa tồn tại
    /// thì chạy file Database/ScriptDB.sql (tách theo lệnh GO). Nhờ vậy có thể chạy ngay
    /// với SQL Server LocalDB đi kèm Visual Studio mà không cần mở SSMS.
    /// </summary>
    public static class DbInit
    {
        public static void EnsureDatabase()
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(DatabaseHelper.ConnectionString);
            string dbName = csb.InitialCatalog;
            csb.InitialCatalog = "master";

            using (SqlConnection conn = new SqlConnection(csb.ConnectionString))
            {
                conn.Open();
                using (SqlCommand check = new SqlCommand("SELECT DB_ID(@Ten)", conn))
                {
                    check.Parameters.AddWithValue("@Ten", dbName);
                    object id = check.ExecuteScalar();
                    if (id != null && id != DBNull.Value) return; // đã có CSDL
                }

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "ScriptDB.sql");
                if (!File.Exists(path))
                    throw new FileNotFoundException("Không tìm thấy file script tạo CSDL: " + path);

                string script = File.ReadAllText(path);
                string[] batches = Regex.Split(script, @"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                foreach (string batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    using (SqlCommand cmd = new SqlCommand(batch, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
