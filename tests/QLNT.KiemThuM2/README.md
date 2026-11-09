# Kiểm thử tích hợp M2–M4

Project console chạy trên Windows, tham chiếu project GUI/BUS/DAO thật. Chạy từ thư mục gốc repo:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj
```

Yêu cầu .NET 10 SDK, SQL Server hoạt động và tài khoản Windows có quyền tạo/xóa database kiểm thử. Mặc định server `localhost`; nếu khác, đặt biến môi trường `QLNT_SQL_SERVER` trước khi chạy.

Chương trình luôn tự đặt `QLNT_DATABASE` thành `QLNT_Test_PN_` + GUID, tạo database đó và xác minh `SELECT DB_NAME()` trước mọi lệnh ghi dữ liệu kiểm thử. Không dùng tên database ứng dụng được truyền vào từ môi trường.

[Program.cs](./Program.cs) tạo schema tối thiểu có khóa/ràng buộc, kiểm tra thêm phiếu, mở form ẩn và dọn database/file tạm trong `finally`. [Cases.cs](./Cases.cs) kiểm tra sửa/xóa, tranh chấp kho, tổng chi M4 và file Excel thực.

## Kết quả chạy đầy đủ ngày 09/11/2026

Đã build và chạy toàn bộ bộ kiểm thử trên bản source hiện tại:

```powershell
dotnet build tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj --no-restore
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj --no-build --no-restore
```

- Build: **0 lỗi, 0 cảnh báo**.
- Chạy kiểm thử: **232/232 xác nhận tự động đạt**, thông báo `PASS ALL: 232 ca`, exit code **0**. Tổng này gồm các ca M2 và các ca tích hợp M3/M4, không phải 232 quy trình M2 riêng biệt.
- Tìm kiếm Thuốc: **36 kiểm tra BUS và 17 kiểm tra giao diện** nằm trong lần chạy đầy đủ; dữ liệu tìm kiếm được đưa vào cache giả rồi khôi phục.
- Phiếu nhập: thêm/sửa/xóa, tính tổng, điều chỉnh tồn theo chênh lệch, giá nhập không ghi đè giá bán, làm mới cache, chặn tồn âm/tràn tồn, phát hiện bản sửa cũ và thao tác đồng thời.
- Rollback: lệnh bắt buộc tác động 0 dòng, lỗi khóa ngoại, trùng chi tiết, không đủ tồn; so sánh dữ liệu phiếu/chi tiết/thuốc trước và sau lỗi.
- Giao diện: chọn thuốc, soạn chi tiết phiếu, chọn phiếu để xem chi tiết, nạp form sửa và bấm Cập nhật/Lưu trên form sửa phiếu thật.
- Excel: tạo/đọc `.xlsx` thật, dữ liệu hợp lệ/không hợp lệ, trùng thuốc, lỗi giữa lô và nhập đồng thời.
- Tích hợp: lưu/sửa/hủy hóa đơn, trừ/hoàn kho, đối chiếu tổng chi từ phiếu nhập với BUS thống kê M4, báo cáo Excel và PDF thật.
- Database thử `QLNT_Test_PN_afdef8ee4a9140e99e928ba0ce187948` đã được xác minh kết nối và xóa sau khi chạy. Không ghi dữ liệu vào database ứng dụng.

Lần này chỉ cập nhật tài liệu kết quả, không cần sửa mã nghiệp vụ. Các ca giao diện chạy tự động trên form/điều khiển WinForms ẩn; chưa thay thế việc người dùng tự thao tác toàn bộ luồng qua menu trên dữ liệu đồ án hiện có. Chưa kiểm thử mất kết nối SQL Server giữa giao dịch hoặc thao tác trên máy khác.

## Tìm kiếm Thuốc nâng cao trong BUS

[ThuocBUS.TimKiemNangCao](../../QLNT.BUS/ThuocBUS.cs) nhận các tham số tùy chọn `maThuoc`, `tenThuoc`, `maLoai`, `giaTu`, `giaDen`, `noi` (mặc định `PhepNoi.And`), trả về `ArrayList`. Mã và tên thuốc tìm theo chuỗi chứa; mã loại khớp chính xác. Chuỗi được trim, không phân biệt hoa/thường; điều kiện để trống được bỏ qua. Giá tìm theo `Thuoc.DonGia` (giá bán).

```csharp
ArrayList ketQua = new ThuocBUS().TimKiemNangCao(
    tenThuoc: "Para", giaTu: 5000m, giaDen: 20000m, noi: PhepNoi.And);
