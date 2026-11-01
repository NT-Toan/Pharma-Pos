using System.Data;

public class KhuyenMaiDAO
{
    private readonly MyConnection conn = new MyConnection();

    public DataTable LayTatCaCTKM()
    {
        return conn.LayBang("SELECT MACTKM, TENCTKM, NGAYBATDAU, NGAYKETTHUC FROM CTKHUYENMAI");
    }

    public DataTable LayTatCaKMSanPham()
    {
        return conn.LayBang("SELECT MACTKM, MATHUOC, PHANTRAMGIAM FROM KM_SANPHAM");
    }

    public DataTable LayTatCaVoucher()
    {
        return conn.LayBang("SELECT MAVOUCHER, MACTKM, MAKH, PHANTRAMGIAM FROM KM_VOUCHER");
    }

    public DataTable LayTatCaKMHoaDon()
    {
        return conn.LayBang("SELECT MACTKM, MUCTIENHD, PHANTRAMGIAM FROM KM_HOADON");
    }
}