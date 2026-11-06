# Bàn giao M2: Phiếu nhập và nhập danh mục thuốc từ Excel

Ngày kiểm tra: **04/11/2026**.

## 1. Đã làm gì?

- Thêm/sửa/xóa phiếu, lưu đầu phiếu và chi tiết, điều chỉnh tồn trong cùng giao dịch.
- Tính tiền và kiểm tra dữ liệu ở BUS; danh sách nghiệp vụ và tìm kiếm dùng `ArrayList`.
- Chặn tồn âm, tràn số lượng, dữ liệu không hợp lệ và thao tác trên phiếu đã bị người khác thay đổi.
- Nhập danh mục thuốc mới từ file `.xlsx`, có nút tạo mẫu, tự tạo mã, tồn ban đầu bằng 0.
- Bộ kiểm tra trước cập nhật tên/trạng thái nút đạt **118/118**; phần soạn phiếu mới đạt thêm **14/14 kiểm tra riêng**. Giới hạn kiểm thử xem mục 6.

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
| Giao diện Excel | [frmThuoc.cs](./QLNT.GUI/frmThuoc.cs) | Nút **Tạo mẫu Excel**, **Nhập Excel** |

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
| Lấy một thuốc | `new ThuocBUS().LayTheoMa("TH0001")` | Trả `Thuoc?`; kiểm tra `null` nếu không có |
| Kiểm tra bán | `new ThuocBUS().KiemTraBan("TH0001", 2, DateTime.Today)` | Kiểm tra lượng > 0, thuốc tồn tại, chưa hết hạn và đủ tồn; không trừ kho |

Thêm/sửa/xóa phiếu thành công tự xóa cache thuốc trong cùng tiến trình ứng dụng. Màn hình M3 đang mở vẫn cần nạp lại danh sách để hiển thị số mới. Nếu chạy ở ứng dụng khác, phải chủ động tải lại; cache không chia sẻ giữa các tiến trình.

Lệnh trừ/hoàn kho của hóa đơn vẫn phải kiểm tra tồn trong giao dịch SQL. Kết quả `KiemTraBan` có thể cũ nếu một người khác vừa bán hàng.

### 3.3. M4 — tổng chi

- Đọc tổng chi từ `PHIEUNHAP.TONGTIEN`, lọc theo `NGAYNHAP`.
- Giá nhập nằm ở `CTPN.DONGIA`; giá bán nằm ở `THUOC.DONGIA`. Lưu phiếu không ghi đè giá bán.
- Đã gọi trực tiếp [ThongKeBUS.TongChi](./QLNT.BUS/ThongKeBUS.cs) trên database kiểm thử: thêm phiếu tăng chi, sửa phiếu đổi chi, xóa phiếu loại phần chi tương ứng.
- Tổng chi hiện tại phản ánh các phiếu đang có. Đây là xóa vật lý; project chưa có lịch sử kiểm toán phiếu bị xóa.

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

Mã kiểm thử: [Program.cs](./tests/QLNT.KiemThuM2/Program.cs), [Cases.cs](./tests/QLNT.KiemThuM2/Cases.cs), [PickerCases.cs](./tests/QLNT.KiemThuM2/PickerCases.cs), [DraftCases.cs](./tests/QLNT.KiemThuM2/DraftCases.cs). Cách chạy: [README kiểm thử](./tests/QLNT.KiemThuM2/README.md).

Lần chạy toàn bộ gần nhất, trước khi đổi tên/trạng thái 4 nút: **118/118 kiểm tra đạt**, tiến trình kết thúc mã 0.

Sau cập nhật giao diện: chạy riêng `--draft-only`, **14/14 kiểm tra đạt**, không kết nối database. Đã thử thêm nhiều thuốc, tự làm mới ô nhập, chọn/cập nhật/bỏ thuốc, nhập mới không mất bản nháp/đầu phiếu, chặn thêm trùng, khóa cập nhật khi vượt giới hạn tiền và bật lại khi dữ liệu hợp lệ. Build bản kiểm thử ở thư mục tạm: **0 lỗi, 0 cảnh báo**. Build vào thư mục chạy mặc định ban đầu bị chặn bởi ứng dụng đang mở; cần đóng và chạy lại ứng dụng để dùng bản mới.

Database riêng `QLNT_Test_PN_5e1421f5fcec462cb7a89fcbeede6225` được tạo, dùng và đã xóa sau kiểm thử. Không ghi vào `QLNhaThuoc`. Schema kiểm thử có 6 bảng cần cho M2, khóa ngoại và ràng buộc số lượng/giá như các bảng tương ứng trong script nhóm.

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

### Chưa xác nhận bằng kiểm thử hiện tại

- Chưa thao tác thủ công toàn bộ nút lưu/sửa/xóa và hộp thoại chọn file trên màn hình người dùng. Kiểm thử ghi SQL gọi BUS/DAO; phần bản nháp đã thử bấm nút thật trên cửa sổ ẩn với dữ liệu giả thay kết quả hộp thoại chọn thuốc.
- Chưa chạy toàn luồng qua form Hóa đơn M3, khuyến mãi, xuất PDF và báo cáo Excel M4. Ca tranh chấp kho dùng SQL mô phỏng bán, không phải toàn bộ hàm lưu hóa đơn M3.
- Chưa thử giao diện thống kê tháng/quý/năm, xuất báo cáo hoặc kết nối từ nhiều máy; chưa đo tải lớn hay lỗi mất điện/mạng lúc commit.
- Chưa thử mọi schema/trigger khác với script nhóm. Code yêu cầu đúng số dòng tác động; trigger thay đổi cách báo số dòng cần kiểm tra riêng nếu nhóm bổ sung.

### M3/M4 nên thử chung tiếp theo

1. M2 lập phiếu nhập một thuốc thêm 10 đơn vị; ghi lại mã phiếu, tồn cũ và tổng tiền.
2. M3 tải lại thuốc, bán 3; đối chiếu tồn còn `tồn cũ + 7`.
3. M2 sửa phiếu xuống 8: tồn giảm thêm 2. Nếu giảm/xóa đến mức không đủ tồn để hoàn, phải báo lỗi và giữ nguyên phiếu/kho.
4. M4 tải lại báo cáo, đối chiếu tổng chi với tổng tiền các phiếu trong cùng khoảng ngày.
5. Thử phiếu khác có đủ tồn để xóa; đối chiếu chi tiết, kho và tổng chi sau xóa.
