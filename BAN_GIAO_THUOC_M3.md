# Bàn giao hàm Thuốc từ M2 cho M3

## 1. Mục đích và phạm vi

M2 cung cấp phần lấy thông tin thuốc và kiểm tra điều kiện bán. M3 gọi các hàm này khi xây dựng Hóa đơn.

Tài liệu này mô tả code hiện có và bằng chứng kiểm thử trong phiên làm việc. **Có code hoặc build thành công chưa có nghĩa là nghiệp vụ đã được kiểm thử trên SQL Server.**

## 2. File liên quan

| File | Vai trò |
| --- | --- |
| [ThuocBUS.cs](./QLNT.BUS/ThuocBUS.cs) | Lớp `ThuocBUS`: lấy thuốc, kiểm tra bán, giữ danh sách trong `ArrayList`. |
| [Thuoc.cs](./QLNT.DAO/Thuoc.cs) | Lớp dữ liệu `Thuoc`: mã, tên, hạn sử dụng, đơn giá, tồn kho, mã loại. |
| [ThuocDAO.cs](./QLNT.DAO/ThuocDAO.cs) | Đọc/ghi bảng `THUOC` thông qua `MyConnection`. |
| [MyConnection.cs](./QLNT.DAO/MyConnection.cs) | Kết nối SQL Server và chạy truy vấn. |
| [KetQua.cs](./QLNT.BUS/KetQua.cs) | Kết quả kiểm tra gồm `ThanhCong` và `ThongBao`. |

M3 cần lấy phần code đã ghép vào solution của nhóm để có đủ các lớp phụ thuộc; không chỉ sao chép riêng hai phương thức. GUI của M3 gọi BUS.

## 3. Tên hàm và cách gọi

| Hàm | Đầu vào | Kết quả trả về |
| --- | --- | --- |
| `Thuoc? LayTheoMa(string maThuoc)` | Mã thuốc, ví dụ `"TH0001"`. | Một đối tượng `Thuoc`; trả `null` nếu mã trống hoặc không tìm thấy. |
| `KetQua KiemTraBan(string maThuoc, int soLuong, DateTime ngayBan)` | Mã thuốc, số lượng muốn bán và ngày bán. | `ThanhCong = true` nếu đạt; ngược lại `ThanhCong = false` và `ThongBao` nêu lỗi. |
| `static void TaiLai()` | Không có. | Xóa cache; lần đọc tiếp theo mới lấy danh sách từ database. |

### 3.1. Lấy thông tin thuốc

```csharp
var thuocBUS = new ThuocBUS();
Thuoc? thuoc = thuocBUS.LayTheoMa("TH0001");
```

Nếu `thuoc` khác `null`, M3 có thể đọc:

- `thuoc.MaThuoc`, `thuoc.TenThuoc`: mã và tên thuốc.
- `thuoc.DonGia`: đơn giá hiện tại, kiểu `decimal`.
- `thuoc.HSD`: hạn sử dụng, kiểu `DateTime?`; có thể không có giá trị.
- `thuoc.SoLuong`: số lượng tồn kho hiện tại trong danh sách đã nạp.
- `thuoc.MaLoai`: mã loại thuốc.

Hàm tìm trên `ArrayList`, bỏ khoảng trắng ở hai đầu mã và không phân biệt chữ hoa/thường. Lần đầu chưa có cache, BUS gọi `ThuocDAO.LayTatCa()` để đọc database.

### 3.2. Kiểm tra hạn dùng và tồn kho

Cả hai việc nằm trong **một hàm `KiemTraBan`**. Thứ tự kiểm tra hiện tại:

| Điều kiện | Kết quả theo code |
| --- | --- |
| Số lượng bán nhỏ hơn hoặc bằng 0 | Lỗi: `Số lượng bán phải lớn hơn 0.` |
| Không tìm thấy thuốc | Lỗi: `Không tìm thấy thuốc.` |
| Có HSD và HSD trước ngày bán | Lỗi: `Thuốc đã hết hạn.` |
| Tồn kho nhỏ hơn số lượng muốn bán | Lỗi: `Thuốc không đủ tồn kho.` |
| Đạt tất cả điều kiện | `ThanhCong = true`, `ThongBao` là chuỗi rỗng. |

