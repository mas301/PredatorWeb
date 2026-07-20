using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace PredatorWeb.Services
{
    public class ConfigurationService
    {
        private readonly Datos _datos;
        private readonly SessionService _sessionService;

        public ConfigurationService(Datos datos, SessionService sessionService)
        {
            _datos = datos;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Obtiene la configuración guardada de una grilla desde GrlSeteo
        /// </summary>
        public async Task<string?> GetGridConfigurationAsync(string nombreVista)
        {
            if (!_sessionService.EmpresaId.HasValue || !_sessionService.UsuarioId.HasValue)
                return null;

            try
            {
                var query = @"
                    SELECT Archivo 
                    FROM GrlSeteo 
                    WHERE EmpresaId = @EmpresaId 
                      AND UsuarioId = @UsuarioId 
                      AND Objeto = @Objeto 
                      AND Propiedad = @Propiedad";

                var parameters = new[]
                {
                    new SqlParameter("@EmpresaId", _sessionService.EmpresaId.Value),
                    new SqlParameter("@UsuarioId", _sessionService.UsuarioId.Value),
                    new SqlParameter("@Objeto", nombreVista),
                    new SqlParameter("@Propiedad", "GrillaAval")
                };

                var result = await _datos.ExecuteQueryAsync(query, parameters);

                if (result.Rows.Count > 0 && result.Rows[0]["Archivo"] != DBNull.Value)
                {
                    return result.Rows[0]["Archivo"].ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener configuración de grilla: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Guarda la configuración de una grilla en GrlSeteo
        /// </summary>
        public async Task SaveGridConfigurationAsync(string nombreVista, string configJson)
        {
            if (!_sessionService.EmpresaId.HasValue || !_sessionService.UsuarioId.HasValue)
                throw new InvalidOperationException("No hay sesión activa");

            try
            {
                // Verificar si ya existe una configuración
                var existingConfig = await GetGridConfigurationAsync(nombreVista);

                string query;
                if (existingConfig == null)
                {
                    // INSERT
                    query = @"
                        INSERT INTO GrlSeteo (EmpresaId, UsuarioId, Objeto, Propiedad, Archivo)
                        VALUES (@EmpresaId, @UsuarioId, @Objeto, @Propiedad, @Archivo)";
                }
                else
                {
                    // UPDATE
                    query = @"
                        UPDATE GrlSeteo 
                        SET Archivo = @Archivo
                        WHERE EmpresaId = @EmpresaId 
                          AND UsuarioId = @UsuarioId 
                          AND Objeto = @Objeto 
                          AND Propiedad = @Propiedad";
                }

                var parameters = new[]
                {
                    new SqlParameter("@EmpresaId", _sessionService.EmpresaId.Value),
                    new SqlParameter("@UsuarioId", _sessionService.UsuarioId.Value),
                    new SqlParameter("@Objeto", nombreVista),
                    new SqlParameter("@Propiedad", "GrillaAval"),
                    new SqlParameter("@Archivo", configJson)
                };

                await _datos.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar configuración de grilla: {ex.Message}", ex);
            }
        }
    }
}
