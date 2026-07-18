using PredatorWeb.Entidades;
using System.Reflection;

namespace PredatorWeb.Services
{
    /// <summary>
    /// Servicio para resolver y crear instancias de entidades dinámicamente usando reflexión
    /// </summary>
    public class EntidadResolverService
    {
        private readonly ILogger<EntidadResolverService> _logger;
        private readonly Dictionary<string, Type> _entidadCache;

        public EntidadResolverService(ILogger<EntidadResolverService> logger)
        {
            _logger = logger;
            _entidadCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            // Cargar todas las entidades al inicio
            LoadEntidades();
        }

        /// <summary>
        /// Carga todas las clases que heredan de Entidad en el ensamblado
        /// </summary>
        private void LoadEntidades()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var entidadBaseType = typeof(Entidad);

                var entidadTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(entidadBaseType))
                    .ToList();

                foreach (var type in entidadTypes)
                {
                    // Registrar por nombre completo y por nombre simple
                    _entidadCache[type.FullName ?? type.Name] = type;
                    _entidadCache[type.Name] = type;

                    _logger.LogInformation($"Entidad registrada: {type.Name} ({type.FullName})");
                }

                _logger.LogInformation($"Total de entidades cargadas: {entidadTypes.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las entidades");
            }
        }

        /// <summary>
        /// Obtiene una instancia de la entidad especificada por su nombre
        /// </summary>
        public Entidad? GetEntidadInstance(string nombreEntidad)
        {
            if (string.IsNullOrWhiteSpace(nombreEntidad))
            {
                _logger.LogWarning("Se intentó obtener una entidad con nombre vacío o nulo");
                return null;
            }

            try
            {
                // Buscar primero por nombre exacto
                if (_entidadCache.TryGetValue(nombreEntidad, out var type))
                {
                    var instance = Activator.CreateInstance(type) as Entidad;
                    _logger.LogInformation($"Instancia creada para entidad: {nombreEntidad}");
                    return instance;
                }

                // Buscar por nombre parcial (últimas partes del namespace)
                var matchingTypes = _entidadCache
                    .Where(kvp => kvp.Key.EndsWith(nombreEntidad, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingTypes.Count == 1)
                {
                    var instance = Activator.CreateInstance(matchingTypes[0].Value) as Entidad;
                    _logger.LogInformation($"Instancia creada para entidad (coincidencia parcial): {nombreEntidad}");
                    return instance;
                }

                if (matchingTypes.Count > 1)
                {
                    _logger.LogWarning($"Se encontraron múltiples coincidencias para '{nombreEntidad}': {string.Join(", ", matchingTypes.Select(m => m.Key))}");
                    return null;
                }

                _logger.LogWarning($"No se encontró la entidad: '{nombreEntidad}'. Entidades disponibles: {string.Join(", ", _entidadCache.Keys)}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear instancia de la entidad: {nombreEntidad}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene información de la entidad sin crear una instancia
        /// </summary>
        public Type? GetEntidadType(string nombreEntidad)
        {
            if (string.IsNullOrWhiteSpace(nombreEntidad))
                return null;

            if (_entidadCache.TryGetValue(nombreEntidad, out var type))
                return type;

            return null;
        }

        /// <summary>
        /// Lista todas las entidades registradas
        /// </summary>
        public IEnumerable<string> GetRegisteredEntidades()
        {
            return _entidadCache.Keys.Distinct();
        }
    }
}
