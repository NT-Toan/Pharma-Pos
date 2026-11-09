# Bàn giao M2: Thuốc, Phiếu nhập, tồn kho và Excel

Người bàn giao: **M2 — Nam**. Người nhận: **M3 — Toàn, M4 — Tường và M1 phụ trách ghép code**.

Ngày cập nhật và kiểm thử gần nhất: **09/11/2026**.

**Trạng thái:** bản source đã qua bộ kiểm thử tự động. Nhánh bàn giao là `m2-thuoc-nhap`, gồm phần tìm kiếm Thuốc nâng cao, giao diện bộ lọc và các thay đổi kiểm thử/tài liệu. M1 nhận commit chứa tài liệu này để ghép và nghiệm thu; bản bàn giao này chưa được hợp nhất vào `main`.

## Đọc nhanh: mỗi người nhận gì?

| Người nhận | Nội dung bàn giao | Phần cần phối hợp |
| --- | --- | --- |
| M3 | Lấy thuốc, giá bán, tồn sau nhập; kiểm tra điều kiện bán; tìm kiếm nhiều trường AND/OR | Tải lại dữ liệu trên màn hình bán hàng và đối chiếu tồn sau nhập/bán/sửa/hủy |
| M4 | Đầu phiếu, chi tiết, ngày nhập, tổng tiền; kết quả đối chiếu tổng chi | Chọn cùng khoảng ngày khi so sánh phiếu nhập với báo cáo |
| M1 | Source M2, cách gọi BUS và kết quả kiểm thử | Ghép đúng bản, build và thử luồng chung qua menu |

## 1. Đã làm gì?

- Thêm/sửa/xóa phiếu, lưu đầu phiếu và chi tiết, điều chỉnh tồn trong cùng giao dịch.
- Tính tiền và kiểm tra dữ liệu ở BUS; danh sách nghiệp vụ và tìm kiếm dùng `ArrayList`.
- Chặn tồn âm, tràn số lượng, dữ liệu không hợp lệ và thao tác trên phiếu đã bị người khác thay đổi.
- Nhập danh mục thuốc mới từ file `.xlsx`, có nút tạo mẫu, tự tạo mã, tồn ban đầu bằng 0.
- Tìm Thuốc theo mã/tên/loại, khoảng giá bán, chọn AND/OR; lọc trên `ArrayList` trong BUS, dùng bộ lọc M1. Form chỉ gửi điều kiện và hiển thị kết quả.
- Build **0 lỗi, 0 cảnh báo**; bộ kiểm thử hiện tại đạt **232/232 xác nhận tự động**, exit code 0. Đây là tổng các ca M2 và tích hợp M3/M4; phạm vi và giới hạn xem mục 6.

## 2. File và kiến trúc

| Thành phần | File | Trách nhiệm |
| --- | --- | --- |
| Dữ liệu đầu phiếu | [PhieuNhap.cs](./QLNT.DAO/PhieuNhap.cs) | Mã phiếu, NCC, nhân viên, ngày nhập, tổng tiền |
| Dữ liệu chi tiết | [CTPhieuNhap.cs](./QLNT.DAO/CTPhieuNhap.cs) | Thuốc, số lượng, giá nhập, thành tiền tính từ số lượng × giá |
| Bản chụp phiếu | [DuLieuPhieuNhap.cs](./QLNT.DAO/DuLieuPhieuNhap.cs) | Đầu phiếu và `ArrayList` chi tiết khi mở sửa/xóa |
| Nghiệp vụ | [PhieuNhapBUS.cs](./QLNT.BUS/PhieuNhapBUS.cs) | Đọc, tìm, kiểm tra, tính tổng; gọi DAO để ghi |
| Truy cập SQL | [PhieuNhapDAO.cs](./QLNT.DAO/PhieuNhapDAO.cs) | Câu SQL có tham số, khóa dữ liệu và kiểm tra số dòng tác động |
| Danh sách phiếu | [frmPhieuNhap.cs](./QLNT.GUI/frmPhieuNhap.cs) | Danh sách phiếu và chi tiết; nút thêm/sửa/xóa |
| Soạn phiếu | [frmTaoPhieuNhap.cs](./QLNT.GUI/frmTaoPhieuNhap.cs) | Chọn nhân viên/NCC/thuốc; chỉnh các dòng dự thảo; lưu thêm hoặc sửa |
| Đọc/tạo Excel | [ThuocExcel.cs](./QLNT.BUS/ThuocExcel.cs) | Đọc giá trị ô, kiểm tra cấu trúc file, tạo mẫu `.xlsx` bằng ClosedXML |
| Nhập thuốc | [ThuocBUS.cs](./QLNT.BUS/ThuocBUS.cs), [ThuocDAO.cs](./QLNT.DAO/ThuocDAO.cs) | `NhapTuExcel` kiểm tra cả file; `ThemHangLoat` lưu trong một giao dịch |
| Tìm kiếm Thuốc | [ThuocBUS.cs](./QLNT.BUS/ThuocBUS.cs), [BoLoc.cs](./QLNT.BUS/BoLoc.cs) | `TimKiemNangCao` kết hợp các trường và nhóm khoảng giá trên `ArrayList` |
| Giao diện Thuốc | [frmThuoc.cs](./QLNT.GUI/frmThuoc.cs) | Vùng tìm kiếm AND/OR; nút **Tìm**, **Xóa bộ lọc**, **Tải lại**, **Tạo mẫu Excel**, **Nhập Excel** |

