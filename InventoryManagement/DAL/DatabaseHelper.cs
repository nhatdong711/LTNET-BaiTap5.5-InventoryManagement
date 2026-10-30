using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace InventoryManagement.DAL
{
    /// <summary>
    /// Lớp tiện ích kết nối CSDL bằng ADO.NET.
    /// Chuỗi kết nối đọc từ App.config (name = "InventoryDB").
    /// Mọi câu lệnh đều truyền tham số bằng SqlParameter để chống SQL Injection.
    /// </summary>
    public static class DatabaseHelper
    {
        private const string DEFAULT_CONN =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=InventoryDB;Integrated Security=True;TrustServerCertificate=True";

        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["InventoryDB"];
                return cs != null ? cs.ConnectionString : DEFAULT_CONN;
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>Mô hình ngắt kết nối: SqlDataAdapter đổ dữ liệu vào DataTable.</summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt); // Fill tự mở và đóng kết nối
                }
                return dt;
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