Code chỉ so sánh phần ngày. **Ngày bán bằng HSD vẫn được phép qua bước hạn dùng. HSD là `null` thì bỏ qua bước kiểm tra hết hạn.** Đây là hành vi của code hiện tại, cần cả nhóm xác nhận phù hợp nghiệp vụ.

### 3.3. Ví dụ gọi trong một hàm xử lý sự kiện của GUI

Đây là ví dụ sử dụng, chưa phải bằng chứng đã chạy thành công. M3 thay mã, số lượng, ngày bán bằng dữ liệu trên form Hóa đơn.

```csharp
try
{
    var thuocBUS = new ThuocBUS();
    string maThuoc = "TH0001";
    int soLuongBan = 2;
    DateTime ngayBan = DateTime.Today;

    Thuoc? thuoc = thuocBUS.LayTheoMa(maThuoc);
    if (thuoc == null)
    {
        MessageBox.Show("Không tìm thấy thuốc.");
        return;
    }

    KetQua ketQua = thuocBUS.KiemTraBan(maThuoc, soLuongBan, ngayBan);
    if (!ketQua.ThanhCong)
    {
        MessageBox.Show(ketQua.ThongBao);
        return;
    }

    // M3 dùng thuoc.TenThuoc và thuoc.DonGia để điền dòng hóa đơn.
    // Bước này chưa lưu hóa đơn và chưa trừ tồn kho.
}
catch (Exception)
{
    MessageBox.Show("Không đọc được thông tin thuốc. Hãy kiểm tra kết nối database.");
}
```

## 4. Lưu ý khi M3 ghép vào Hóa đơn

- Hai hàm trên chỉ đọc và kiểm tra; không lưu hóa đơn, không trừ kho, không giữ chỗ số lượng.
- Dữ liệu được giữ trong cache. Sau khi giao dịch hóa đơn/phiếu nhập làm thay đổi tồn kho thành công, module thực hiện giao dịch gọi `ThuocBUS.TaiLai()`. Thay đổi từ chương trình khác cũng cần tải lại để tránh đọc cache cũ.
- Nếu một mã thuốc xuất hiện nhiều dòng trong hóa đơn, cần kiểm tra tổng số lượng của mã đó. Hàm không tự cộng các dòng hóa đơn.
- M3 phải bảo đảm thao tác ghi hóa đơn, chi tiết và tồn kho nằm trong cùng giao dịch; kết quả kiểm tra trước đó không bảo đảm tồn kho vẫn còn khi một người khác đồng thời bán hàng.
- Không sửa `SoLuong` của đối tượng nhận từ BUS để thay cho cập nhật SQL: đối tượng hiện tại thuộc danh sách cache, sửa nó không ghi vào database.
- Lỗi kết nối hoặc đọc database có thể phát sinh ngoại lệ từ `LayTheoMa`/`KiemTraBan`; hai hàm này không chuyển mọi ngoại lệ thành `KetQua.Loi`. GUI cần xử lý như ví dụ trên.

## 5. Những kiểm tra đã thực hiện

| Kiểm tra | Kết quả đã ghi nhận | Giới hạn |
| --- | --- | --- |
| `dotnet build QLNT.slnx --no-restore` | Thành công; 0 lỗi, 0 cảnh báo trong phiên triển khai. | Chứng minh code biên dịch được; không chứng minh dữ liệu hay nghiệp vụ chạy đúng. |
| Đọc source `ThuocBUS.LayTheoMa` và `ThuocBUS.KiemTraBan` | Đã đối chiếu chữ ký, điều kiện, thứ tự và thông báo trong tài liệu với source. | Là kiểm tra source, không phải thực thi các tình huống bên dưới. |
| Thử đọc số dòng bảng bằng `sqlcmd` | Chưa truy vấn được; công cụ báo lỗi ODBC/TLS/kết nối. | Chưa xác nhận dữ liệu SQL Server qua công cụ này; chưa kết luận kết nối của ứng dụng bị lỗi tương tự. |