Luồng lưu phiếu: GUI → BUS → DAO → [MyConnection.ChayGiaoDich](./QLNT.DAO/MyConnection.cs) → SQL Server.
GUI không tự viết SQL hoặc tự sửa tồn kho. Hàm giao dịch của M1 đã được dùng, không thay đổi.

### Các nút soạn thuốc trên phiếu

Trong [frmTaoPhieuNhap](./QLNT.GUI/frmTaoPhieuNhap.cs):

- **Thêm thuốc:** đưa thuốc đang nhập vào bảng dự thảo; chỉ bật khi chưa chọn dòng để sửa và dữ liệu dự kiến hợp lệ theo BUS.
- **Cập nhật:** sửa thuốc đang chọn trong bảng; chỉ bật khi có lựa chọn và dữ liệu dự kiến hợp lệ.
- **Bỏ khỏi phiếu:** bỏ thuốc đang chọn khỏi bản nháp, tính lại tổng; không xóa thuốc khỏi danh mục.
- **Nhập mới:** bỏ lựa chọn, xóa ô thuốc, đưa số lượng về 1 và giá về 0; giữ nguyên đầu phiếu, bảng và tổng tiền.

Thêm/cập nhật thành công tự làm mới vùng nhập. Khi đang chọn thuốc trong bảng, nút Thêm thuốc tắt để tránh nhầm thao tác. Kiểm tra nghiệp vụ vẫn dùng `PhieuNhapBUS.TinhTongTien`; thay đổi bản nháp chưa ghi SQL. Nút **Lưu phiếu** giữ nhiệm vụ lưu toàn bộ phiếu.

## 3. Hàm bàn giao và cách gọi

### 3.1. Đọc và ghi phiếu

Các hàm dưới đây thuộc [PhieuNhapBUS](./QLNT.BUS/PhieuNhapBUS.cs).

| Hàm | Ví dụ gọi | Kết quả / lưu ý |
| --- | --- | --- |
| `LayTatCa()` | `new PhieuNhapBUS().LayTatCa()` | `ArrayList` chứa `PhieuNhap`; dùng bộ nhớ đệm |
| `TimKiem(tuKhoa)` | `bus.TimKiem("PN0001")` | Tìm trên `ArrayList` theo mã phiếu/NCC/nhân viên |
| `LayChiTiet(maPN)` | `bus.LayChiTiet("PN0001")` | `ArrayList` chứa `CTPhieuNhap`; đọc SQL theo mã |
| `TinhTongTien(chiTiet)` | `bus.TinhTongTien(cacDong)` | Tổng số lượng × giá nhập; phát sinh lỗi nếu chi tiết không hợp lệ |
| `KiemTraDuLieu(phieu, chiTiet)` | `bus.KiemTraDuLieu(phieu, cacDong)` | `KetQua`, chỉ kiểm tra, không lưu |
| `Them(phieu, chiTiet)` | `bus.Them(phieu, cacDong)` | `KetQua`; sau thành công, đối tượng `phieu` nhận mã và tổng tiền chính thức |
| `LayDeSua(maPN)` | `bus.LayDeSua("PN0001")` | `DuLieuPhieuNhap`; đọc đầu phiếu và chi tiết trong cùng giao dịch; giữ làm bản gốc |
| `Sua(phieuMoi, chiTietMoi, banGoc)` | `bus.Sua(phieuMoi, cacDongMoi, banGoc)` | Tính lại tổng, điều chỉnh chênh lệch tồn; kiểm tra phiếu chưa bị người khác đổi |
| `Xoa(maPN, banGoc)` | `bus.Xoa("PN0001", banGoc)` | Xóa chi tiết, đầu phiếu và trừ lại lượng đã nhập; chặn nếu không đủ tồn |
| `TaiLai()` | `PhieuNhapBUS.TaiLai()` | Xóa bộ nhớ đệm phiếu; lần đọc tiếp theo lấy SQL |

