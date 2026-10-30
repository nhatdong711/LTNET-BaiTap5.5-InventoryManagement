/* =========================================================
   BÀI TẬP 5.5 - HỆ THỐNG QUẢN LÝ QUY TRÌNH XUẤT - NHẬP KHO
                 NGUYÊN VẬT LIỆU (INVENTORY MANAGEMENT)
   Script tạo CSDL InventoryDB + dữ liệu mẫu
   ========================================================= */
USE master;
GO
IF DB_ID(N'InventoryDB') IS NOT NULL
BEGIN
    ALTER DATABASE InventoryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE InventoryDB;
END
GO
CREATE DATABASE InventoryDB;
GO
USE InventoryDB;
GO

-- 1. Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNCC     INT IDENTITY(1,1) PRIMARY KEY,
    TenNCC    NVARCHAR(150) NOT NULL,
    DienThoai VARCHAR(15) UNIQUE,
    DiaChi    NVARCHAR(255)
);

-- 2. Vật tư / Sản phẩm
CREATE TABLE VatTu (
    MaVatTu         VARCHAR(20) PRIMARY KEY,
    TenVatTu        NVARCHAR(150) NOT NULL,
    DVT             NVARCHAR(20),
    SoLuongTon      DECIMAL(18,2) NOT NULL DEFAULT 0 CONSTRAINT CK_VatTu_SoLuongTon CHECK (SoLuongTon >= 0),
    DinhMucToiThieu DECIMAL(18,2) NOT NULL DEFAULT 10 CHECK (DinhMucToiThieu >= 0)  -- ngưỡng tồn kho an toàn
);

-- 3. Phiếu nhập / xuất kho
CREATE TABLE PhieuKho (
    MaPhieu   INT IDENTITY(1,1) PRIMARY KEY,
    LoaiPhieu NVARCHAR(10) NOT NULL CONSTRAINT CK_PhieuKho_Loai CHECK (LoaiPhieu IN ('NHAP', 'XUAT')),
    NgayLap   DATETIME NOT NULL DEFAULT GETDATE(),
    MaNCC     INT NULL FOREIGN KEY REFERENCES NhaCungCap(MaNCC),   -- NULL nếu là phiếu xuất
    TongTien  DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT CK_PhieuKho_NCC CHECK (LoaiPhieu = 'XUAT' OR MaNCC IS NOT NULL)
);

-- 4. Chi tiết phiếu kho
CREATE TABLE ChiTietPhieuKho (
    MaPhieu INT NOT NULL FOREIGN KEY REFERENCES PhieuKho(MaPhieu),
    MaVatTu VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES VatTu(MaVatTu),
    SoLuong DECIMAL(18,2) NOT NULL CHECK (SoLuong > 0),
    DonGia  DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    PRIMARY KEY (MaPhieu, MaVatTu)
);
GO

/* ---------------- DỮ LIỆU MẪU ---------------- */
INSERT INTO NhaCungCap (TenNCC, DienThoai, DiaChi) VALUES
(N'Công ty TNHH Thép Việt',      '02438123456', N'Số 12 Nguyễn Trãi, Thanh Xuân, Hà Nội'),
(N'Công ty CP Vật liệu Hòa Phát', '02439876543', N'KCN Phố Nối A, Hưng Yên'),
(N'Nhà phân phối Điện Quang',     '02837771234', N'125 Hàm Nghi, Quận 1, TP.HCM');

INSERT INTO VatTu (MaVatTu, TenVatTu, DVT, SoLuongTon, DinhMucToiThieu) VALUES
('VT001', N'Thép cuộn phi 6',        N'Kg',    0, 200),
('VT002', N'Xi măng PCB40',          N'Bao',   0, 50),
('VT003', N'Dây điện Cadivi 2.5mm',  N'Cuộn',  0, 10),
('VT004', N'Ống nhựa PVC phi 21',    N'Cây',   0, 30),
('VT005', N'Bóng đèn LED 20W',       N'Cái',   0, 40),
('VT006', N'Sơn chống thấm 18L',     N'Thùng', 0, 5);
GO

/* Tạo vài phiếu mẫu, đồng thời cập nhật tồn kho cho khớp */
INSERT INTO PhieuKho (LoaiPhieu, NgayLap, MaNCC, TongTien) VALUES ('NHAP', DATEADD(DAY, -10, GETDATE()), 1, 0);
INSERT INTO ChiTietPhieuKho VALUES (1, 'VT001', 500, 16500), (1, 'VT002', 120, 92000);

INSERT INTO PhieuKho (LoaiPhieu, NgayLap, MaNCC, TongTien) VALUES ('NHAP', DATEADD(DAY, -6, GETDATE()), 3, 0);
INSERT INTO ChiTietPhieuKho VALUES (2, 'VT003', 25, 850000), (2, 'VT005', 100, 65000), (2, 'VT004', 20, 28000);

INSERT INTO PhieuKho (LoaiPhieu, NgayLap, MaNCC, TongTien) VALUES ('NHAP', DATEADD(DAY, -4, GETDATE()), 2, 0);
INSERT INTO ChiTietPhieuKho VALUES (3, 'VT006', 8, 1450000);

INSERT INTO PhieuKho (LoaiPhieu, NgayLap, MaNCC, TongTien) VALUES ('XUAT', DATEADD(DAY, -2, GETDATE()), NULL, 0);
INSERT INTO ChiTietPhieuKho VALUES (4, 'VT001', 380, 16500), (4, 'VT002', 40, 92000), (4, 'VT005', 70, 65000), (4, 'VT006', 5, 1450000);

UPDATE p SET TongTien = x.Tong
FROM PhieuKho p JOIN (SELECT MaPhieu, SUM(SoLuong * DonGia) AS Tong FROM ChiTietPhieuKho GROUP BY MaPhieu) x ON p.MaPhieu = x.MaPhieu;

UPDATE v SET SoLuongTon = ISNULL(n.Nhap, 0) - ISNULL(x.Xuat, 0)
FROM VatTu v
LEFT JOIN (SELECT ct.MaVatTu, SUM(ct.SoLuong) AS Nhap FROM ChiTietPhieuKho ct JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
           WHERE p.LoaiPhieu = 'NHAP' GROUP BY ct.MaVatTu) n ON v.MaVatTu = n.MaVatTu
LEFT JOIN (SELECT ct.MaVatTu, SUM(ct.SoLuong) AS Xuat FROM ChiTietPhieuKho ct JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
           WHERE p.LoaiPhieu = 'XUAT' GROUP BY ct.MaVatTu) x ON v.MaVatTu = x.MaVatTu;
GO

SELECT * FROM VatTu;
SELECT * FROM PhieuKho;
