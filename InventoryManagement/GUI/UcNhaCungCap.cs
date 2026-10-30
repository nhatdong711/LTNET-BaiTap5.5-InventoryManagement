using System.Drawing;
using System.Windows.Forms;
using InventoryManagement.BUS;
using InventoryManagement.DTO;

namespace InventoryManagement.GUI
{
    /// <summary>Thêm / Sửa / Xóa / Tra cứu nhà cung cấp (GUI → BUS → DAL).</summary>
    public class UcNhaCungCap : UserControl, ICoTheTaiLai
    {
        private readonly NhaCungCapBUS bus = new NhaCungCapBUS();
        private DataGridView dgv;
        private TextBox txtTim, txtMa, txtTen, txtDT, txtDiaChi;

        public UcNhaCungCap()
        {
            Padding = new Padding(8);

            Panel pnlTim = new Panel { Dock = DockStyle.Top, Height = 42 };
            txtTim = new TextBox { Location = new Point(150, 8), Width = 280 };
            pnlTim.Controls.Add(new Label { Text = "Tìm theo tên / SĐT:", AutoSize = true, Location = new Point(0, 11) });
            pnlTim.Controls.Add(txtTim);
            Button btnTim = UiHelper.TaoNut("Tìm", TaiLai, 80);
            btnTim.Location = new Point(440, 5);
            pnlTim.Controls.Add(btnTim);
            txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; UiHelper.ThucHien(TaiLai); } };

            dgv = UiHelper.TaoLuoi();
            dgv.SelectionChanged += (s, e) =>
            {
                NhaCungCapDTO n = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as NhaCungCapDTO;
                if (n == null) return;
                txtMa.Text = n.MaNCC.ToString();
                txtTen.Text = n.TenNCC;
                txtDT.Text = n.DienThoai;
                txtDiaChi.Text = n.DiaChi;
            };
            dgv.DataBindingComplete += (s, e) =>
            {
                if (dgv.Columns.Contains("MaNCC")) dgv.Columns["MaNCC"].HeaderText = "Mã";
                if (dgv.Columns.Contains("TenNCC")) dgv.Columns["TenNCC"].HeaderText = "Tên nhà cung cấp";
                if (dgv.Columns.Contains("DienThoai")) dgv.Columns["DienThoai"].HeaderText = "Điện thoại";
                if (dgv.Columns.Contains("DiaChi")) dgv.Columns["DiaChi"].HeaderText = "Địa chỉ";
            };

            TableLayoutPanel p = UiHelper.TaoKhungNhap();
            txtMa = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            txtTen = new TextBox { Dock = DockStyle.Fill };
            txtDT = new TextBox { Dock = DockStyle.Fill };
            txtDiaChi = new TextBox { Dock = DockStyle.Fill };
            UiHelper.ThemDong(p, "Mã NCC:", txtMa);
            UiHelper.ThemDong(p, "Tên NCC:", txtTen);
            UiHelper.ThemDong(p, "Điện thoại:", txtDT);
            UiHelper.ThemDong(p, "Địa chỉ:", txtDiaChi);
            UiHelper.ThemNut(p,
                UiHelper.TaoNut("Thêm", () => { bus.Them(DocForm()); UiHelper.ThongBao("Đã thêm nhà cung cấp."); TaiLai(); }),
                UiHelper.TaoNut("Sửa", () => { bus.Sua(DocForm()); UiHelper.ThongBao("Đã cập nhật."); TaiLai(); }),
                UiHelper.TaoNut("Xóa", () =>
                {
                    NhaCungCapDTO n = DocForm();
                    if (!UiHelper.XacNhan("Xóa nhà cung cấp \"" + n.TenNCC + "\"?")) return;
                    bus.Xoa(n.MaNCC); UiHelper.ThongBao("Đã xóa."); TaiLai();
                }),
                UiHelper.TaoNut("Làm mới", () => { txtMa.Clear(); txtTen.Clear(); txtDT.Clear(); txtDiaChi.Clear(); txtTen.Focus(); }));

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 680 };
            split.Panel1.Controls.Add(dgv);
            split.Panel1.Controls.Add(pnlTim);
            split.Panel2.Controls.Add(p);
            Controls.Add(split);
        }

        public void TaiLai()
        {
            dgv.DataSource = bus.TimKiem(txtTim.Text);
        }

        private NhaCungCapDTO DocForm()
        {
            int ma;
            int.TryParse(txtMa.Text, out ma);
            return new NhaCungCapDTO { MaNCC = ma, TenNCC = txtTen.Text, DienThoai = txtDT.Text, DiaChi = txtDiaChi.Text };
        }
    }
}
