using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagement.GUI
{
    /// <summary>Các màn hình con có thể nạp lại dữ liệu khi được chọn.</summary>
    internal interface ICoTheTaiLai
    {
        void TaiLai();
    }

    /// <summary>Màn hình chính: chứa các tab Lập phiếu, Vật tư, Nhà cung cấp, Tồn kho & Báo cáo.</summary>
    public class FrmMain : Form
    {
        private readonly TabControl tabs;

        public FrmMain()
        {
            Text = "INVENTORY MANAGEMENT - Quản lý xuất nhập kho nguyên vật liệu";
            Size = new Size(1200, 700);
            MinimumSize = new Size(1050, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 6) };
            ThemTab("Lập phiếu Nhập / Xuất", new UcPhieuKho());
            ThemTab("Vật tư", new UcVatTu());
            ThemTab("Nhà cung cấp", new UcNhaCungCap());
            ThemTab("Tồn kho & Báo cáo", new UcBaoCao());
            tabs.SelectedIndexChanged += (s, e) => TaiLaiTabHienTai();

            StatusStrip status = new StatusStrip();
            status.Items.Add("Kết nối: InventoryDB  |  Kiến trúc 3 lớp DTO – DAL – BUS – GUI  |  ADO.NET + SqlTransaction");

            Controls.Add(tabs);
            Controls.Add(status);
            Load += (s, e) => TaiLaiTabHienTai();
        }

        private void ThemTab(string tieuDe, UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            TabPage tab = new TabPage(tieuDe);
            tab.Controls.Add(uc);
            tabs.TabPages.Add(tab);
        }

        private void TaiLaiTabHienTai()
        {
            if (tabs.SelectedTab == null || tabs.SelectedTab.Controls.Count == 0) return;
            ICoTheTaiLai uc = tabs.SelectedTab.Controls[0] as ICoTheTaiLai;
            if (uc != null) UiHelper.ThucHien(uc.TaiLai);
        }
    }
}
