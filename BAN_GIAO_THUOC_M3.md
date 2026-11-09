# Bàn giao hàm Thuốc từ M2 cho M3

Ngày cập nhật: **09/11/2026**. Bàn giao tổng hợp Phiếu nhập, tồn kho, Excel và kết quả kiểm thử: [Bàn giao M2 cho M3/M4](./BAN_GIAO_PHIEU_NHAP_M3_M4.md).

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
| `ArrayList LayTatCa()` | Không có. | Danh sách thuốc trong cache, trả bản sao danh sách; đối tượng thuốc bên trong vẫn dùng chung. |
| `ArrayList TimKiemNangCao(...)` | Mã, tên, mã loại, giá từ/đến, AND/OR. | Kết quả lọc trên `ArrayList`; cách gọi xem mục 3.4. |

### 3.1. Lấy thông tin thuốc

```csharp
var thuocBUS = new ThuocBUS();
Thuoc? thuoc = thuocBUS.LayTheoMa("TH0001");
```

Nếu `thuoc` khác `null`, M3 có thể đọc:

- `thuoc.MaThuoc`, `thuoc.TenThuoc`: mã và tên thuốc.
- `thuoc.DonGia`: **giá bán** hiện tại, kiểu `decimal`; giá nhập được lưu riêng ở chi tiết phiếu nhập, không ghi đè thuộc tính này.
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

### 3.4. Tìm kiếm Thuốc nâng cao — phần mới

Chữ ký trong [ThuocBUS](./QLNT.BUS/ThuocBUS.cs):

```csharp
public ArrayList TimKiemNangCao(
    string? maThuoc = null, string? tenThuoc = null, string? maLoai = null,
    decimal? giaTu = null, decimal? giaDen = null, PhepNoi noi = PhepNoi.And)
```

Ví dụ tìm tên Para **và** giá bán trong khoảng 5.000–20.000:

```csharp
ArrayList ketQua = new ThuocBUS().TimKiemNangCao(
    tenThuoc: "Para", giaTu: 5000m, giaDen: 20000m, noi: PhepNoi.And);
```

- Mã/tên tìm theo chuỗi chứa; mã loại khớp chính xác. Chuỗi bỏ khoảng trắng hai đầu, không phân biệt hoa/thường.
- Điều kiện trống được bỏ qua. Giá `null` là không giới hạn đầu đó; giá 0 vẫn là điều kiện thực sự. Cả hai đầu khoảng giá đều được tính.
- AND: đúng tất cả điều kiện; OR: đúng ít nhất một trường chữ **hoặc** nằm trong toàn bộ khoảng giá. Ví dụ OR với Para và khoảng giá là `tên chứa Para OR (giá >= 5000 AND giá <= 20000)`.
- Giá âm, khoảng ngược, giá trị phép nối sai gây `ArgumentException` hoặc lớp con. Lỗi đọc database cũng có thể phát sinh ngoại lệ.
- Đầu vào thuốc và kết quả là `ArrayList`. `List<DieuKien>` chỉ chứa tiêu chí cho [BoLoc.Loc](./QLNT.BUS/BoLoc.cs) của M1. Kết quả giữ thứ tự, không nhân đôi thuốc khớp nhiều tiêu chí.
- [Form Thuốc](./QLNT.GUI/frmThuoc.cs) đã có vùng mã/tên/loại, giá tùy chọn, AND/OR, **Tìm**, **Xóa bộ lọc**, **Tải lại**. Nghiệp vụ lọc nằm trong BUS.
- API cũ `TimKiem(string)` được giữ nguyên. Bản cập nhật này chưa tự thêm vùng lọc nâng cao vào hộp thoại chọn thuốc của M3.

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
| Build project kiểm thử ngày 09/11/2026 cùng GUI/BUS/DAO tham chiếu | Thành công; 0 lỗi, 0 cảnh báo. | Chứng minh code biên dịch được; không tự chứng minh toàn bộ nghiệp vụ. |
| Đọc source `ThuocBUS.LayTheoMa` và `ThuocBUS.KiemTraBan` | Đã đối chiếu chữ ký, điều kiện, thứ tự và thông báo trong tài liệu với source. | Là kiểm tra source, không phải thực thi các tình huống bên dưới. |
| Đọc thuốc tồn tại và tải lại cache sau nhập | Đạt: `TH0001` tồn 10, thêm phiếu nhập 3 thì đọc lại tồn 13; giá bán vẫn 100. Xem [Program.KiemTraBUSVaGiaoDich](./tests/QLNT.KiemThuM2/Program.cs). | Chỉ xác nhận dữ liệu thử; không phải mọi trường hợp mã rỗng/không tồn tại. |
| Tìm kiếm Thuốc trong BUS | 36/36 xác nhận đạt trong lần chạy đầy đủ. Xem [ThuocFilterCases.cs](./tests/QLNT.KiemThuM2/ThuocFilterCases.cs). | Dùng cache giả, khôi phục sau chạy. |
| Bộ lọc trên form Thuốc | 17/17 xác nhận đạt trong lần chạy đầy đủ. Xem [ThuocFilterUiCases.cs](./tests/QLNT.KiemThuM2/ThuocFilterUiCases.cs). | Điều khiển WinForms thật trên form ẩn; cache giả. |
| Bộ kiểm thử đầy đủ M2 và tích hợp M3/M4 | **232/232 xác nhận đạt**, exit code 0 ngày 09/11/2026. Xem [README kiểm thử](./tests/QLNT.KiemThuM2/README.md). | SQL dùng database ngẫu nhiên riêng đã xóa; không ghi vào database ứng dụng. Không bao phủ mọi nhánh `KiemTraBan`. |