Khi sửa, hãy tạo đối tượng và chi tiết mới, giữ nguyên `banGoc`. Giao diện hiện tại đã làm như vậy.
Nếu bỏ tham số `banGoc`, DAO vẫn tính chênh lệch từ dữ liệu hiện tại trong SQL, nhưng không phát hiện việc người dùng đang sửa một bản cũ. Giao diện luôn truyền bản gốc.

Các hàm đọc có thể ném ngoại lệ khi mất kết nối/không còn phiếu; bên gọi cần xử lý lỗi. Các hàm ghi của BUS trả về `KetQua`, đọc `ThanhCong` và `ThongBao`.

### 3.2. M3 — thuốc và tồn kho sau nhập

Các hàm thuộc [ThuocBUS](./QLNT.BUS/ThuocBUS.cs); tài liệu trước: [Bàn giao thuốc cho M3](./BAN_GIAO_THUOC_M3.md).

| Cần làm | Cách gọi | Ý nghĩa |
| --- | --- | --- |
| Làm mới dữ liệu trước khi dùng lại màn hình đang mở | `ThuocBUS.TaiLai()` | Lần đọc tiếp theo lấy lại tồn mới |
| Lấy danh sách thuốc | `new ThuocBUS().LayTatCa()` | `ArrayList` chứa các đối tượng `Thuoc` |
| Lấy một thuốc | `new ThuocBUS().LayTheoMa("TH0001")` | Trả `Thuoc?`; kiểm tra `null` nếu không có |
| Kiểm tra bán | `new ThuocBUS().KiemTraBan("TH0001", 2, DateTime.Today)` | Kiểm tra lượng > 0, thuốc tồn tại, chưa hết hạn và đủ tồn; không trừ kho |

Ví dụ M3 lấy lại giá và tồn sau khi phiếu nhập đã lưu thành công:

```csharp
ThuocBUS.TaiLai();
var thuocBUS = new ThuocBUS();
Thuoc? thuoc = thuocBUS.LayTheoMa("TH0001");
if (thuoc != null)
{
    decimal giaBan = thuoc.DonGia;
    int tonHienTai = thuoc.SoLuong;
    DateTime? hanDung = thuoc.HSD;
    KetQua kiemTra = thuocBUS.KiemTraBan(thuoc.MaThuoc, 2, DateTime.Today);
    // GUI đọc kiemTra.ThanhCong và kiemTra.ThongBao để xử lý.
}
```

Đây là mẫu gọi API; mã `TH0001` phải được thay bằng thuốc người dùng chọn. Các hàm đọc có thể ném ngoại lệ kết nối; GUI cần xử lý lỗi. Không sửa trực tiếp `thuoc.SoLuong` để cập nhật kho: đối tượng thuộc cache, thao tác đó không ghi SQL.

`thuoc.DonGia` là **giá bán**; giá mua nằm ở `CTPhieuNhap.DonGia`. Nhập thuốc chỉ tăng tồn, không tự đổi giá bán. `KiemTraBan` kiểm tra hạn dùng và tồn trong cùng một hàm: HSD trước ngày bán bị chặn; HSD bằng ngày bán được phép; HSD `null` bỏ qua bước hạn dùng theo code hiện tại. Hàm chỉ kiểm tra, không giữ chỗ hoặc trừ kho. Các trường hợp biên hạn dùng chưa có ca kiểm thử trực tiếp trong lần chạy 232 xác nhận.

