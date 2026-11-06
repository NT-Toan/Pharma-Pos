# Phân công và thứ tự bàn giao đồ án Quản lý nhà thuốc

## Quy ước chung

- Dùng SQL Server và cùng cấu trúc, dữ liệu mẫu từ `QLNT_v2.sql`, `QLNT_data.sql`.
- Kiến trúc 3 lớp: `QLNT.GUI` → `QLNT.BUS` → `QLNT.DAO`. GUI chỉ gọi BUS; SQL chỉ nằm trong DAO và đi qua `MyConnection`.
- Các class dữ liệu như `LoaiThuoc`, `Thuoc`, `HoaDon` nằm trong DAO. BUS chứa nghiệp vụ, kiểm tra dữ liệu và tìm kiếm trên `ArrayList`. Đây là quyết định mới của nhóm; bản quy ước PDF cũ còn ghi `List<T>` nên cả nhóm phải thống nhất theo `ArrayList`.
- Thêm, sửa, xóa phải ghi SQL Server rồi cập nhật hoặc tải lại `ArrayList`. Tìm kiếm thông thường không chạy `SELECT` mỗi lần người dùng gõ. Thống kê tổng hợp được phép truy vấn SQL.
- Mỗi người làm đủ DAO → BUS → GUI cho module mình, làm trên nhánh riêng. Chỉ ghép vào `main` khi chức năng đã chạy được.

## Trạng thái hiện tại và giới hạn bàn giao

- Solution đã có 3 project GUI, BUS, DAO. Đã có phần đọc, hiển thị và tìm Loại thuốc theo mã hoặc tên.
- `MyConnection.LayBang`, `ThucThi`, `LayGiaTri` đã có và project Build được. M2 có thể dùng chúng để làm CRUD Loại thuốc.
- `ChayGiaoDich` đã viết nhưng **chưa bàn giao cho phần Phiếu nhập/Hóa đơn**: hiện mỗi câu SQL chỉ nhận một tham số, và lệnh tác động 0 dòng vẫn có thể được commit. M1 phải hoàn thiện và thử rollback trước khi M2/M3 dùng.
- Build thành công chưa thay cho thử kết nối và thao tác trên SQL Server thật. Người nhận module phải chạy thử chức năng của mình với dữ liệu mẫu.

## Việc giao ngay tại mốc M1 hiện tại

M1 đã có `LayBang`, `ThucThi`, `LayGiaTri`; `ChayGiaoDich` chưa sẵn sàng cho nghiệp vụ nhiều bảng. Ba người dưới đây có thể bắt đầu **song song** trên nhánh riêng sau khi lấy code mới nhất từ `main`.

### M2 — Nam: bắt đầu từ Loại thuốc

1. Hoàn thiện chức năng **Thêm → Sửa → Xóa Loại thuốc** trên phần danh sách/tìm kiếm đã có. DAO gọi `MyConnection.ThucThi` với tham số; BUS kiểm tra dữ liệu bắt buộc, mã duy nhất và cập nhật/tải lại `ArrayList`; form chỉ gọi BUS.
2. Thử trên SQL Server thật: thêm một loại, sửa tên, xóa loại chưa được tham chiếu; kiểm tra form hiện đúng sau mỗi lần ghi. Trường hợp loại đã được thuốc tham chiếu phải báo lỗi rõ, không làm hỏng form.
3. Sau khi Loại thuốc chạy đủ CRUD, làm CRUD **Thuốc** rồi **Nhà cung cấp**. Bàn giao sớm cho M3 hàm lấy thuốc theo mã và kiểm tra còn hạn, đủ tồn.

**Tạm chờ M1 bàn giao transaction:** chưa ghép phần lưu/sửa/xóa Phiếu nhập với chi tiết và thay đổi tồn kho.

### M3 — Toàn: dựng luồng xem Hóa đơn

