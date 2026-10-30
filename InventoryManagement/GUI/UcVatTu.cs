using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using InventoryManagement.BUS;
using InventoryManagement.DTO;

namespace InventoryManagement.GUI
{
    /// <summary>Thêm / Sửa / Xóa / Tra cứu vật tư (GUI → BUS → DAL).</summary>
    public class UcVatTu : UserControl, ICoTheTaiLai
    {
        private readonly VatTuBUS bus = new VatTuBUS();
        private DataGridView dgv;
        private TextBox txtTim, txtMa, txtTen, txtDVT, txtDinhMuc, txtTon;

        public UcVatTu()
        {
            Padding = new Padding(8);

            Panel pnlTim = new Panel { Dock = DockStyle.Top, Height = 42 };
            txtTim = new TextBox { Location = new Point(150, 8), Width = 280 };
            pnlTim.Controls.Add(new Label { Text = "Tìm theo mã / tên:", AutoSize = true, Location = new Point(0, 11) });
            pnlTim.Controls.Add(txtTim);
            Button btnTim = UiHelper.TaoNut("Tìm", TaiLai, 80);
            btnTim.Location = new Point(440, 5);
            pnlTim.Controls.Add(btnTim);
            txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; UiHelper.ThucHien(TaiLai); } };

            dgv = UiHelper.TaoLuoi();
            dgv.SelectionChanged += (s, e) => ChonDong();
            dgv.DataBindingComplete += (s, e) => DinhDang();
            dgv.CellFormatting += (s, e) =>
            {
                VatTuDTO v = e.RowIndex >= 0 ? dgv.Rows[e.RowIndex].DataBoundItem as VatTuDTO : null;
                if (v != null && v.SoLuongTon < v.DinhMucToiThieu) e.CellStyle.ForeColor = Color.Firebrick;
            };

            TableLayoutPanel p = UiHelper.TaoKhungNhap(130);
            txtMa = new TextBox { Dock = DockStyle.Fill, CharacterCasing = CharacterCasing.Upper };
            txtTen = new TextBox { Dock = DockStyle.Fill };
            txtDVT = new TextBox { Dock = DockStyle.Fill };
            txtDinhMuc = new TextBox { Dock = DockStyle.Fill, Text = "10" };
            txtTon = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            UiHelper.ThemDong(p, "Mã vật tư:", txtMa);
            UiHelper.ThemDong(p, "Tên vật tư:", txtTen);
            UiHelper.ThemDong(p, "Đơn vị tính:", txtDVT);
            UiHelper.ThemDong(p, "Định mức tối thiểu:", txtDinhMuc);
            UiHelper.ThemDong(p, "Số lượng tồn:", txtTon);
            UiHelper.ThemDong(p, null, new Label { Text = "Số lượng tồn chỉ thay đổi qua phiếu nhập / xuất.", AutoSize = true, ForeColor = Color.DimGray });
            UiHelper.ThemNut(p,
                UiHelper.TaoNut("Thêm", () => { bus.Them(DocForm()); UiHelper.ThongBao("Đã thêm vật tư."); TaiLai(); }),
                UiHelper.TaoNut("Sửa", () => { bus.Sua(DocForm()); UiHelper.ThongBao("Đã cập nhật vật tư."); TaiLai(); }),
                UiHelper.TaoNut("Xóa", () =>
                {
                    if (!UiHelper.XacNhan("Xóa vật tư " + txtMa.Text + "?")) return;
                    bus.Xoa(txtMa.Text.Trim()); UiHelper.ThongBao("Đã xóa vật tư."); TaiLai();
                }),
                UiHelper.TaoNut("Làm mới", () => { txtMa.Clear(); txtTen.Clear(); txtDVT.Clear(); txtDinhMuc.Text = "10"; txtTon.Clear(); txtMa.Focus(); }));

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 680 };
            split.FixedPanel = FixedPanel.Panel2;
            split.SizeChanged += (s2, e2) => { int d = split.Width - 400; if (d > 150) split.SplitterDistance = d; };
            split.Panel1.Controls.Add(dgv);
            split.Panel1.Controls.Add(pnlTim);
            split.Panel2.Controls.Add(p);
            Controls.Add(split);
        }

        public void TaiLai()
        {
            dgv.DataSource = bus.TimKiem(txtTim.Text);
        }

        private void DinhDang()
        {
            string[,] cot =
            {
                { "MaVatTu", "Mã VT", "" }, { "TenVatTu", "Tên vật tư", "" }, { "DVT", "ĐVT", "" },
                { "SoLuongTon", "Tồn kho", "N2" }, { "DinhMucToiThieu", "Định mức", "N2" }
            };
            for (int i = 0; i < cot.GetLength(0); i++)
            {
                if (!dgv.Columns.Contains(cot[i, 0])) continue;
                dgv.Columns[cot[i, 0]].HeaderText = cot[i, 1];
                if (cot[i, 2] != "") dgv.Columns[cot[i, 0]].DefaultCellStyle.Format = cot[i, 2];
            }
        }

        private void ChonDong()
        {
            VatTuDTO v = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as VatTuDTO;
            if (v == null) return;
            txtMa.Text = v.MaVatTu;
            txtTen.Text = v.TenVatTu;
            txtDVT.Text = v.DVT;
            txtDinhMuc.Text = v.DinhMucToiThieu.ToString("0.##");
            txtTon.Text = v.SoLuongTon.ToString("N2");
        }

        private VatTuDTO DocForm()
        {
            decimal dinhMuc;
            if (!decimal.TryParse(txtDinhMuc.Text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out dinhMuc))
                throw new ArgumentException("Định mức tối thiểu phải là số.");
            return new VatTuDTO { MaVatTu = txtMa.Text, TenVatTu = txtTen.Text, DVT = txtDVT.Text, DinhMucToiThieu = dinhMuc };
        }
    }
}
