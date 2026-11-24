using Npgsql;
using portfolio.Models;

namespace portfolio.Data
{
    public class Database
    {
        private readonly string connectionString =
            "Host=localhost;Port=5432;Database=portfolio_db;Username=postgres;Password=YOUR_PASSWORD";

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}