Thêm/sửa/xóa phiếu thành công tự xóa cache thuốc trong cùng tiến trình ứng dụng. Màn hình M3 đang mở vẫn cần nạp lại danh sách để hiển thị số mới. Nếu chạy ở ứng dụng khác, phải chủ động tải lại; cache không chia sẻ giữa các tiến trình.

Lệnh trừ/hoàn kho của hóa đơn vẫn phải kiểm tra tồn trong giao dịch SQL. Kết quả `KiemTraBan` có thể cũ nếu một người khác vừa bán hàng.

### 3.3. M4 — tổng chi

- Đọc tổng chi từ `PHIEUNHAP.TONGTIEN`, lọc theo `NGAYNHAP`.
- Giá nhập nằm ở `CTPN.DONGIA`; giá bán nằm ở `THUOC.DONGIA`. Lưu phiếu không ghi đè giá bán.
- Đã gọi trực tiếp [ThongKeBUS.TongChi](./QLNT.BUS/ThongKeBUS.cs) trên database kiểm thử: thêm phiếu tăng chi, sửa phiếu đổi chi, xóa phiếu loại phần chi tương ứng.
- Tổng chi hiện tại phản ánh các phiếu đang có. Đây là xóa vật lý; project chưa có lịch sử kiểm toán phiếu bị xóa.

Ví dụ M4 đọc đầu phiếu, các dòng thuốc và đối chiếu tổng chi:

```csharp
var phieuBUS = new PhieuNhapBUS();
PhieuNhapBUS.TaiLai();
ArrayList cacPhieu = phieuBUS.LayTatCa();
ArrayList cacDong = phieuBUS.LayChiTiet("PN0001");
decimal tongChi = new QLNT.BUS.ThongKeBUS()
    .TongChi(DateTime.Today, DateTime.Today);
```

Đọc `PhieuNhap.MaPN`, `NgayNhap`, `TongTien` ở đầu phiếu; đọc `CTPhieuNhap.SoLuong`, `DonGia`, `ThanhTien` ở chi tiết. `LayTatCa()` chưa lọc theo ngày; `TongChi(tuNgay, denNgay)` lấy tổng trong khoảng ngày truyền vào, tính cả ngày đầu và ngày cuối. Tổng chi ngày không nhất thiết bằng một phiếu, vì có thể có nhiều phiếu trong ngày. `ThongKeBUS` là phần sẵn có của M4; M2 đã gọi để đối chiếu, không bổ sung API thống kê mới.

### 3.4. Tìm kiếm Thuốc nhiều trường và khoảng giá

API thuộc [ThuocBUS.TimKiemNangCao](./QLNT.BUS/ThuocBUS.cs):

```csharp
public ArrayList TimKiemNangCao(
    string? maThuoc = null, string? tenThuoc = null, string? maLoai = null,
    decimal? giaTu = null, decimal? giaDen = null, PhepNoi noi = PhepNoi.And)
```

Ví dụ tên chứa Para **và** giá bán từ 5.000 đến 20.000:

```csharp
ArrayList ketQua = new ThuocBUS().TimKiemNangCao(
    tenThuoc: "Para", giaTu: 5000m, giaDen: 20000m, noi: PhepNoi.And);
```

Đổi `noi` sang `PhepNoi.Or` để tìm tên chứa Para **hoặc** giá nằm trong toàn bộ khoảng 5.000–20.000.

| Điều kiện | Quy tắc |
| --- | --- |
| Mã/tên thuốc | Tìm chuỗi chứa; bỏ khoảng trắng hai đầu, không phân biệt hoa/thường |
| Loại thuốc | Truyền **mã loại**, khớp chính xác; form hiển thị tên để chọn |
| Giá từ/đến | Giá bán `Thuoc.DonGia`; tính cả hai đầu; có thể chỉ truyền một đầu |
| Điều kiện trống | Bỏ qua; tất cả trống trả toàn bộ thuốc |
| AND | Đúng tất cả các điều kiện đã nhập |
| OR | Đúng ít nhất một trường chữ hoặc đúng toàn bộ nhóm khoảng giá; không nối OR giữa hai đầu giá |
| Kết quả | `ArrayList`, giữ thứ tự gốc, thuốc khớp nhiều điều kiện không bị nhân đôi |
| Giá âm/khoảng ngược/phép nối sai | Ném `ArgumentException` hoặc lớp con; bên gọi cần bắt lỗi |

