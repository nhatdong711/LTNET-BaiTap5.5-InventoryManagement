using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagement.GUI
{
    /// <summary>Các hàm dựng giao diện và xử lý lỗi dùng chung cho các Form.</summary>
    internal static class UiHelper
    {
        public static DataGridView TaoLuoi()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
        }

        public static TableLayoutPanel TaoKhungNhap(int rongNhan = 120)
        {
            TableLayoutPanel p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10), AutoScroll = true };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rongNhan));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return p;
        }

        public static void ThemDong(TableLayoutPanel p, string nhan, Control c)
        {
            int row = p.RowCount++;
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            if (nhan != null) p.Controls.Add(new Label { Text = nhan, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            p.Controls.Add(c, nhan == null ? 0 : 1, row);
            if (nhan == null) p.SetColumnSpan(c, 2);
        }

        public static void ThemNut(TableLayoutPanel p, params Button[] nut)
        {
            FlowLayoutPanel f = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
            f.Controls.AddRange(nut);
            int row = p.RowCount++;
            p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            p.Controls.Add(f, 0, row);
            p.SetColumnSpan(f, 2);
        }

        public static Button TaoNut(string text, Action action, int rong = 90)
        {
            Button b = new Button { Text = text, Width = rong, Height = 34 };
            b.Click += (s, e) => ThucHien(action);
            return b;
        }

        public static void ThongBao(string msg)
        {
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool XacNhan(string msg)
        {
            return MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>Bọc thao tác: lỗi nhập liệu/nghiệp vụ → cảnh báo; lỗi SQL → báo đã Rollback.</summary>
        public static void ThucHien(Action action)
        {
            try
            {
                action();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi CSDL (giao dịch đã được Rollback):\n" + ex.Message, "Lỗi SQL Server",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