Bộ kiểm thử hiện tại chưa có ca gọi trực tiếp `ThuocBUS.KiemTraBan`. Các ca hóa đơn đã chạy kiểm tra nghiệp vụ lưu/trừ kho ở module hóa đơn, không thay thế việc kiểm tra riêng từng điều kiện của hàm này. Vì vậy, các ca KB bên dưới vẫn ghi chưa xác nhận.

## 6. Phạm vi đã/chưa xác nhận riêng cho API lấy thuốc và kiểm tra bán

Các kết quả mong đợi bên dưới được suy ra từ source hiện tại. Dùng thuốc thử hoặc dữ liệu đã xác nhận để kiểm tra; không coi mã ví dụ là chắc chắn tồn tại trong database.

| Mã ca | Hàm/tình huống | Kết quả mong đợi theo code | Trạng thái |
| --- | --- | --- | --- |
| LT01 | `LayTheoMa`: mã đang tồn tại | Trả đúng thông tin thuốc, khớp SQL Server. | Đạt với dữ liệu thử; tồn trước/sau nhập và giá bán được đối chiếu trong bộ chạy. |
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
| CACHE01 | Phiếu nhập commit làm thay đổi tồn, lấy lại thuốc | Kết quả dùng số lượng mới trong SQL sau khi BUS xóa cache. | Đạt với thêm/sửa phiếu; chỉ xác nhận đọc lại tồn, chưa kiểm tra mọi nhánh `KiemTraBan`. |
| DB01 | Không có cache, mất kết nối SQL khi đọc thuốc | GUI nhận và xử lý ngoại lệ, không báo thành công. | Chưa xác nhận từng ca. |

Khi chạy thử, ghi lại mã thuốc, số lượng, ngày bán, HSD, tồn thực tế, kết quả nhận được và người thực hiện. Chỉ chuyển trạng thái sang “Đã đạt” khi đã có kết quả thực tế phù hợp.

## 7. Tích hợp M3 trong source hiện tại