Danh sách thuốc và kết quả dùng `ArrayList`; `List<DieuKien>` chỉ chứa tiêu chí để gọi [BoLoc.Loc](./QLNT.BUS/BoLoc.cs) của M1. Lần đọc đầu có thể nạp thuốc từ DAO; việc lọc thực hiện trong bộ nhớ.

Trên [form Thuốc](./QLNT.GUI/frmThuoc.cs): nhập điều kiện → đánh dấu giá từ/đến nếu cần → chọn AND/OR → **Tìm** hoặc Enter. **Xóa bộ lọc** đặt lại tiêu chí và hiện toàn bộ thuốc. **Tải lại** đọc dữ liệu mới rồi áp dụng lại điều kiện hiện tại. Khoảng giá ngược hiển thị lỗi ngay vùng lọc và giữ kết quả trước đó.

## 4. Quy tắc tồn kho, tiền và rollback

| Thao tác trong [PhieuNhapDAO](./QLNT.DAO/PhieuNhapDAO.cs) | Quy tắc |
| --- | --- |
| `Them` | Tồn mới = tồn hiện tại + lượng nhập |
| `Sua` → `DieuChinhTon` | Tồn mới = tồn hiện tại + lượng trên phiếu mới − lượng trên phiếu cũ |
| `Xoa` → `DieuChinhTon` | Tồn mới = tồn hiện tại − lượng trên phiếu bị xóa |

Thuốc bỏ khỏi phiếu mới được tính lượng mới bằng 0; thuốc thêm vào được tính lượng cũ bằng 0. Chỉ sửa giá nhập thì tồn không đổi. Ví dụ phiếu cũ nhập 10, phiếu mới nhập 12: cộng thêm 2 vào tồn hiện tại.

[PhieuNhapBUS.TinhTongTien](./QLNT.BUS/PhieuNhapBUS.cs) yêu cầu lượng nguyên dương; không trùng mã thuốc; giá nhập không âm, tối đa 2 số lẻ; tổng tiền nằm trong giới hạn `DECIMAL(18,2)`. Giá nhập 0 được phép theo cấu trúc bảng. Giá bán của thuốc mới từ Excel phải lớn hơn 0.

Các hàm ghi trong [PhieuNhapDAO](./QLNT.DAO/PhieuNhapDAO.cs) chạy toàn bộ lệnh bằng cùng connection/transaction của M1. Lệnh bắt buộc cập nhật/thêm/xóa một dòng phải tác động đúng 1 dòng; xóa chi tiết phải đúng số dòng đã đọc. Khi sai số dòng hoặc có lỗi SQL, DAO ném ngoại lệ để M1 rollback toàn bộ. Tồn kho phải nằm trong 0–2.147.483.647.

Mã phiếu hiển thị khi thêm là mã dự kiến. Mã lưu chính thức được cấp dưới khóa giao dịch để tránh hai người nhận cùng mã. Chỉ cập nhật mã/tổng trên đối tượng đầu vào và xóa cache sau khi commit thành công.

## 5. Nhập thuốc từ Excel

### Cách thao tác

1. Mở mục quản lý Thuốc → **Tạo mẫu Excel** → chọn nơi lưu.
2. Mở file, điền trang `THUOC` từ dòng 2. Trang `LOAITHUOC` liệt kê mã loại hiện có; trang `HUONGDAN` giải thích cách nhập.
3. Lưu file rồi vào **Nhập Excel** → chọn file `.xlsx` → xác nhận.
4. Nếu báo sai dòng nào, sửa dòng đó rồi nhập lại. Chỉ cần một dòng sai thì cả file không được lưu.

| Cột bắt buộc ở dòng 1 | Nội dung |
| --- | --- |
| `TENTHUOC` | Tên 1–100 ký tự |
| `MALOAI` | Mã loại thuốc có trong database |
| `HSD` | Để trống, ô ngày Excel hoặc chữ `dd/MM/yyyy` / `yyyy-MM-dd`; năm 1753–9998 để giao diện hiển thị được |
| `DONGIA` | Giá bán > 0, tối đa 2 số lẻ. Nên nhập ô số Excel; nếu nhập chữ thì dùng dấu chấm thập phân, không phân cách hàng nghìn |

