using Microsoft.Data.SqlClient;
using System;

class TableChecker
{
    static void Main()
    {
        string connectionString = "Server=MSI;Database=QL_NhaNghi;Trusted_Connection=True;TrustServerCertificate=True;";
        string[] tables = { "doi_tac", "thanh_toan_doi_tac", "tai_khoan_ngan_hang_doi_tac", "co_so_luu_tru", "anh_phong" };
        
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            foreach (var table in tables)
            {
                string sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}'";
                int count = (int)new SqlCommand(sql, connection).ExecuteScalar();
                Console.WriteLine($"Table '{table}': {(count > 0 ? "EXISTS" : "MISSING")}");
            }
        }
    }
}
