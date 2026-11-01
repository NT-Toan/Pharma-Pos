using System.Data;
using Microsoft.Data.SqlClient;


public class MyConnection
{

    private string connString = "Server=localhost;Database=QLNhaThuoc;Integrated Security=True;TrustServerCertificate=True;";
    private SqlConnection? connection = null;
    public SqlConnection getConnection()
    {
        if (connection == null)
        {
            connection = new SqlConnection(connString);
        }
        if (connection.State == ConnectionState.Closed)
        {
            connection.Open();
        }

        return connection;
    }
    public void dongKetNoi()
    {
        if (connection != null &&
            connection.State == ConnectionState.Open)
        {
            connection.Close();
        }
    }
    // cái này dùng để lấy dữ liệu từ database về, trả về 1 bảng dữ liệu thường dùng hàm select để gọi các bảng ra 
    public DataTable LayBang(string sql, params SqlParameter[] thamSo)
    {
        DataTable bang = new DataTable();
        using (SqlConnection ketNoi = new SqlConnection(connString))
        using (SqlCommand lenh = new SqlCommand(sql, ketNoi))
        using (SqlDataAdapter boDoc = new SqlDataAdapter(lenh))
        {
            lenh.Parameters.AddRange(thamSo);
            ketNoi.Open();
            boDoc.Fill(bang);
        }

        return bang;
    }
    // cái này dùng để thực hiện các câu lệnh insert, update, delete, trả về số dòng bị ảnh hưởng
    public int ThucThi(string sql, params SqlParameter[] thamso)
    {
        using (SqlConnection ketNoi = new SqlConnection(connString))
        using (SqlCommand lenh = new SqlCommand(sql, ketNoi))
        {
            lenh.Parameters.AddRange(thamso);
            ketNoi.Open();
            return lenh.ExecuteNonQuery();// pt yêu cầu sql sever thực hiện câu lệnh sql sever thực thi câu lệnh đó mà ko trả về bảng dữ liệu
        }
    }
    //lấy giá trị đầu tiên của hàm 
    public object? LayGiaTri(string sql, params SqlParameter[] thamso)
    {
        using (SqlConnection ketNoi = new SqlConnection(connString))
        using (SqlCommand lenh = new SqlCommand(sql, ketNoi))
        {
            lenh.Parameters.AddRange(thamso);
            ketNoi.Open();
            return lenh.ExecuteScalar();// hàm này lấy 1 giá trị duy nhất từ câu lệnh sql, thường dùng để lấy giá trị tổng, đếm, min, max, sum
        }
    }
    //chạy giao dịch là giữ liệu nhất quán khi thực hiện nó sẽ hoàn thành hay hông hoàn thành nha 
    public bool ChayGiaoDich(string[] sql, SqlParameter[] thamso)
    {
        using (SqlConnection ketNoi = new SqlConnection(connString))
        {
            ketNoi.Open();
            SqlTransaction giaoDich = ketNoi.BeginTransaction();// hàm này là bắt đầu 1 giao dịch, nó sẽ giữ liệu nhất quán khi thực hiện nó sẽ hoàn thành hay hông hoàn thành nha
            try
            {
                for (int i = 0; i < sql.Length; i++)
                {
                    using (SqlCommand lenh = new SqlCommand(sql[i], ketNoi, giaoDich))
                    {
                        lenh.Parameters.Add(thamso[i]);//lấy tham số của câu lệnh sql thứ i
                        lenh.ExecuteNonQuery();//hàm này là thực hiện câu lệnh sql, nó sẽ trả về số dòng bị ảnh hưởng
                    }
                }
                giaoDich.Commit();//khi thành công nó sẽ commit lại dữ liệu, nếu có lỗi nó sẽ rollback lại dữ liệu
                return true;
            }
            catch
            {
                giaoDich.Rollback();//nếu hông nó sẽ trả về false và rollback lại dữ liệu 
                return false;
            }

        }
    }
}
