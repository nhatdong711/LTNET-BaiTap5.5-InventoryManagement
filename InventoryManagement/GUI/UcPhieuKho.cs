using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InventoryManagement.BUS;
using InventoryManagement.DTO;

namespace InventoryManagement.GUI
{
    /// <summary>Lập phiếu nhập / xuất kho. Bấm "Lưu phiếu kho" → BUS → DAL chạy một SqlTransaction.</summary>
    public class UcPhieuKho : UserControl, ICoTheTaiLai
    {
        private readonly PhieuKhoBUS phieuBUS = new PhieuKhoBUS();
        private readonly VatTuBUS vtBUS = new VatTuBUS();
        private readonly NhaCungCapBUS nccBUS = new NhaCungCapBUS();

        private readonly List<ChiTietPhieuKhoDTO> dsChiTiet = new List<ChiTietPhieuKhoDTO>();
        private readonly BindingSource bsChiTiet = new BindingSource();

        private RadioButton rdoNhap, rdoXuat;
        private ComboBox cbNCC, cbVatTu;
        private NumericUpDown nudSoLuong, nudDonGia;
        private DataGridView dgvChiTiet;
        private Label lblTong, lblTon;

        public UcPhieuKho()
        {
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            // ---- Thông tin phiếu ----
            GroupBox gbPhieu = new GroupBox { Text = "Thông tin phiếu", Dock = DockStyle.Top, Height = 80 };
            FlowLayoutPanel fp = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(6, 4, 6, 0) };
            rdoNhap = new RadioButton { Text = "Phiếu NHẬP", Checked = true, AutoSize = true, ForeColor = Color.DarkGreen, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 6, 10, 0) };
            rdoXuat = new RadioButton { Text = "Phiếu XUẤT", AutoSize = true, ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 6, 30, 0) };
            cbNCC = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 330 };
            fp.Controls.AddRange(new Control[] { rdoNhap, rdoXuat, new Label { Text = "Nhà cung cấp:", AutoSize = true, Margin = new Padding(3, 8, 3, 0) }, cbNCC });
            gbPhieu.Controls.Add(fp);
            rdoNhap.CheckedChanged += (s, e) => DoiLoaiPhieu();

            // ---- Thêm dòng ----
            GroupBox gbDong = new GroupBox { Text = "Thêm vật tư vào phiếu", Dock = DockStyle.Top, Height = 120 };
            TableLayoutPanel tp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 2, Padding = new Padding(6, 2, 6, 0) };
            tp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tp.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            tp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
            tp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            tp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            cbVatTu = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            nudSoLuong = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 10000000, Minimum = 0, Value = 1 };
            nudDonGia = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 0, Maximum = 1000000000, Minimum = 0, ThousandsSeparator = true };
            lblTon = new Label { AutoSize = true, ForeColor = Color.DimGray, Anchor = AnchorStyles.Left };
            tp.Controls.Add(new Label { Text = "Vật tư:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            tp.Controls.Add(cbVatTu, 1, 0);
            tp.Controls.Add(new Label { Text = "Số lượng:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
            tp.Controls.Add(nudSoLuong, 3, 0);
            tp.Controls.Add(new Label { Text = "Đơn giá:", AutoSize = true, Anchor = AnchorStyles.Left }, 4, 0);
            tp.Controls.Add(nudDonGia, 5, 0);
            tp.Controls.Add(lblTon, 1, 1);
            FlowLayoutPanel nutDong = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0) };
            nutDong.Controls.Add(UiHelper.TaoNut("Xóa dòng", XoaDong, 100));
            nutDong.Controls.Add(UiHelper.TaoNut("+ Thêm dòng", ThemDong, 120));
            tp.Controls.Add(nutDong, 2, 1);
            tp.SetColumnSpan(nutDong, 4);
            gbDong.Controls.Add(tp);
            cbVatTu.SelectedIndexChanged += (s, e) => UiHelper.ThucHien(HienThongTinVatTu);

            // ---- Lưới chi tiết ----
            dgvChiTiet = UiHelper.TaoLuoi();
            bsChiTiet.DataSource = dsChiTiet;
            dgvChiTiet.DataSource = bsChiTiet;
            dgvChiTiet.DataBindingComplete += (s, e) => DinhDangLuoi();

            // ---- Tổng + Lưu ----
            Panel duoi = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(6) };
            lblTong = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.DarkBlue, TextAlign = ContentAlignment.MiddleLeft, Text = "Tổng tiền: 0 đ" };
            Button btnLuu = UiHelper.TaoNut("LƯU PHIẾU KHO", LuuPhieu, 200);
            btnLuu.Dock = DockStyle.Right;
            btnLuu.BackColor = Color.FromArgb(9, 105, 218);
            btnLuu.ForeColor = Color.White;
            btnLuu.FlatStyle = FlatStyle.Flat;
            btnLuu.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            Button btnHuy = UiHelper.TaoNut("Hủy phiếu", HuyPhieu, 120);
            btnHuy.Dock = DockStyle.Right;
            duoi.Controls.Add(lblTong);
            duoi.Controls.Add(btnHuy);
            duoi.Controls.Add(btnLuu);

            Controls.Add(dgvChiTiet);
            Controls.Add(duoi);
            Controls.Add(gbDong);
            Controls.Add(gbPhieu);
            Padding = new Padding(8);
        }

        public void TaiLai()
        {
            string maDangChon = cbVatTu.SelectedItem is VatTuDTO ? ((VatTuDTO)cbVatTu.SelectedItem).MaVatTu : null;
            cbNCC.DataSource = nccBUS.GetAll();
            List<VatTuDTO> ds = vtBUS.GetAll();
            cbVatTu.DataSource = ds;
            VatTuDTO cu = ds.FirstOrDefault(v => v.MaVatTu == maDangChon);
            if (cu != null) cbVatTu.SelectedItem = cu;
            DoiLoaiPhieu();
        }

        private string LoaiPhieu { get { return rdoNhap.Checked ? PhieuKhoDTO.NHAP : PhieuKhoDTO.XUAT; } }

        private void DoiLoaiPhieu()
        {
            cbNCC.Enabled = rdoNhap.Checked; // phiếu xuất không có nhà cung cấp
            UiHelper.ThucHien(HienThongTinVatTu);
        }

        private void HienThongTinVatTu()
        {
            VatTuDTO vt = cbVatTu.SelectedItem as VatTuDTO;
            if (vt == null) { lblTon.Text = ""; return; }
            decimal giaGanNhat = vtBUS.GetDonGiaNhapGanNhat(vt.MaVatTu);
            lblTon.Text = string.Format("Tồn hiện tại: {0:N2} {1}  |  Giá nhập gần nhất: {2:N0} đ", vt.SoLuongTon, vt.DVT, giaGanNhat);
            if (giaGanNhat > 0 && giaGanNhat <= nudDonGia.Maximum) nudDonGia.Value = giaGanNhat;
        }

        private void ThemDong()
        {
            phieuBUS.ThemDong(dsChiTiet, cbVatTu.SelectedItem as VatTuDTO, nudSoLuong.Value, nudDonGia.Value);
            CapNhatLuoi();
            nudSoLuong.Value = 1;
        }

        private void XoaDong()
        {
            ChiTietPhieuKhoDTO ct = bsChiTiet.Current as ChiTietPhieuKhoDTO;
            if (ct == null) throw new ArgumentException("Chọn dòng cần xóa.");
            dsChiTiet.Remove(ct);
            CapNhatLuoi();
        }

        private void HuyPhieu()
        {
            dsChiTiet.Clear();
            CapNhatLuoi();
        }

        private void LuuPhieu()
        {
            NhaCungCapDTO ncc = cbNCC.SelectedItem as NhaCungCapDTO;
            PhieuKhoDTO phieu = new PhieuKhoDTO
            {
                LoaiPhieu = LoaiPhieu,
                MaNCC = rdoNhap.Checked && ncc != null ? (int?)ncc.MaNCC : null,
                ChiTiet = new List<ChiTietPhieuKhoDTO>(dsChiTiet)
            };
            if (!UiHelper.XacNhan(string.Format("Lưu phiếu {0} gồm {1} vật tư, tổng {2:N0} đ?",
                    phieu.LoaiPhieu, phieu.ChiTiet.Count, phieu.ChiTiet.Sum(c => c.ThanhTien))))
                return;

            int maPhieu = phieuBUS.LuuPhieu(phieu); // lỗi thiếu hàng → InvalidOperationException, phiếu đã Rollback
            UiHelper.ThongBao("Lưu phiếu " + phieu.LoaiPhieu + " số " + maPhieu + " thành công.\nTồn kho đã được cập nhật.");
            dsChiTiet.Clear();
            CapNhatLuoi();
            TaiLai();
        }

        private void CapNhatLuoi()
        {
            bsChiTiet.ResetBindings(false);
            lblTong.Text = "Tổng tiền: " + dsChiTiet.Sum(c => c.ThanhTien).ToString("N0") + " đ   (" + dsChiTiet.Count + " dòng)";
        }

        private void DinhDangLuoi()
        {
            string[,] cot =
            {
                { "MaVatTu", "Mã VT", "" }, { "TenVatTu", "Tên vật tư", "" }, { "DVT", "ĐVT", "" },
                { "SoLuong", "Số lượng", "N2" }, { "DonGia", "Đơn giá", "N0" }, { "ThanhTien", "Thành tiền", "N0" }
            };
            for (int i = 0; i < cot.GetLength(0); i++)
            {
                if (!dgvChiTiet.Columns.Contains(cot[i, 0])) continue;
                dgvChiTiet.Columns[cot[i, 0]].HeaderText = cot[i, 1];
                if (cot[i, 2] != "") dgvChiTiet.Columns[cot[i, 0]].DefaultCellStyle.Format = cot[i, 2];
            }
        }
    }
}
