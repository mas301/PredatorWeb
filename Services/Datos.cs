using Microsoft.Data.SqlClient;
using System.Data;

namespace PredatorWeb.Services
{
    public class Datos
    {
        private readonly string _connectionString;

        public Datos(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DLK")
                ?? "Data Source=caleb.pe;Initial Catalog=DLK;User Id=sagesnet;Password=D0br@nOc;TrustServerCertificate=True;Encrypt=True;";
        }

        /// <summary>
        /// Ejecuta una consulta SELECT y devuelve un DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string query, params SqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw;
            }

            return dataTable;
        }

        /// <summary>
        /// Ejecuta una consulta SELECT y devuelve un SqlDataReader
        /// </summary>
        public async Task<SqlDataReader> ExecuteReaderAsync(string query, SqlConnection connection, params SqlParameter[] parameters)
        {
            try
            {
                var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                var reader = await command.ExecuteReaderAsync();

                return reader;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL (INSERT, UPDATE, DELETE) y devuelve el número de filas afectadas
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
        {
            int affectedRows = 0;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                affectedRows = await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            return affectedRows;
        }

        /// <summary>
        /// Ejecuta un comando scalar (COUNT, SUM, etc.) y devuelve un valor único
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string query, params SqlParameter[] parameters)
        {
            object? result = null;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                result = await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Crea una nueva conexión SQL
        /// </summary>
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