Người dùng đã phản hồi chung rằng thao tác đã được, nhưng chưa cung cấp kết quả riêng cho từng tình huống. Vì vậy, tài liệu **chưa đánh dấu các ca nghiệp vụ dưới đây là đã đạt**.

## 6. Những trường hợp chưa có kết quả kiểm thử cụ thể

Các kết quả mong đợi bên dưới được suy ra từ source hiện tại. Dùng thuốc thử hoặc dữ liệu đã xác nhận để kiểm tra; không coi mã ví dụ là chắc chắn tồn tại trong database.

| Mã ca | Hàm/tình huống | Kết quả mong đợi theo code | Trạng thái |
| --- | --- | --- | --- |
| LT01 | `LayTheoMa`: mã đang tồn tại | Trả đúng thông tin thuốc, khớp SQL Server. | Chưa xác nhận từng ca. |
| LT02 | `LayTheoMa`: mã không tồn tại | Trả `null`. | Chưa xác nhận từng ca. |
| LT03 | `LayTheoMa`: mã rỗng hoặc toàn khoảng trắng | Trả `null`. | Chưa xác nhận từng ca. |
| LT04 | `LayTheoMa`: khác hoa/thường, có khoảng trắng hai đầu | Tìm được cùng thuốc nếu mã tồn tại. | Chưa xác nhận từng ca. |
| KB01 | `KiemTraBan`: số lượng bằng 0 hoặc âm | Báo số lượng phải lớn hơn 0. | Chưa xác nhận từng ca. |
| KB02 | `KiemTraBan`: số lượng dương, mã không tồn tại | Báo không tìm thấy thuốc. | Chưa xác nhận từng ca. |
| KB03 | Thuốc đủ tồn nhưng HSD trước ngày bán | Báo thuốc đã hết hạn. | Chưa xác nhận từng ca. |
| KB04 | HSD bằng ngày bán, đủ tồn | Thành công. | Chưa xác nhận từng ca. |
| KB05 | HSD sau ngày bán, đủ tồn | Thành công. | Chưa xác nhận từng ca. |
| KB06 | HSD là `null`, đủ tồn | Thành công theo quy tắc hiện tại. | Chưa xác nhận từng ca. |
| KB07 | Chưa hết hạn; số lượng bán nhỏ hơn tồn | Thành công. | Chưa xác nhận từng ca. |
| KB08 | Chưa hết hạn; số lượng bán bằng tồn | Thành công. | Chưa xác nhận từng ca. |
| KB09 | Chưa hết hạn; số lượng bán lớn hơn tồn | Báo không đủ tồn kho. | Chưa xác nhận từng ca. |
| KB10 | Tồn bằng 0; bán 1 đơn vị, chưa hết hạn | Báo không đủ tồn kho. | Chưa xác nhận từng ca. |
| KB11 | Vừa hết hạn vừa thiếu tồn | Báo hết hạn trước vì thứ tự kiểm tra. | Chưa xác nhận từng ca. |
| CACHE01 | Tồn trong SQL đã đổi, gọi `TaiLai()` rồi lấy/kiểm tra lại | Kết quả dùng số lượng mới trong SQL. | Chưa xác nhận từng ca. |
| DB01 | Không có cache, mất kết nối SQL khi đọc thuốc | GUI nhận và xử lý ngoại lệ, không báo thành công. | Chưa xác nhận từng ca. |

Khi chạy thử, ghi lại mã thuốc, số lượng, ngày bán, HSD, tồn thực tế, kết quả nhận được và người thực hiện. Chỉ chuyển trạng thái sang “Đã đạt” khi đã có kết quả thực tế phù hợp.