Không thêm cột `SOLUONG` hoặc `MATHUOC`: mã được tạo tự động, tồn ban đầu là 0. Tăng tồn bằng phiếu nhập.

[ThuocExcel.Doc](./QLNT.BUS/ThuocExcel.cs) ưu tiên trang `THUOC`, nếu không có thì dùng trang đầu. Nhận cột theo tên, cho phép đổi thứ tự, bỏ dòng trống hoàn toàn. Không nhận công thức, ô lỗi, thiếu/trùng/thừa tiêu đề. Giới hạn file 20 MB và dòng cuối không quá 10.001 (gồm tiêu đề).

[ThuocBUS.NhapTuExcel](./QLNT.BUS/ThuocBUS.cs) chặn thuốc trùng **tên + loại + hạn dùng** trong file hoặc với dữ liệu hiện có. Đây là nhập thuốc mới; không cập nhật thuốc cũ và không nhập lại số lượng tồn. Thuốc có ngày đã hết hạn không tự bị loại ở bước nhập danh mục; nghiệp vụ bán kiểm tra hạn khi bán.

[ThuocDAO.ThemHangLoat](./QLNT.DAO/ThuocDAO.cs) cấp mã dưới khóa và lưu cả lô trong một giao dịch. Nếu có lỗi SQL giữa lô, thuốc đã thêm ở đầu lô cũng rollback.

API: `new ThuocBUS().NhapTuExcel(duongDan)` và `new ThuocBUS().TaoMauExcel(duongDan)`, đều trả `KetQua`.

