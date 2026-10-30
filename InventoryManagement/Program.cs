using System;
using System.Windows.Forms;
using InventoryManagement.GUI;

namespace InventoryManagement
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                DAL.DbInit.EnsureDatabase(); // lần đầu chạy: tự tạo CSDL từ Database/ScriptDB.sql
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không kết nối / khởi tạo được CSDL.\n" +
                    "Kiểm tra chuỗi kết nối trong App.config (mặc định dùng (localdb)\\MSSQLLocalDB).\n\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new FrmMain());
        }
    }
}
