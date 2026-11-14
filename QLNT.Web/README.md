# Web đặt thuốc

Đây là một chức năng web ASP.NET Core Razor Pages tách khỏi ứng dụng WinForms. Web dùng chung `QLNT.BUS` và `QLNT.DAO` với dự án hiện tại.

1. Chạy `Database/QLNT_dat_thuoc.sql` trên database `QLNhaThuoc` một lần (script có thể chạy lại để bổ sung các cột đơn đặt, thanh toán và khuyến mãi).
2. Kiểm tra SQL Server có thể kết nối bằng Windows Authentication. Có thể đổi server bằng biến môi trường `QLNT_SQL_SERVER`.
3. Trong Git Bash: `cd /c/c_sharp/QLNT.Web`, rồi chạy `ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://127.0.0.1:5085`.
4. Mở `http://127.0.0.1:5085/`.

Trang `/` dành cho khách tìm thuốc, thêm vào giỏ, áp dụng voucher chung, chọn chuyển khoản online / trả tại cửa hàng / COD, xem lại và xác nhận đơn. Web dùng cùng `HoaDonBUS.TinhTienDuThao` như WinForms: giảm sản phẩm trước, sau đó chọn mức có lợi hơn giữa voucher và mốc khuyến mãi hóa đơn. Giỏ hàng hiển thị từng khoản giảm và tổng sau giảm từ báo giá máy chủ; giá được tính lại khi gửi đơn. Web không có trang nhập hàng hay trang xử lý đơn cho nhân viên. Lựa chọn online là thông tin khách khai báo; web chưa kết nối cổng thanh toán.

Khi đặt thành công, DAO kiểm tra số lượng còn lại và trừ tồn kho trong cùng giao dịch SQL với việc lưu đơn. Nếu thiếu hàng hoặc lỗi lưu đơn, toàn bộ giao dịch được rollback. WinForms dùng chung bảng `THUOC`: màn Thuốc đọc lại tồn khi người dùng quay lại ứng dụng, và có nút **Tải lại**.

Trong WinForms, vào **Bán hàng → Đơn đặt từ web** để xem đơn mới, thuốc, khuyến mãi, cách thanh toán và thông tin liên hệ. Nhấn `...` chọn nhân viên (dùng biến `static` của form chọn). Với trả tại cửa hàng, nhập tiền khách đưa; với online, ô tiền bằng 0 và nhân viên phải đánh dấu đã đối chiếu chuyển khoản; với COD, ô tiền bằng 0, hóa đơn ở trạng thái chờ thu. Sau khi giao hàng, bấm **Xác nhận thu COD** và nhập số tiền thực nhận. Hóa đơn COD chỉ được tính vào doanh thu sau bước đó. Hệ thống tạo khách hàng nếu chưa có, tạo hóa đơn từ giá và các khoản giảm đã chốt lúc đặt, rồi liên kết hóa đơn với đơn web trong cùng giao dịch; tồn kho không bị trừ lần hai. Đơn chỉ tạo hóa đơn được một lần. Hủy đơn chưa xử lý hoặc hủy hóa đơn đã tạo sẽ hoàn tồn tương ứng. Voucher riêng của khách hàng thành viên không áp dụng trên web vì web chưa có đăng nhập khách hàng.
