using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventoryManagement.BUS;

namespace InventoryManagement.GUI
{
    /// <summary>Cảnh báo tồn kho dưới định mức, tổng giá trị tồn (ExecuteScalar), lịch sử phiếu kho.</summary>
    public class UcBaoCao : UserControl, ICoTheTaiLai
    {
        private readonly VatTuBUS vtBUS = new VatTuBUS();
        private readonly PhieuKhoBUS phieuBUS = new PhieuKhoBUS();

        private readonly Label lblGiaTriTon, lblSoCanhBao;
        private readonly DataGridView dgvCanhBao, dgvPhieu, dgvChiTiet;
        private readonly DateTimePicker dtpTu, dtpDen;
        private readonly ComboBox cbLoai;

        public UcBaoCao()
        {
            Padding = new Padding(8);

            // ---- Thẻ tổng quan ----
            FlowLayoutPanel the = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 70 };
            lblGiaTriTon = TaoThe(Color.FromArgb(9, 105, 218));
            lblSoCanhBao = TaoThe(Color.Firebrick);
            the.Controls.Add(lblGiaTriTon);
            the.Controls.Add(lblSoCanhBao);

            // ---- Cảnh báo ----
            dgvCanhBao = UiHelper.TaoLuoi();
            dgvCanhBao.DefaultCellStyle.ForeColor = Color.Firebrick;
            GroupBox gbCanhBao = new GroupBox { Text = "⚠ Vật tư tồn kho dưới định mức an toàn", Dock = DockStyle.Fill };
            gbCanhBao.Controls.Add(dgvCanhBao);

            // ---- Lịch sử phiếu ----
            GroupBox gbPhieu = new GroupBox { Text = "Lịch sử phiếu kho", Dock = DockStyle.Fill };
            FlowLayoutPanel loc = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            dtpTu = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today.AddDays(-30) };
            dtpDen = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today };
            cbLoai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            cbLoai.Items.AddRange(new object[] { "Tất cả", "NHAP", "XUAT" });
            cbLoai.SelectedIndex = 0;
            loc.Controls.AddRange(new Control[]
            {
                new Label { Text = "Từ:", AutoSize = true, Margin = new Padding(0, 7, 0, 0) }, dtpTu,
                new Label { Text = "Đến:", AutoSize = true, Margin = new Padding(6, 7, 0, 0) }, dtpDen,
                cbLoai, UiHelper.TaoNut("Xem", TaiPhieu, 70)
            });
            dgvPhieu = UiHelper.TaoLuoi();
            dgvChiTiet = UiHelper.TaoLuoi();
            dgvChiTiet.Dock = DockStyle.Bottom;
            dgvChiTiet.Height = 130;
            dgvPhieu.SelectionChanged += (s, e) => UiHelper.ThucHien(TaiChiTiet);
            gbPhieu.Controls.Add(dgvPhieu);
            gbPhieu.Controls.Add(dgvChiTiet);
            gbPhieu.Controls.Add(loc);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 520 };
            split.SizeChanged += (s2, e2) => { int d = split.Width * 45 / 100; if (d > 100) split.SplitterDistance = d; };
            split.Panel1.Controls.Add(gbCanhBao);
            split.Panel2.Controls.Add(gbPhieu);

            Controls.Add(split);
            Controls.Add(the);
        }

        private static Label TaoThe(Color mau)
        {
            return new Label
            {
                AutoSize = false, Width = 420, Height = 58, BackColor = mau, ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0), Margin = new Padding(0, 0, 12, 0)
            };
        }

        public void TaiLai()
        {
            decimal tongGiaTri = vtBUS.GetTongGiaTriTon(); // ExecuteScalar
            lblGiaTriTon.Text = "Tổng giá trị tồn kho:\n" + tongGiaTri.ToString("N0") + " đ";

            DataTable canhBao = vtBUS.GetDuoiDinhMuc();
            dgvCanhBao.DataSource = canhBao;
            foreach (DataGridViewColumn c in dgvCanhBao.Columns)
                if (c.ValueType == typeof(decimal)) c.DefaultCellStyle.Format = "N2";
            lblSoCanhBao.Text = "Vật tư dưới định mức:\n" + canhBao.Rows.Count + " mặt hàng cần nhập thêm";

            TaiPhieu();
        }

        private void TaiPhieu()
        {
            string loai = cbLoai.SelectedIndex > 0 ? cbLoai.SelectedItem.ToString() : "";
            dgvPhieu.DataSource = phieuBUS.GetDanhSachPhieu(dtpTu.Value, dtpDen.Value, loai);
            if (dgvPhieu.Columns.Contains("Tổng tiền")) dgvPhieu.Columns["Tổng tiền"].DefaultCellStyle.Format = "N0";
            if (dgvPhieu.Columns.Contains("Ngày lập")) dgvPhieu.Columns["Ngày lập"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            TaiChiTiet();
        }

        private void TaiChiTiet()
        {
            if (dgvPhieu.CurrentRow == null) { dgvChiTiet.DataSource = null; return; }
            int maPhieu = Convert.ToInt32(dgvPhieu.CurrentRow.Cells["Mã phiếu"].Value);
            dgvChiTiet.DataSource = phieuBUS.GetChiTiet(maPhieu);
            foreach (string c in new[] { "Đơn giá", "Thành tiền" })
                if (dgvChiTiet.Columns.Contains(c)) dgvChiTiet.Columns[c].DefaultCellStyle.Format = "N0";
        }
    }
}