M3 có các lớp `HoaDon`/`CTHoaDon`, `HoaDonDAO`/`HoaDonBUS` và màn hình bán hàng. Màn hình danh sách hỗ trợ tìm hóa đơn, xem chi tiết, sửa, hủy và in/xuất PDF. Form lập hóa đơn hỗ trợ chọn nhân viên, khách hàng/khách lẻ, tạo khách hàng mới ngay trong cửa sổ chọn khách hàng (khách mới được tự chọn cho hóa đơn), thêm/xóa thuốc, sửa số lượng, voucher, tính giảm giá và tổng tiền.

### 7.1. Ghi hóa đơn và tồn kho

- Lưu hóa đơn mới ghi đầu hóa đơn, chi tiết và trừ tồn trong **một transaction Serializable**. Hóa đơn chỉ hoàn tất khi toàn bộ thao tác commit.
- Trừ tồn bằng câu `UPDATE` có điều kiện `SOLUONG >= số lượng` và kiểm tra hạn dùng trong cùng transaction. Nếu thuốc hết hạn, không đủ tồn, không tồn tại hoặc một chi tiết không được ghi, transaction rollback và lỗi được báo cho người dùng.
- Sửa hóa đơn khóa và đọc các chi tiết hiện có trong transaction, hoàn số lượng cũ, ghi lại đầu/chi tiết mới rồi trừ tồn mới. Việc này cũng rollback toàn bộ nếu một bước thất bại.
- Hủy hóa đơn hoàn tồn và xóa chi tiết/đầu hóa đơn trong cùng transaction. Schema `HOADON` hiện **không có cột trạng thái hủy**, nên đây là xóa vật lý có hoàn kho, không phải lưu hóa đơn với trạng thái “đã hủy”.
- Mã hóa đơn được sinh trong transaction theo định dạng `HD0001`; thao tác tạo đồng thời được khóa khi lấy mã để tránh cấp trùng.
- Sau lưu/hủy thành công, cache thuốc/khuyến mãi và danh sách hóa đơn được tải lại.

### 7.2. Thanh toán và in

- Form kiểm tra tiền khách đưa không nhỏ hơn tổng thanh toán, hiển thị tiền thừa và cho in hóa đơn nháp hoặc hóa đơn đã lưu; chọn Microsoft Print to PDF trong hộp thoại máy in để lưu PDF.
- Schema hiện không có cột/bảng lưu tiền khách đưa, tiền thừa hay trạng thái thanh toán. Hai giá trị này chỉ được đối chiếu trong form, không được lưu như dữ liệu thanh toán.
- Khi có giảm giá thuốc, thành tiền đã giảm được lưu trên chi tiết; `TIENGIAM`/`TONGTIEN` lưu giảm cấp hóa đơn và số phải trả theo công thức của schema.

### 7.3. Kiểm tra đã thực hiện và giới hạn

Ngày 09/11/2026, build project kiểm thử và các project tham chiếu thành công: 0 cảnh báo, 0 lỗi. Lần chạy đầy đủ đạt 232 xác nhận tự động, exit code 0.

[HoaDonCases.KiemTraHoaDon](./tests/QLNT.KiemThuM2/HoaDonCases.cs) đã tạo/sửa/hủy hóa đơn trong SQL, đối chiếu đầu/chi tiết/tiền giảm/tồn, chặn bán vượt tồn, xuất PDF qua Microsoft Print to PDF, xuất báo cáo quý `.xlsx` và đối chiếu các hàm thống kê. [UiEditCases.KiemTraSuaLuuTrenForm](./tests/QLNT.KiemThuM2/UiEditCases.cs) đã bấm sửa/lưu phiếu nhập và hóa đơn trên form ẩn, xác nhận SQL và tồn sau khi lưu.

Chưa thử thủ công trọn luồng qua menu, in ra giấy, mọi biên hạn dùng, mất kết nối giữa giao dịch hoặc hai máy hoạt động đồng thời. Bằng chứng này áp dụng cho schema/dữ liệu trong database thử riêng; database đó đã được xóa, dữ liệu ứng dụng không bị thay đổi.
