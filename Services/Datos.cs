using Microsoft.Data.SqlClient;
using System.Data;

namespace PredatorWeb.Services
{
    public class Datos
    {
        private readonly string _connectionString;
        private readonly ILogger<Datos> _logger;

        public Datos(IConfiguration configuration, ILogger<Datos> logger)
        {
            _connectionString = configuration.GetConnectionString("DLK")
                ?? "Data Source=caleb.pe;Initial Catalog=DLK;User Id=sagesnet;Password=D0br@nOc;TrustServerCertificate=True;Encrypt=True;";
            _logger = logger;

            _logger.LogInformation($"Datos inicializado con connection string: {MaskConnectionString(_connectionString)}");
        }

        /// <summary>
        /// Ejecuta una consulta SELECT y devuelve un DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string query, params SqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            try
            {
                _logger.LogInformation($"Ejecutando query: {query}");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);

                _logger.LogInformation($"Query ejecutado exitosamente. {dataTable.Rows.Count} registros obtenidos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar query: {query}");
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
                _logger.LogInformation($"Ejecutando query con reader: {query}");

                var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                var reader = await command.ExecuteReaderAsync();
                _logger.LogInformation("Query ejecutado exitosamente con reader.");

                return reader;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar query con reader: {query}");
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
                _logger.LogInformation($"Ejecutando comando: {query}");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                affectedRows = await command.ExecuteNonQueryAsync();

                _logger.LogInformation($"Comando ejecutado exitosamente. {affectedRows} filas afectadas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar comando: {query}");
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
                _logger.LogInformation($"Ejecutando comando scalar: {query}");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                result = await command.ExecuteScalarAsync();

                _logger.LogInformation($"Comando scalar ejecutado exitosamente. Resultado: {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ejecutar comando scalar: {query}");
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

        /// <summary>
        /// Enmascara la contraseña en la cadena de conexión para logs
        /// </summary>
        private string MaskConnectionString(string connectionString)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString,
                @"Password=([^;]+)",
                "Password=***");
        }
    }
}