Chỉ hỗ trợ `.xlsx`; file `.xls` cũ cần mở trong Excel và **Save As → .xlsx**. Không đổi tên phần mở rộng để chuyển định dạng. Thư viện: [ClosedXML 0.105.1](https://www.nuget.org/packages/ClosedXML/0.105.1), được khai báo trong [project BUS](./QLNT.BUS/QLNT.BUS.csproj).

## 6. Kết quả kiểm thử và phần còn cần thử chung

Mã kiểm thử: [Program.cs](./tests/QLNT.KiemThuM2/Program.cs), [Cases.cs](./tests/QLNT.KiemThuM2/Cases.cs), [PickerCases.cs](./tests/QLNT.KiemThuM2/PickerCases.cs), [DraftCases.cs](./tests/QLNT.KiemThuM2/DraftCases.cs), [UiEditCases.cs](./tests/QLNT.KiemThuM2/UiEditCases.cs), [ThuocFilterCases.cs](./tests/QLNT.KiemThuM2/ThuocFilterCases.cs), [ThuocFilterUiCases.cs](./tests/QLNT.KiemThuM2/ThuocFilterUiCases.cs). Cách chạy và kết quả chi tiết: [README kiểm thử](./tests/QLNT.KiemThuM2/README.md).

Lần chạy đầy đủ ngày **09/11/2026**: **232/232 xác nhận tự động đạt**, tiến trình kết thúc mã **0**. Build project kiểm thử và các project GUI/BUS/DAO tham chiếu: **0 lỗi, 0 cảnh báo**. Tổng này đã bao gồm các ca bộ lọc mới và tích hợp M3/M4; không cộng thêm các kết quả chạy riêng vào 232.

Các lệnh đã dùng:

```powershell
dotnet build tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj --no-restore
dotnet run --project tests/QLNT.KiemThuM2/QLNT.KiemThuM2.csproj --no-build --no-restore
```

Database riêng `QLNT_Test_PN_afdef8ee4a9140e99e928ba0ce187948` được tạo, xác minh kết nối và đã xóa sau kiểm thử. Không ghi vào `QLNhaThuoc`. Schema thử chứa các bảng cần cho Phiếu nhập, Thuốc, Hóa đơn, Khuyến mãi và Thống kê cùng các khóa/ràng buộc tương ứng. Các ca bộ lọc dùng cache giả và khôi phục sau khi chạy. Kiểm thử giao diện chạy trên điều khiển WinForms thật ở form ẩn.

| Nhóm đã chạy | Nội dung xác minh | Kết quả |
| --- | --- | --- |
| Kiểm tra BUS | Phiếu rỗng; thuốc trùng; lượng 0/âm; giá âm/quá 2 số lẻ; tràn tiền; NV/NCC/thuốc không tồn tại | Đạt |
| Thêm phiếu | Đầu phiếu + nhiều chi tiết + tăng kho; tự tính lại tổng; giữ giá bán; làm mới cache; hai người thêm đồng thời | Đạt |
| Sửa phiếu | Tăng lượng; bỏ/đổi thuốc; sửa giá khi đã bán bớt; tính lại tổng và cache; hai người sửa cùng bản gốc | Đạt |
| Chặn lỗi sửa/xóa | Không còn phiếu; bản chụp cũ; không đủ tồn để giảm/bỏ dòng/xóa; tràn tồn | Đạt |
| Rollback | Lệnh tăng/giảm kho tác động 0 dòng sau lệnh đầu thành công; lỗi PK khi thêm chi tiết; lỗi FK cuối lúc sửa đầu phiếu | Đạt; so sánh dữ liệu đầu phiếu, chi tiết và kho trước/sau lỗi |
| Xóa phiếu | Xóa đầu + chi tiết, trừ lại lượng nhập, cache được làm mới; thiếu tồn ở thuốc sau rollback thuốc đầu | Đạt |
| Tranh chấp kho | Xóa phiếu đồng thời với một giao dịch SQL mô phỏng lệnh bán trừ 7/10 đơn vị | Đạt; chỉ một thao tác thành công, tồn không âm |
| Tích hợp tổng chi M4 | Gọi BUS thống kê đang có, đối chiếu tổng chi trong ngày sau thêm/sửa/xóa | Đạt |
| Excel hợp lệ | `.xlsx` thực; Unicode; giá lẻ; ngày Excel/ngày chữ/không HSD; dòng trống; đổi thứ tự cột; mã tự động và tồn 0 | Đạt |
| Excel không hợp lệ | Tên rỗng/dài; loại không có; giá sai; ngày sai/mơ hồ/ngoài khoảng; trùng thuốc; thiếu/trùng/thừa cột; công thức; file không có/hỏng/đuôi `.xls` | Đạt; dòng sai có báo số dòng khi phù hợp, không lưu lô |
| Giao dịch nhập Excel | Lỗi FK hoặc thuốc trùng giữa lô rollback phần đầu; hai file nhập đồng thời; hết mã; file hỏng được đóng | Đạt |
| Khởi tạo WinForms | Mở form ẩn; tải danh sách; chọn phiếu hiện chi tiết; nạp phiếu sửa; mã/tổng chỉ đọc; các nút mới có mặt; picker chọn được thuốc tồn 0 | Đạt |
| Chọn/đổi thuốc WinForms | Chuyển dòng khi lượng cũ ở mức tối đa; bỏ chọn; thuốc tồn 0/int.MaxValue; xóa danh sách; tìm không có kết quả rồi xóa từ khóa | Đạt cho nhập/bán; lỗi gán Value=0 khi Minimum=1 đã được tái hiện trước sửa và hết lỗi sau sửa |
| Soạn bản nháp qua nút mới | Thêm/cập nhật/bỏ thuốc, nhập mới và tự làm mới; trạng thái nút theo lựa chọn/dữ liệu; tổng tiền; giữ nguyên đầu phiếu | 14/14 đạt sau cập nhật UX; không ghi SQL |
| Bộ lọc Thuốc trong BUS | Mã/tên/loại, AND/OR, nhóm khoảng giá, biên giá, giá lẻ, Unicode, ô trống, kết quả rỗng, không trùng, dữ liệu sai, không đổi cache | 36/36 đạt trong lần chạy đầy đủ |
| Bộ lọc trên form Thuốc | Nút Tìm, Enter, AND/OR, chọn loại, giá tùy chọn/giá 0, xóa bộ lọc, báo khoảng giá sai, bố cục tối thiểu | 17/17 đạt trong lần chạy đầy đủ; dùng cache giả |
| Bấm sửa/lưu trên form thật | Mở phiếu đã lưu, đổi lượng, cập nhật chi tiết rồi bấm Lưu; kiểm tra SQL và tồn sau commit | Đạt trên database thử |
| Hóa đơn M3 và báo cáo | Tạo/sửa/hủy hóa đơn, giá/giảm giá/tồn, bấm lưu form sửa hóa đơn, xuất PDF thật, báo cáo quý `.xlsx` và đối chiếu thống kê tháng/quý/năm | Đạt các ca tự động trong bộ chạy; xem [HoaDonCases.cs](./tests/QLNT.KiemThuM2/HoaDonCases.cs) |

### Ví dụ nhập rồi bán đã có kết quả thực tế

[UiEditCases.KiemTraSuaLuuTrenForm](./tests/QLNT.KiemThuM2/UiEditCases.cs) dùng thuốc thử `TH0900`, giá bán 100, tồn ban đầu 0:

| Bước thực hiện | Tồn đã xác minh | Thông tin đối chiếu |
| --- | --- | --- |
| Tạo phiếu nhập 10, giá nhập 5 qua BUS | 10 | Ca tạo xác nhận thành công và tồn 10 |
| Trên form sửa phiếu: đổi lượng thành 12, Cập nhật rồi Lưu | 12 | Phiếu nhập 60 |
| Tạo hóa đơn bán 2 qua BUS | 10 | Ca này xác nhận tạo thành công và tồn 10 |
| Trên form sửa hóa đơn: đổi lượng bán thành 3 rồi Lưu | 9 | Hóa đơn 300 |

Mã thuốc/phiếu của ca này chỉ nằm trong database thử đã xóa; không phải dữ liệu có sẵn để nhóm tra trong database ứng dụng.

### Chưa xác nhận bằng kiểm thử hiện tại

- Chưa thao tác thủ công trọn luồng qua menu trên dữ liệu đồ án hiện có, gồm các hộp thoại chọn file, chọn từng đối tượng, thêm/xóa phiếu qua mọi nút. Đã bấm nút bản nháp và sửa/lưu trên form ẩn như bảng trên.
- Chưa có ca gọi trực tiếp `ThuocBUS.KiemTraBan` cho từng biên hạn dùng, mã không tồn tại và số lượng 0/âm/bằng tồn. Các kiểm tra này mới được đối chiếu source; không coi việc hóa đơn lưu thành công là đã bao phủ mọi nhánh của API này. Chi tiết tại [Bàn giao Thuốc cho M3](./BAN_GIAO_THUOC_M3.md).
- Chưa thử trọn giao diện chọn khoảng ngày thống kê và mọi báo cáo nhiều năm; đã đối chiếu các hàm thống kê tháng/quý/năm và đọc lại tệp Excel/PDF trong các ca tự động.
- Ca tranh chấp xóa phiếu/bán đồng thời dùng một lệnh SQL bán có giao dịch; chưa chạy hai ứng dụng trên hai máy đồng thời. Chưa đo tải lớn hay thử mất điện/mạng lúc commit.
- Chưa thử mọi schema/trigger khác với script nhóm. Code yêu cầu đúng số dòng tác động; trigger thay đổi cách báo số dòng cần kiểm tra riêng nếu nhóm bổ sung.

### M3/M4 nên thử chung tiếp theo

1. M2 lập phiếu nhập một thuốc thêm 10 đơn vị; ghi lại mã phiếu, tồn cũ và tổng tiền.
2. M3 tải lại thuốc, bán 3; đối chiếu tồn còn `tồn cũ + 7`.
3. M2 sửa phiếu xuống 8: tồn giảm thêm 2. Nếu giảm/xóa đến mức không đủ tồn để hoàn, phải báo lỗi và giữ nguyên phiếu/kho.
4. M4 tải lại báo cáo, đối chiếu tổng chi với tổng tiền các phiếu trong cùng khoảng ngày.
5. Thử phiếu khác có đủ tồn để xóa; đối chiếu chi tiết, kho và tổng chi sau xóa.

Đây là kịch bản nghiệm thu chung đề nghị, không phải kết quả thủ công đã thực hiện. Ghi lại mã thuốc, mã phiếu/hóa đơn, tồn trước/sau, khoảng ngày báo cáo, kết quả và người kiểm tra. M1 nhận đúng bản bàn giao rồi chạy lại trên môi trường của nhóm.