```

Khoảng giá tính cả hai đầu; có thể chỉ nhập một đầu. Khi chọn OR, khoảng giá vẫn là một nhóm `giá >= giaTu AND giá <= giaDen`: ví dụ tên chứa Para **HOẶC** giá trong khoảng. Giá âm, khoảng đảo ngược hoặc phép nối không hợp lệ gây `ArgumentException` (gồm `ArgumentOutOfRangeException`); form gọi hàm cần bắt lỗi và hiển thị thông báo.

Danh sách thuốc và kết quả dùng `ArrayList`, lọc trong bộ nhớ bằng bộ lọc M1. `List<DieuKien>` chỉ là danh sách tiêu chí theo API dùng chung, không chứa các bản ghi thuốc. Lần đầu đọc có thể nạp cache từ DAO như trước; việc lọc không tạo câu SQL tìm kiếm. Kết quả là danh sách riêng, giữ thứ tự gốc, không nhân đôi thuốc khi OR.

Chạy riêng kiểm tra tính năng này, không kết nối SQL:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj -- --thuoc-filter-only
```

[ThuocFilterCases.cs](./ThuocFilterCases.cs) dùng dữ liệu thuốc giả trong cache, khôi phục cache cũ sau thử. Kết quả ngày 08/11/2026: **36/36 kiểm tra đạt**, exit code 0; build project kiểm thử và các project tham chiếu 0 lỗi, 0 cảnh báo. Gồm nhiều trường AND/OR, nhóm khoảng giá khi OR, biên/giá lẻ, Unicode, ô trống, dữ liệu không hợp lệ, kết quả rỗng, không trùng và không thay đổi cache gốc. Đã chạy lại và đạt trong toàn bộ bộ kiểm thử tích hợp ngày 09/11/2026 như phần kết quả ở trên.

## Bộ lọc trên màn hình Thuốc

[frmThuoc](../../QLNT.GUI/frmThuoc.cs) có vùng tìm kiếm riêng: mã thuốc, tên thuốc, loại, giá từ/đến và AND/OR. Chọn **Tất cả loại** để bỏ điều kiện loại. Đánh dấu **Giá từ** hoặc **Giá đến** để dùng giới hạn tương ứng; ô giá không đánh dấu được bỏ qua, kể cả đang có số.

- **Tìm** hoặc Enter trong vùng lọc: gửi các điều kiện cho BUS rồi hiển thị `ArrayList` kết quả và số lượng.
- **Xóa bộ lọc**: xóa tiêu chí, đưa AND/OR về AND, bỏ các giới hạn giá và hiển thị lại toàn bộ thuốc; không xóa dữ liệu danh mục.
- **Tải lại**: nạp lại cache thuốc/loại từ DAO và áp dụng lại bộ lọc hiện tại.
- Khoảng giá nhập ngược: hiển thị lỗi BUS ngay trên vùng lọc, giữ kết quả trước đó; sửa hợp lệ rồi Tìm để áp dụng.

Ví dụ thử: tên `Para`, đánh dấu giá từ `5000` và giá đến `20000`, chọn AND rồi Tìm. Kết quả phải vừa chứa Para trong tên vừa nằm trong khoảng giá. Chuyển OR sẽ lấy thuốc khớp tên **hoặc** nằm trong toàn bộ khoảng giá.