1. Tạo class dữ liệu, DAO và BUS để **đọc danh sách Hóa đơn và CTHD**; dựng form main-sub với hai danh sách. Chọn một hóa đơn ở danh sách trên thì danh sách dưới hiện các thuốc của hóa đơn đó.
2. Chuẩn bị form nhập hóa đơn: nút `…` để chọn nhân viên, khách hàng, thuốc; vùng tổng tiền dễ thấy; mã, đơn giá, thành tiền và tổng tiền là ô chỉ đọc. Form chỉ gọi BUS, không viết SQL trong GUI.
3. Thống nhất sớm với M2/M4 các hàm lấy thuốc và khuyến mãi sẽ gọi. M3 sở hữu form hóa đơn; M1 sở hữu `Program.cs` và màn hình chính để tránh hai người sửa cùng file.

**Cập nhật source:** đã có danh sách hóa đơn/chi tiết, form lập nháp, chọn nhân viên/khách hàng/thuốc, tính tiền nháp và in hóa đơn đã tồn tại (có thể chọn máy in PDF). Form nháp chưa lưu xuống database, chưa sửa/xóa hóa đơn và chưa trừ/hoàn tồn kho; cần M1 hoàn thiện transaction trước khi bật ghi dữ liệu. Chưa xác nhận runtime với SQL Server.

**Tạm chờ M1 + M2 + M4 bàn giao:** chưa ghép thao tác lưu/sửa/xóa Hóa đơn và trừ/hoàn tồn kho.

### M4 — Tường: ưu tiên hàm khuyến mãi cho M3

1. Làm dữ liệu và CRUD **chương trình khuyến mãi**, sau đó **giảm giá sản phẩm**, **voucher**, **mốc giảm hóa đơn**. DAO dùng SQL có tham số; kiểm tra ngày hiệu lực, khách được dùng voucher và mức giảm trong BUS.
2. Bàn giao sớm cho M3 ba hàm: lấy phần trăm giảm sản phẩm theo ngày; kiểm tra voucher hợp lệ cho khách và ngày bán; chọn mốc giảm hóa đơn đang hiệu lực. Chốt cách chọn loại giảm giá trước khi M3 tính tổng tiền.
3. Khi các hàm cho M3 chạy được, làm màn hình quản lý khuyến mãi và chuẩn bị các hàm thống kê từ dữ liệu mẫu. Đối chiếu số liệu cuối cùng sau khi M3 có hóa đơn chạy thật.

**Tạm chờ M3 bàn giao hóa đơn thật để nghiệm thu thống kê**, nhưng có thể phát triển truy vấn thống kê trên dữ liệu mẫu từ trước.

### Mốc bàn giao tiếp theo

1. **Nam → Toàn:** thuốc có thể tra theo mã, kiểm tra hạn dùng và tồn kho.
2. **Tường → Toàn:** ba hàm khuyến mãi ở trên chạy đúng với dữ liệu mẫu.
3. **M1 → Nam và Toàn:** `ChayGiaoDich` hỗ trợ nhiều tham số cho từng lệnh và đã thử commit/rollback.
4. **Toàn → Tường:** hóa đơn, chi tiết và tổng tiền chạy thật để đối chiếu thống kê.

## Phân công theo người

