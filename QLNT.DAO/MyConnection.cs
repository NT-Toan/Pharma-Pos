using System.Data;
using Microsoft.Data.SqlClient;

public class MyConnection
{
    private static readonly string serverName =
        Environment.GetEnvironmentVariable("QLNT_SQL_SERVER") ?? "localhost";
    private static readonly string databaseName =
        Environment.GetEnvironmentVariable("QLNT_DATABASE") ?? "QLNhaThuoc";
    private static readonly string connString = TaoChuoiKetNoi();
    private SqlConnection? connection;

    private static string TaoChuoiKetNoi()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = serverName,
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };
        return builder.ConnectionString;
    }

    private static void MoKetNoi(SqlConnection ketNoi)
    {
        try
        {
            ketNoi.Open();
        }
        catch (SqlException ex) when (ex.Number == 4060)
        {
            throw new InvalidOperationException(
                $"Không mở được database '{databaseName}' trên SQL Server '{serverName}'. " +
                "Hãy tạo database này và cấp quyền cho tài khoản Windows hiện tại. " +
                "Sau đó chọn database làm database hiện hành khi chạy D:\\QLNT_v2.sql, " +
                "rồi chạy D:\\QLNT_data.sql để nạp dữ liệu mẫu.",
                ex);
        }
    }

    public SqlConnection getConnection()
    {
        connection ??= new SqlConnection(connString);
        if (connection.State == ConnectionState.Closed)
            MoKetNoi(connection);

        return connection;
    }

    public void dongKetNoi()
    {
        if (connection?.State == ConnectionState.Open)
            connection.Close();
    }

    public DataTable LayBang(string sql, params SqlParameter[] thamSo)
    {
        var bang = new DataTable();
        using var ketNoi = new SqlConnection(connString);
        using var lenh = new SqlCommand(sql, ketNoi);
        using var boDoc = new SqlDataAdapter(lenh);
        lenh.Parameters.AddRange(thamSo);
        MoKetNoi(ketNoi);
        boDoc.Fill(bang);
        return bang;
    }

    public int ThucThi(string sql, params SqlParameter[] thamso)
    {
        using var ketNoi = new SqlConnection(connString);
        using var lenh = new SqlCommand(sql, ketNoi);
        lenh.Parameters.AddRange(thamso);
        MoKetNoi(ketNoi);
        return lenh.ExecuteNonQuery();
    }

    public object? LayGiaTri(string sql, params SqlParameter[] thamso)
    {
        using var ketNoi = new SqlConnection(connString);
        using var lenh = new SqlCommand(sql, ketNoi);
        lenh.Parameters.AddRange(thamso);
        MoKetNoi(ketNoi);
        return lenh.ExecuteScalar();
    }

    public T ChayGiaoDich<T>(Func<SqlConnection, SqlTransaction, T> thaoTac)
    {
        ArgumentNullException.ThrowIfNull(thaoTac);
        using var ketNoi = new SqlConnection(connString);
        MoKetNoi(ketNoi);
        using SqlTransaction giaoDich = ketNoi.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            T ketQua = thaoTac(ketNoi, giaoDich);
            giaoDich.Commit();
            return ketQua;
        }
        catch (Exception thaoTacException)
        {
            try
            {
                giaoDich.Rollback();
            }
            catch (Exception rollbackException)
            {
                throw new AggregateException("Giao dịch thất bại và không thể rollback.", thaoTacException, rollbackException);
            }
            throw;
        }
    }
}
