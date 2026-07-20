using PredatorWeb.Entidades;
using System.Reflection;

namespace PredatorWeb.Services
{
    /// <summary>
    /// Servicio para resolver y crear instancias de entidades dinámicamente usando reflexión
    /// </summary>
    public class EntidadResolverService
    {
        private readonly Dictionary<string, Type> _entidadCache;

        public EntidadResolverService()
        {
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
                }
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Obtiene una instancia de la entidad especificada por su nombre
        /// </summary>
        public Entidad? GetEntidadInstance(string nombreEntidad)
        {
            if (string.IsNullOrWhiteSpace(nombreEntidad))
            {
                return null;
            }

            try
            {
                // Buscar primero por nombre exacto
                if (_entidadCache.TryGetValue(nombreEntidad, out var type))
                {
                    var instance = Activator.CreateInstance(type) as Entidad;
                    return instance;
                }

                // Buscar por nombre parcial (últimas partes del namespace)
                var matchingTypes = _entidadCache
                    .Where(kvp => kvp.Key.EndsWith(nombreEntidad, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingTypes.Count == 1)
                {
                    var instance = Activator.CreateInstance(matchingTypes[0].Value) as Entidad;
                    return instance;
                }

                if (matchingTypes.Count > 1)
                {
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
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
