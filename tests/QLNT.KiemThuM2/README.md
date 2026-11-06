# Kiểm thử M2

Project console chạy trên Windows, tham chiếu project GUI/BUS/DAO thật. Chạy từ thư mục gốc repo:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj
```

Yêu cầu .NET 10 SDK, SQL Server hoạt động và tài khoản Windows có quyền tạo/xóa database kiểm thử. Mặc định server `localhost`; nếu khác, đặt biến môi trường `QLNT_SQL_SERVER` trước khi chạy.

Chương trình luôn tự đặt `QLNT_DATABASE` thành `QLNT_Test_PN_` + GUID, tạo database đó và xác minh `SELECT DB_NAME()` trước mọi lệnh ghi dữ liệu kiểm thử. Không dùng tên database ứng dụng được truyền vào từ môi trường.

[Program.cs](./Program.cs) tạo schema tối thiểu có khóa/ràng buộc, kiểm tra thêm phiếu, mở form ẩn và dọn database/file tạm trong `finally`. [Cases.cs](./Cases.cs) kiểm tra sửa/xóa, tranh chấp kho, tổng chi M4 và file Excel thực.

Phần hóa đơn tạo dữ liệu khách hàng, khuyến mãi sản phẩm, mốc giảm và voucher trong database riêng; kiểm tra tạo/sửa/hủy, đối soát `HOADON` với `CTHD`, số tiền giảm và tồn kho. Phần này cũng xuất PDF thật qua máy in Windows **Microsoft Print to PDF** và xác nhận chữ ký tệp. Không ghi hóa đơn vào database ứng dụng.

Lần chạy ngày 06/11/2026: **149 kiểm tra đạt**, exit code 0; database kiểm thử riêng được xác minh rồi xóa. Trong đó có kiểm thử PDF thật qua Microsoft Print to PDF, loại CTKM trên hóa đơn và cột/lọc thể loại CTKM trên trang Khuyến mãi. Mỗi dòng `PASS` là một xác nhận tự động; không có nghĩa đã thử thủ công 149 quy trình giao diện khác nhau.

Sau khi đổi tên và trạng thái nút soạn phiếu, đã chạy riêng **14/14 kiểm tra bản nháp**, exit code 0. Chạy riêng phần này không kết nối SQL:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj -- --draft-only
```

[DraftCases.cs](./DraftCases.cs) bấm nút thật trên điều khiển WinForms ẩn để thử thêm, cập nhật, bỏ thuốc, nhập mới, tổng tiền và dữ liệu được giữ lại. Kết quả hộp thoại chọn thuốc được thay bằng đối tượng giả; không chạy Load/Lưu phiếu. Bộ chạy đầy đủ hiện bao gồm cả 14 kiểm tra này; chưa chạy lại toàn bộ sau cập nhật UX.

Chỉ kiểm tra lỗi chọn/đổi thuốc, không kết nối SQL:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj -- --picker-only
```

[PickerCases.cs](./PickerCases.cs) dùng điều khiển thật và sự kiện WinForms thật trên cửa sổ ẩn, đưa dữ liệu thuốc giả vào danh sách. Chạy 16 kiểm tra cho nhập hàng/bán hàng, gồm đổi thuốc, bỏ chọn, lượng tối đa và các giới hạn tồn. Đã tái hiện lỗi `Value=0` trong khi `Minimum=1` trên bản trước sửa; bản sau sửa đạt.

Các tình huống lỗi so sánh dữ liệu đầu phiếu, chi tiết và thuốc trước/sau để kiểm tra rollback. Các ca gọi trực tiếp DAO cố ý bỏ qua kiểm tra BUS để gây lỗi ở giữa giao dịch.

Nếu cưỡng bức tắt tiến trình hoặc SQL Server bị ngắt trước bước dọn, database tạm có thể còn. Chương trình in đúng tên ngay lúc bắt đầu để nhận diện; không tự xóa database của lần chạy khác.

Phạm vi chưa thử và hướng dẫn bàn giao: [Bàn giao M2](../../BAN_GIAO_PHIEU_NHAP_M3_M4.md).
