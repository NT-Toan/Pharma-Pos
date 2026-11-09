using System.Collections;
using System.Reflection;
using QLNT.GUI;

internal static partial class Program
{
    private static void KiemTraGiaoDienLocThuoc()
    {
        var cacheThuoc = typeof(ThuocBUS).GetField("danhSachThuoc", BindingFlags.Static | BindingFlags.NonPublic)!;
        var cacheLoai = typeof(LoaiThuocBUS).GetField("danhSachLoaiThuoc", BindingFlags.Static | BindingFlags.NonPublic)!;
        object? thuocGoc = cacheThuoc.GetValue(null), loaiGoc = cacheLoai.GetValue(null);
        try
        {
            var cacThuoc = new ArrayList
            {
                new Thuoc { MaThuoc = "TH0001", TenThuoc = "Paracetamol 500mg", MaLoai = "LT0001", DonGia = 5000 },
                new Thuoc { MaThuoc = "TH0002", TenThuoc = "Paracetamol 650mg", MaLoai = "LT0001", DonGia = 20000 },
                new Thuoc { MaThuoc = "TH0003", TenThuoc = "Ibuprofen", MaLoai = "LT0002", DonGia = 10000 },
                new Thuoc { MaThuoc = "TH0004", TenThuoc = "Vitamin C", MaLoai = "LT0003", DonGia = 30000 },
                new Thuoc { MaThuoc = "TH0005", TenThuoc = "Para Extra", MaLoai = "LT0002", DonGia = 25000 },
                new Thuoc { MaThuoc = "TH0006", TenThuoc = "Amoxicillin", MaLoai = "LT0002", DonGia = 1000 }
            };
            cacheThuoc.SetValue(null, cacThuoc);
            cacheLoai.SetValue(null, new ArrayList
            {
                new LoaiThuoc("LT0001", "Giảm đau"), new LoaiThuoc("LT0002", "Kháng sinh"), new LoaiThuoc("LT0003", "Vitamin")
            });
            using var form = new frmThuoc();
            using var host = new Form { Opacity = 0, ShowInTaskbar = false, Size = form.Size, MinimumSize = form.MinimumSize };
            // Dùng điều khiển thật nhưng không mở form gốc để Load không xóa cache giả và đọc SQL.
            foreach (Control control in form.Controls.Cast<Control>().ToArray()) host.Controls.Add(control);
            typeof(frmThuoc).GetMethod("NapLoaiThuoc", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null);
            host.Show(); Application.DoEvents();
            T O<T>(string ten) where T : Control => (T)host.Controls.Find(ten, true).Single();
            var ma = O<TextBox>("txtLocMaThuoc");
            var ten = O<TextBox>("txtLocTenThuoc");
            var loai = O<ComboBox>("cboLocLoaiThuoc");
            var noi = O<ComboBox>("cboLocPhepNoi");
            var coGiaTu = O<CheckBox>("chkLocGiaTu");
            var coGiaDen = O<CheckBox>("chkLocGiaDen");
            var giaTu = O<NumericUpDown>("nudLocGiaTu");
            var giaDen = O<NumericUpDown>("nudLocGiaDen");
            var tim = O<Button>("btnTimKiemThuoc");
            var xoaLoc = O<Button>("btnXoaBoLocThuoc");
            var loi = O<Label>("lblLoiLocThuoc");
            var soLuong = O<Label>("lblSoLuongThuoc");
            var danhSach = O<ListView>("lvThuoc");
            string[] CacMa() => danhSach.Items.Cast<ListViewItem>().Select(d => d.Text).ToArray();
            void Tim() { tim.PerformClick(); Application.DoEvents(); }
            void XoaLoc() { xoaLoc.PerformClick(); Application.DoEvents(); }
            void ChonLoai(string code) => loai.SelectedItem = loai.Items.Cast<LoaiThuoc>().Single(l => l.MaLoai == code);
            void ChupAnh(string file)
            {
                string? thuMuc = Environment.GetEnvironmentVariable("QLNT_UI_CAPTURE_DIR");
                if (string.IsNullOrWhiteSpace(thuMuc)) return;
                Directory.CreateDirectory(thuMuc);
                using var bitmap = new Bitmap(host.Width, host.Height);
                host.DrawToBitmap(bitmap, new Rectangle(Point.Empty, host.Size));
                bitmap.Save(Path.Combine(thuMuc, file), System.Drawing.Imaging.ImageFormat.Png);
            }

            Tim();
            Check(danhSach.Items.Count == 6 && soLuong.Text.Contains("6") && loai.SelectedIndex == 0 && noi.SelectedIndex == 0,
                "Form Thuoc mo bo loc rong hien tat ca va mac dinh AND");
            Check(!giaTu.Enabled && !giaDen.Enabled, "Form Thuoc gia chua danh dau khong ap dung");
            ten.Text = "Para"; coGiaTu.Checked = true; giaTu.Value = 5000; coGiaDen.Checked = true; giaDen.Value = 20000;
            Check(danhSach.Items.Count == 6 && giaTu.Enabled && giaDen.Enabled, "Nhap dieu kien cho bam Tim moi ap dung");
            Tim();
            Check(CacMa().SequenceEqual(["TH0001", "TH0002"]) && soLuong.Text.Contains("2"), "Form gui ten va khoang gia AND xuong BUS");
            ChupAnh("thuoc-filter-1100.png");
            noi.SelectedIndex = 1; Tim();
            Check(CacMa().SequenceEqual(["TH0001", "TH0002", "TH0003", "TH0005"]), "Form gui OR va giu dung nhom khoang gia");
            XoaLoc(); ma.Text = "TH0004"; Tim();
            Check(CacMa().SequenceEqual(["TH0004"]), "Form loc rieng ma thuoc");
            XoaLoc(); ChonLoai("LT0002"); Tim();
            Check(CacMa().SequenceEqual(["TH0003", "TH0005", "TH0006"]), "Form chon loai bang ten nhung gui dung ma loai");
            ten.Text = "KhongCo"; Tim();
            Check(danhSach.Items.Count == 0 && soLuong.Text.Contains("0"), "Form khong co ket qua hien danh sach rong");
            XoaLoc();
            Check(danhSach.Items.Count == 6 && ma.Text == "" && ten.Text == "" && loai.SelectedIndex == 0 && noi.SelectedIndex == 0 &&
                !coGiaTu.Checked && !coGiaDen.Checked && giaTu.Value == 0 && giaDen.Value == 0 && !giaTu.Enabled && !giaDen.Enabled,
                "Xoa bo loc reset cac dieu kien va hien lai toan bo");
            coGiaTu.Checked = true; coGiaDen.Checked = true; giaTu.Value = 20000; giaDen.Value = 5000; Tim();
            Check(loi.Text.Contains("Giá từ") && danhSach.Items.Count == 6, "Khoang gia dao nguoc hien loi BUS va khong thay ket qua cu");
            giaDen.Value = 30000; Tim();
            Check(loi.Text == "" && CacMa().SequenceEqual(["TH0002", "TH0004", "TH0005"]), "Sua khoang gia hop le xoa thong bao loi");
            XoaLoc(); coGiaDen.Checked = true; giaDen.Value = 5000; Tim();
            Check(CacMa().SequenceEqual(["TH0001", "TH0006"]), "Form cho phep chi loc gia den");
            coGiaDen.Checked = false; Tim();
            Check(danhSach.Items.Count == 6 && !giaDen.Enabled && giaDen.Value == 5000,
                "Bo danh dau gia bo qua gia tri cu trong o");
            coGiaDen.Checked = true; giaDen.Value = 0; Tim();
            Check(danhSach.Items.Count == 0, "Gia den 0 duoc gui xuong BUS khac voi khong loc");
            XoaLoc(); ma.Text = "TH0003";
            var enter = new KeyEventArgs(Keys.Enter);
            typeof(Control).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(ma, [enter]);
            Application.DoEvents();
            Check(enter.SuppressKeyPress && CacMa().SequenceEqual(["TH0003"]), "Enter trong o loc cung chay Tim");
            XoaLoc();
            Check(cacThuoc.Count == 6 && ((Thuoc)cacThuoc[0]!).DonGia == 5000 &&
                ReferenceEquals(cacheThuoc.GetValue(null), cacThuoc), "Tim va xoa bo loc khong xoa thuoc hoac doi cache");
            host.Size = host.MinimumSize; host.PerformLayout(); Application.DoEvents();
            var nhom = O<GroupBox>("grpLocThuoc");
            Check(nhom.Bottom <= danhSach.Top && danhSach.Height > 100 &&
                new Control[] { ma, ten, loai, noi, giaTu, giaDen, tim, xoaLoc }.All(c =>
                    c.Width > 0 && c.Height > 0 && c.Parent!.ClientRectangle.Contains(c.Bounds)),
                "Bo loc va bang khong chong len nhau tai kich thuoc nho nhat");
            ChupAnh("thuoc-filter-850.png");
            host.Close();
        }
        finally
        {
            cacheThuoc.SetValue(null, thuocGoc);
            cacheLoai.SetValue(null, loaiGoc);
        }
    }
}
