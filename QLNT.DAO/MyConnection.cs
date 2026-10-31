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
    public DataTable LayBang(string sql)
    {
        DataTable bang = new DataTable();
        using (SqlConnection ketNoi = new SqlConnection(connString))
        using (SqlCommand lenh = new SqlCommand(sql, ketNoi))
        using (SqlDataAdapter boDoc = new SqlDataAdapter(lenh))
        {
            ketNoi.Open();
            boDoc.Fill(bang);
        }

        return bang;
    }
}