| Người | Ưu tiên làm trước | Làm sau khi nhận đủ phần phụ thuộc | Bàn giao |
| --- | --- | --- | --- |
| **Lộc — Nền tảng, hệ thống** | Hoàn thiện và thử `ChayGiaoDich`; chốt `MyConnection`, `KetQua`, mã tự sinh, bộ lọc `ArrayList` với `AND`/`OR` và phép so sánh. | Nhân viên, khách hàng, bảng tham số, màn hình chính menu trái, form `…` chọn nhân viên/khách hàng; helper Excel dùng chung khi cần. | API dùng chung cho M2–M4; transaction đã thử commit và rollback cho M2/M3. |
| **Nam — Thuốc, nhập hàng** | Hoàn thiện CRUD Loại thuốc trên form hiện có, sau đó CRUD Thuốc và Nhà cung cấp; tìm trên `ArrayList`, form `…` chọn thuốc/NCC. | Khi M1 bàn giao transaction: Phiếu nhập + chi tiết, tăng/hoàn tồn kho trong một giao dịch; cuối cùng nhập thuốc từ Excel. | Hàm lấy thuốc, kiểm tra đủ tồn và chưa hết hạn cho M3; dữ liệu tồn kho đúng sau nhập hàng. |
| **Toàn — Bán hàng** | Dựng form hóa đơn gồm danh sách hóa đơn và danh sách chi tiết; chọn hóa đơn thì hiện các dòng thuốc. Chuẩn bị nút `…` và các ô tự điền/chỉ đọc. | Khi nhận hàm thuốc của M2, khuyến mãi của M4 và transaction của M1: lưu/sửa/xóa Hóa đơn + chi tiết, trừ/hoàn tồn kho, tính tiền; cuối cùng in/xuất PDF hóa đơn. | Hóa đơn chạy thật để M4 đối chiếu thống kê. |
| **Tường — Khuyến mãi, thống kê** | Làm chương trình khuyến mãi, giảm theo sản phẩm, voucher, mốc giảm hóa đơn; ưu tiên các hàm kiểm tra và tính mức giảm cho M3. | Thống kê tổng thu, tổng chi, lợi nhuận theo khoảng ngày/tháng/quý/năm; bảng theo nhân viên, khách hàng, sản phẩm và quý; xuất Excel. Đối chiếu với hóa đơn thật của M3. | Hàm khuyến mãi cho M3; bảng thống kê và file Excel cho cả nhóm kiểm thử. |

## Thứ tự bàn giao

1. **Bắt đầu ngay, làm song song:** M2 làm CRUD Loại thuốc rồi Thuốc/NCC; M4 làm các hàm khuyến mãi; M3 dựng form hóa đơn; M1 hoàn thiện transaction và phần dùng chung.
2. **M1 bàn giao transaction:** chỉ sau khi thử được nhiều tham số trong một lệnh, commit khi thành công và rollback khi có lỗi hoặc bước bắt buộc tác động 0 dòng. Lúc đó M2 làm Phiếu nhập và M3 làm phần lưu Hóa đơn.
3. **M2 + M4 bàn giao cho M3:** M2 giao hàm lấy/kiểm tra thuốc; M4 giao hàm giảm giá sản phẩm, voucher và mốc hóa đơn. M3 ghép nghiệp vụ bán hàng và tồn kho.
4. **M3 bàn giao cho M4:** khi hóa đơn có chi tiết và tổng tiền đúng, M4 đối chiếu thống kê. Bảng theo sản phẩm dùng `CTHD.THANHTIEN`; tổng theo nhân viên/khách hàng dùng `HOADON.TONGTIEN`.
5. **Cả nhóm kiểm thử:** tìm kiếm nhiều điều kiện; thêm/sửa/xóa; dữ liệu tự điền và các ô không cho nhập; nhập/xuất Excel; PDF; màn hình chính. Mỗi người giải thích được module của mình trước buổi chấm.

## Điều kiện hoàn thành một chức năng trước khi ghép vào `main`

- Build cả solution thành công; form chạy và thao tác được trên SQL Server với dữ liệu mẫu.
- DAO dùng câu SQL có tham số; GUI không gọi DAO trực tiếp; kiểm tra nghiệp vụ nằm trong BUS.
- Tìm kiếm chạy trên `ArrayList`; sau khi ghi DB, danh sách trên form phản ánh dữ liệu mới.
- Với Phiếu nhập/Hóa đơn, đầu phiếu, chi tiết và tồn kho cùng commit hoặc cùng rollback.
- Không đưa `bin/`, `obj/`, `*.csproj.user` lên GitHub.

## Tài liệu gốc cần đối chiếu

- `C-w7-YeuCauDoAn.pdf`: yêu cầu bổ sung của cô, gồm 3 lớp, main-sub, tìm kiếm nâng cao, thống kê, Excel, PDF, màn hình chính và báo cáo.
- `QLNT - Quy ước nhóm và hợp đồng hàm BUS.pdf`: phân công M1–M4, chữ ký hàm và quy tắc tính tiền. Quyết định dùng `ArrayList` của nhóm được ưu tiên khi triển khai.
