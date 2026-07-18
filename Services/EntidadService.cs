using System.Data;
using PredatorWeb.Entidades;

namespace PredatorWeb.Services
{
    public class EntidadService
    {
        private readonly Datos _datos;
        private readonly ILogger<EntidadService> _logger;

        public EntidadService(Datos datos, ILogger<EntidadService> logger)
        {
            _datos = datos;
            _logger = logger;
        }

        public async Task<DataTable> GetEntidadDataAsync(string nombreEntidad)
        {
            _logger.LogInformation($"Intentando cargar datos para la entidad: '{nombreEntidad}'");

            var entidad = GetEntidadByNombre(nombreEntidad);
            if (entidad == null)
            {
                throw new InvalidOperationException($"No se encontró configuración para la entidad: '{nombreEntidad}'");
            }

            _logger.LogInformation($"Entidad encontrada. Vista a consultar: {entidad.NombreVista}");

            var query = $"SELECT * FROM {entidad.NombreVista}";
            _logger.LogInformation($"Ejecutando query: {query}");

            var dataTable = await _datos.ExecuteQueryAsync(query);

            _logger.LogInformation($"Se cargaron {dataTable.Rows.Count} registros de {entidad.NombreVista}");

            return dataTable;
        }

        private Entidad? GetEntidadByNombre(string nombreEntidad)
        {
            _logger.LogInformation($"Buscando mapeo para: '{nombreEntidad}'");

            // Mapeo de entidades conocidas - más flexible con normalización
            var normalized = nombreEntidad?.Trim().ToLower() ?? "";

            return normalized switch
            {
                "comprobanteventa" or "comprobantes" or "comprobante" or "comprobantesventa" or "comprobanteventas" => new Entidades.Ventas.ComprobanteVenta(),
                _ => null
            };
        }
    }
}
