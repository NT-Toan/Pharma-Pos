# Pharma-Pos

Dự án quản lý nhà thuốc theo kiến trúc 3 lớp (GUI → BUS → DAO), sử dụng SQL Server.

## Khởi tạo database

1. Trong SQL Server Management Studio, tạo database tên `QLNhaThuoc` nếu chưa có.
2. Chọn database `QLNhaThuoc` làm database hiện hành rồi chạy [`Database/QLNT_v2.sql`](Database/QLNT_v2.sql). File này tạo bảng và thêm 5 dòng `THAMSO`; nó không tạo database và không nên chạy lặp lại.
3. Chạy [`Database/QLNT_data.sql`](Database/QLNT_data.sql) **một lần** sau bước 2 để nạp dữ liệu mẫu. Không chạy lại trên database đã có dữ liệu vì các khóa chính sẽ bị trùng.
4. Chạy [`Database/QLNT_dat_thuoc.sql`](Database/QLNT_dat_thuoc.sql) để thêm bảng đơn đặt web và các cột thanh toán/khuyến mãi cho database cũ. Script này có thể chạy lại. Database mới tạo từ `QLNT_v2.sql` vẫn cần bước này để dùng web đặt thuốc.
5. Mặc định ứng dụng kết nối `localhost` bằng Windows Authentication. Nếu SQL Server dùng instance khác, đặt biến môi trường trong PowerShell trước khi chạy, ví dụ:

   ```powershell
   $env:QLNT_SQL_SERVER = ".\SQLEXPRESS"
   $env:QLNT_DATABASE = "QLNhaThuoc"
   ```

## Chạy giao diện

```powershell
dotnet run --project .\QLNT.GUI\QLNT.GUI.csproj
```

Nếu thiếu database hoặc quyền truy cập, ứng dụng sẽ báo tên server/database. Tạo database và chạy các file SQL theo thứ tự trên trước khi khởi động lại ứng dụng.

## Excel

Ứng dụng nhập danh mục thuốc và xuất thống kê bằng `.xlsx`. File mẫu nhập thuốc cũng ở định dạng `.xlsx`.

## M3 - Bán hàng

Màn hình **Bán hàng** hỗ trợ lập/sửa hóa đơn, tính khuyến mãi, kiểm tra và trừ tồn kho nguyên tử, hủy hóa đơn kèm hoàn tồn, tìm kiếm và in hóa đơn nháp/đã lưu. Bản nháp ghi rõ loại và phần trăm CTKM trên từng dòng thuốc (`KMSanPham`) và nguồn/mức giảm toàn hóa đơn (`KMHoaDon` hoặc `KMVoucher`). Nút **Xuất PDF** mở hộp thoại chọn nơi lưu và tạo tệp trực tiếp bằng máy in Windows **Microsoft Print to PDF**; nội dung PDF phân trang và hiển thị tiền giảm hóa đơn cùng tổng thanh toán. Trong cửa sổ chọn khách hàng có nút **Thêm khách hàng mới**; khách vừa tạo sẽ được chọn ngay cho hóa đơn. Hủy hóa đơn hiện xóa dữ liệu hóa đơn và hoàn kho vì schema chưa có trạng thái hủy. Tiền khách đưa được lưu trong `HOADON`; tiền thừa được tính từ số tiền đã đưa và tổng thanh toán.

## Web đặt thuốc

```powershell
dotnet run --project .\QLNT.Web\QLNT.Web.csproj
```

Web dùng chung SQL Server với WinForms. Khách có thể tìm thuốc, thêm vào giỏ, xem giảm giá sản phẩm và mức giảm hóa đơn có lợi hơn giữa voucher chung với khuyến mãi theo mốc, rồi xác nhận đơn. Đơn được giữ thuốc trong kho ngay khi lưu thành công. Trong WinForms, vào **Bán hàng → Đơn đặt từ web** để tạo hóa đơn từ đơn này; tại quầy nhập tiền khách đưa, online cần đối chiếu thủ công, COD ghi nhận thu sau khi giao. Xem thêm [`QLNT.Web/README.md`](QLNT.Web/README.md).
