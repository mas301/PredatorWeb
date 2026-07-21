using Microsoft.Data.SqlClient;
using System.Data;

namespace PredatorWeb.Services
{
    public class Datos
    {
        // Cadena de conexión inicial para consultar la tabla de dominios/empresas
        private readonly string _initialConnectionString = "Data Source=srvgesnet.database.windows.net;Initial Catalog=Dominio;User Id=saDominio;Password=Mauricio2004;";

        // Cadena de conexión de respaldo (la original)
        private readonly string _defaultConnectionString = "Data Source=caleb.pe;Initial Catalog=DLK;User Id=sagesnet;Password=D0br@nOc;TrustServerCertificate=True;Encrypt=True;";

        private readonly SessionService _sessionService;

        public Datos(IConfiguration configuration, SessionService sessionService)
        {
            _sessionService = sessionService;

            // Si no hay una conexión activa en la sesión, usar la de respaldo
            if (string.IsNullOrEmpty(_sessionService.ActiveConnectionString))
            {
                _sessionService.ActiveConnectionString = configuration.GetConnectionString("DLK") ?? _defaultConnectionString;
            }
        }

        /// <summary>
        /// Obtiene la cadena de conexión activa
        /// </summary>
        private string GetConnectionString()
        {
            return _sessionService.ActiveConnectionString ?? _defaultConnectionString;
        }

        /// <summary>
        /// Obtiene información de diagnóstico sobre la conexión actual
        /// </summary>
        public string GetConnectionInfo()
        {
            var connString = GetConnectionString();
            return $"Cadena de conexión completa:\n{connString}";
        }

        /// <summary>
        /// Configura la cadena de conexión basada en el código de empresa
        /// </summary>
        public async Task<bool> ConfigureConnectionByCompanyAsync(string codigoEmpresa)
        {
            try
            {
                // Usar la cadena inicial para consultar GrlEmpresas
                using var connection = new SqlConnection(_initialConnectionString);
                await connection.OpenAsync();

                var query = "SELECT Servidor, BaseDatos FROM GrlEmpresas WHERE CodigoEmpresa = @CodigoEmpresa";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CodigoEmpresa", codigoEmpresa);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var servidor = reader.GetString(reader.GetOrdinal("Servidor"));
                    var baseDatos = reader.GetString(reader.GetOrdinal("BaseDatos"));

                    // Construir el User Id concatenando "sa" + BaseDatos
                    var userId = "sa" + baseDatos;
                    var password = "Mauricio2004";

                    // Construir la nueva cadena de conexión
                    var newConnectionString = $"Data Source={servidor};Initial Catalog={baseDatos};User Id={userId};Password={password};TrustServerCertificate=True;Encrypt=True;";

                    // PROBAR LA CONEXIÓN ANTES DE GUARDARLA
                    using var testConnection = new SqlConnection(newConnectionString);
                    await testConnection.OpenAsync();
                    // Si llegamos aquí, la conexión funciona
                    testConnection.Close();

                    _sessionService.ActiveConnectionString = newConnectionString;
                    _sessionService.CompanyCode = codigoEmpresa;
                    _sessionService.IsLoggedIn = true;

                    return true;
                }

                return false;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error de SQL al configurar empresa '{codigoEmpresa}': {sqlEx.Message} (Código: {sqlEx.Number})", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al configurar empresa '{codigoEmpresa}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restaura la cadena de conexión al valor por defecto
        /// </summary>
        public void ResetConnection()
        {
            _sessionService.ActiveConnectionString = _defaultConnectionString;
            _sessionService.IsLoggedIn = false;
            _sessionService.CompanyCode = null;
            _sessionService.Username = null;
        }

        /// <summary>
        /// Ejecuta una consulta SELECT y devuelve un DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string query, params SqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            try
            {
                var connectionString = GetConnectionString();
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                // Usar SqlDataReader en lugar de SqlDataAdapter para operaciones verdaderamente async
                using var reader = await command.ExecuteReaderAsync();
                dataTable.Load(reader);
            }
            catch (SqlException sqlEx)
            {
                var builder = new SqlConnectionStringBuilder(GetConnectionString());
                throw new Exception($"Error SQL en '{builder.DataSource}/{builder.InitialCatalog}': {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar consulta: {ex.Message}", ex);
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
                using var connection = new SqlConnection(GetConnectionString());
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
                using var connection = new SqlConnection(GetConnectionString());
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
            return new SqlConnection(GetConnectionString());
        }

        /// <summary>
        /// Valida las credenciales de usuario ejecutando GrlUsuariosMantenimiento
        /// </summary>
        public async Task<DataTable> ValidateUserCredentialsAsync(string codigoEmpresa, string usuario, string clave)
        {
            var dataTable = new DataTable();

            try
            {
                var connectionString = GetConnectionString();
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("GrlUsuariosMantenimiento", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Agregar parámetros
                command.Parameters.AddWithValue("@Accion", 19);
                command.Parameters.AddWithValue("@RUC", codigoEmpresa);
                command.Parameters.AddWithValue("@Codigo", usuario);
                command.Parameters.AddWithValue("@Clave", clave);

                // Ejecutar y cargar resultados
                using var reader = await command.ExecuteReaderAsync();
                dataTable.Load(reader);
            }
            catch (SqlException sqlEx)
            {
                var builder = new SqlConnectionStringBuilder(GetConnectionString());
                throw new Exception($"Error SQL al validar usuario en '{builder.DataSource}/{builder.InitialCatalog}': {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar credenciales de usuario: {ex.Message}", ex);
            }

            return dataTable;
        }

        /// <summary>
        /// Obtiene las sedes autorizadas para un usuario de una empresa desde la vista GrlSedeVistaLista
        /// </summary>
        public async Task<DataTable> GetSedesByUsuarioAsync(int empresaId, int usuarioId)
        {
            var dataTable = new DataTable();

            try
            {
                var connectionString = GetConnectionString();
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT SedeId, Sede FROM GrlSedeVistaLista WHERE EmpresaId = @EmpresaId AND UsuarioId = @UsuarioId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmpresaId", empresaId);
                command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                using var reader = await command.ExecuteReaderAsync();
                dataTable.Load(reader);
            }
            catch (SqlException sqlEx)
            {
                var builder = new SqlConnectionStringBuilder(GetConnectionString());
                throw new Exception($"Error SQL al obtener sedes en '{builder.DataSource}/{builder.InitialCatalog}': {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener sedes del usuario: {ex.Message}", ex);
            }

            return dataTable;
        }
    }
}