Kiểm tra giao diện bằng điều khiển thật và cache giả, không kết nối SQL:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj -- --thuoc-filter-ui-only
```

[ThuocFilterUiCases.cs](./ThuocFilterUiCases.cs) đã đạt **17/17 kiểm tra**, exit code 0, ngày 08/11/2026: nút Tìm, AND/OR, loại, khoảng giá tùy chọn, giá 0, Enter, xóa bộ lọc, dữ liệu không hợp lệ, không đổi cache gốc và bố cục ở kích thước tối thiểu. Đã xem ảnh giao diện rộng 1100 và 850 pixel. Build project GUI/kiểm thử cùng các project tham chiếu: **0 lỗi, 0 cảnh báo**. Các kiểm tra này không thao tác các nút ghi dữ liệu và chưa thay thế kiểm thử toàn luồng trên database thật.

## Kết quả tích hợp đã có trước khi bổ sung lọc Thuốc

Phần hóa đơn tạo dữ liệu khách hàng, khuyến mãi sản phẩm, mốc giảm và voucher trong database riêng; kiểm tra tạo/sửa/hủy, đối soát `HOADON` với `CTHD`, số tiền giảm và tồn kho. Phần này cũng xuất PDF thật qua máy in Windows **Microsoft Print to PDF** và xác nhận chữ ký tệp. Không ghi hóa đơn vào database ứng dụng.

Kiểm thử hóa đơn và trang Khuyến mãi bao gồm đối soát CTHD/HOADON/tồn kho, xuất PDF thật, và hiển thị/lọc thể loại khuyến mãi. Lần chạy ngày 08/11/2026: **161 kiểm tra đạt**, exit code 0; database kiểm thử riêng được xác minh rồi xóa. Các ca mới đối chiếu thống kê tháng/quý/năm, doanh thu theo nhân viên/khách hàng/sản phẩm, xuất báo cáo `.xlsx`, chặn cột tổng sản phẩm lấy nhầm hóa đơn ngoài năm chọn và bấm nút sửa/lưu trên form Phiếu nhập, Hóa đơn. Mỗi dòng `PASS` là một xác nhận tự động; không có nghĩa đã thử thủ công 161 quy trình giao diện khác nhau.

Kiểm tra riêng thao tác bấm **Sửa → Lưu** trên hai form với database thử:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj -- --ui-edit-only
```

Sau khi đổi tên và trạng thái nút soạn phiếu, đã chạy riêng **14/14 kiểm tra bản nháp**, exit code 0. Chạy riêng phần này không kết nối SQL:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj -- --draft-only
```

[DraftCases.cs](./DraftCases.cs) bấm nút thật trên điều khiển WinForms ẩn để thử thêm, cập nhật, bỏ thuốc, nhập mới, tổng tiền và dữ liệu được giữ lại. Kết quả hộp thoại chọn thuốc được thay bằng đối tượng giả; không chạy Load/Lưu phiếu. Bộ chạy đầy đủ hiện bao gồm cả 14 kiểm tra này và đã được chạy lại sau cập nhật UX.

Chỉ kiểm tra lỗi chọn/đổi thuốc, không kết nối SQL:

```powershell
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj -- --picker-only
```

[PickerCases.cs](./PickerCases.cs) dùng điều khiển thật và sự kiện WinForms thật trên cửa sổ ẩn, đưa dữ liệu thuốc giả vào danh sách. Chạy 16 kiểm tra cho nhập hàng/bán hàng, gồm đổi thuốc, bỏ chọn, lượng tối đa và các giới hạn tồn. Đã tái hiện lỗi `Value=0` trong khi `Minimum=1` trên bản trước sửa; bản sau sửa đạt.

Các tình huống lỗi so sánh dữ liệu đầu phiếu, chi tiết và thuốc trước/sau để kiểm tra rollback. Các ca gọi trực tiếp DAO cố ý bỏ qua kiểm tra BUS để gây lỗi ở giữa giao dịch.

Nếu cưỡng bức tắt tiến trình hoặc SQL Server bị ngắt trước bước dọn, database tạm có thể còn. Chương trình in đúng tên ngay lúc bắt đầu để nhận diện; không tự xóa database của lần chạy khác.

Phạm vi chưa thử và hướng dẫn bàn giao: [Bàn giao M2](../../BAN_GIAO_PHIEU_NHAP_M3_M4.md).
