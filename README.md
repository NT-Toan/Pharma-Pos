# Pharma-Pos

Dự án quản lý nhà thuốc theo kiến trúc 3 lớp (GUI → BUS → DAO), sử dụng SQL Server.

## Khởi tạo database

1. Trong SQL Server Management Studio, tạo database tên `QLNhaThuoc` nếu chưa có.
2. Chọn database `QLNhaThuoc` làm database hiện hành rồi chạy `D:\QLNT_v2.sql`. File này tạo bảng và thêm 5 dòng `THAMSO`; nó không tạo database và không nên chạy lặp lại.
3. Chạy `D:\QLNT_data.sql` **một lần** sau bước 2 để nạp dữ liệu mẫu. Không chạy lại trên database đã có dữ liệu vì các khóa chính sẽ bị trùng.
4. Mặc định ứng dụng kết nối `localhost` bằng Windows Authentication. Nếu SQL Server dùng instance khác, đặt biến môi trường trong PowerShell trước khi chạy, ví dụ:

   ```powershell
   $env:QLNT_SQL_SERVER = ".\SQLEXPRESS"
   $env:QLNT_DATABASE = "QLNhaThuoc"
   ```

## Chạy giao diện

```powershell
dotnet run --project .\QLNT.GUI\QLNT.GUI.csproj
```

Nếu thiếu database hoặc quyền truy cập, ứng dụng sẽ báo tên server/database. Tạo database và chạy hai file SQL theo thứ tự trên trước khi khởi động lại ứng dụng.

## M3 - Bán hàng

Màn hình **Bán hàng** hỗ trợ lập/sửa hóa đơn, tính khuyến mãi, kiểm tra và trừ tồn kho nguyên tử, hủy hóa đơn kèm hoàn tồn, tìm kiếm, in hóa đơn nháp/đã lưu và xuất PDF qua máy in Microsoft Print to PDF. Trong cửa sổ chọn khách hàng có nút **Thêm khách hàng mới**; khách vừa tạo sẽ được chọn ngay cho hóa đơn. Hủy hóa đơn hiện xóa dữ liệu hóa đơn và hoàn kho vì schema chưa có trạng thái hủy. Tiền khách đưa/tiền thừa chỉ được đối chiếu tại form, chưa lưu vào database do schema chưa có trường thanh toán.
