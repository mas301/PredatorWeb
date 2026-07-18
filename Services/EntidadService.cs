using System.Data;
using PredatorWeb.Entidades;

namespace PredatorWeb.Services
{
    public class EntidadService
    {
        private readonly Datos _datos;
        private readonly EntidadResolverService _entidadResolver;
        private readonly ILogger<EntidadService> _logger;

        public EntidadService(Datos datos, EntidadResolverService entidadResolver, ILogger<EntidadService> logger)
        {
            _datos = datos;
            _entidadResolver = entidadResolver;
            _logger = logger;
        }

        public async Task<DataTable> GetEntidadDataAsync(string nombreEntidad)
        {
            _logger.LogInformation($"Intentando cargar datos para la entidad: '{nombreEntidad}'");

            var entidad = _entidadResolver.GetEntidadInstance(nombreEntidad);
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

        /// <summary>
        /// Obtiene una instancia de la entidad por su nombre
        /// </summary>
        public Entidad? GetEntidad(string nombreEntidad)
        {
            return _entidadResolver.GetEntidadInstance(nombreEntidad);
        }
    }
}
