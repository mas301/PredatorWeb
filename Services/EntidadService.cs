using System.Data;
using System.Text;
using PredatorWeb.Entidades;
using PredatorWeb.Models;

namespace PredatorWeb.Services
{
    public class EntidadService
    {
        private readonly Datos _datos;
        private readonly EntidadResolverService _entidadResolver;

        public EntidadService(Datos datos, EntidadResolverService entidadResolver)
        {
            _datos = datos;
            _entidadResolver = entidadResolver;
        }

        public async Task<DataTable> GetEntidadDataAsync(string nombreEntidad)
        {
            return await GetEntidadDataAsync(nombreEntidad, null, CancellationToken.None);
        }

        public async Task<DataTable> GetEntidadDataAsync(string nombreEntidad, ServerFilter? filters)
        {
            return await GetEntidadDataAsync(nombreEntidad, filters, CancellationToken.None);
        }

        public async Task<DataTable> GetEntidadDataAsync(string nombreEntidad, CancellationToken cancellationToken)
        {
            return await GetEntidadDataAsync(nombreEntidad, null, cancellationToken);
        }

        public async Task<DataTable> GetEntidadDataAsync(string nombreEntidad, ServerFilter? filters, CancellationToken cancellationToken)
        {
            // Verificar cancelación antes de comenzar
            cancellationToken.ThrowIfCancellationRequested();

            var entidad = _entidadResolver.GetEntidadInstance(nombreEntidad);
            if (entidad == null)
            {
                throw new InvalidOperationException($"No se encontró configuración para la entidad: '{nombreEntidad}'");
            }

            var query = BuildFilteredQuery(entidad.NombreVista, filters);

            // Verificar cancelación antes de ejecutar la consulta
            cancellationToken.ThrowIfCancellationRequested();

            var dataTable = await _datos.ExecuteQueryAsync(query, cancellationToken);

            return dataTable;
        }

        private string BuildFilteredQuery(string viewName, ServerFilter? filters)
        {
            var query = new StringBuilder($"SELECT * FROM {viewName}");
            var whereClauses = new List<string>();

            if (filters != null)
            {
                // Filtros de texto (LIKE)
                foreach (var filter in filters.ColumnFilters)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Value))
                    {
                        var columnName = SanitizeColumnName(filter.Key);
                        var filterValue = filter.Value.Replace("'", "''"); // Escapar comillas simples
                        whereClauses.Add($"{columnName} LIKE '%{filterValue}%'");
                    }
                }

                // Filtros multi-valor (IN)
                foreach (var filter in filters.MultiValueFilters)
                {
                    if (filter.Value != null && filter.Value.Any())
                    {
                        var columnName = SanitizeColumnName(filter.Key);
                        var escapedValues = filter.Value.Select(v => $"'{v.Replace("'", "''")}'");
                        var inClause = string.Join(", ", escapedValues);
                        whereClauses.Add($"{columnName} IN ({inClause})");
                    }
                }

                // Filtros de fecha
                foreach (var filter in filters.DateFilters)
                {
                    var columnName = SanitizeColumnName(filter.Key);
                    var dateFilter = filter.Value;

                    switch (dateFilter.Type)
                    {
                        case DateFilterType.Today:
                            whereClauses.Add($"CAST({columnName} AS DATE) = CAST(GETDATE() AS DATE)");
                            break;

                        case DateFilterType.ThisWeek:
                            whereClauses.Add($"{columnName} >= DATEADD(DAY, 1-DATEPART(WEEKDAY, GETDATE()), CAST(GETDATE() AS DATE))");
                            whereClauses.Add($"{columnName} < DATEADD(DAY, 8-DATEPART(WEEKDAY, GETDATE()), CAST(GETDATE() AS DATE))");
                            break;

                        case DateFilterType.ThisMonth:
                            whereClauses.Add($"YEAR({columnName}) = YEAR(GETDATE()) AND MONTH({columnName}) = MONTH(GETDATE())");
                            break;

                        case DateFilterType.ThisYear:
                            whereClauses.Add($"YEAR({columnName}) = YEAR(GETDATE())");
                            break;

                        case DateFilterType.CustomRange:
                            if (dateFilter.StartDate.HasValue)
                            {
                                whereClauses.Add($"{columnName} >= '{dateFilter.StartDate.Value:yyyy-MM-dd}'");
                            }
                            if (dateFilter.EndDate.HasValue)
                            {
                                whereClauses.Add($"{columnName} <= '{dateFilter.EndDate.Value:yyyy-MM-dd 23:59:59}'");
                            }
                            break;
                    }
                }

                // Filtros booleanos
                foreach (var filter in filters.BooleanFilters)
                {
                    if (filter.Value != BooleanFilterType.All)
                    {
                        var columnName = SanitizeColumnName(filter.Key);
                        var boolValue = filter.Value == BooleanFilterType.Checked ? "1" : "0";
                        whereClauses.Add($"{columnName} = {boolValue}");
                    }
                }

                // WHERE
                if (whereClauses.Any())
                {
                    query.Append(" WHERE ");
                    query.Append(string.Join(" AND ", whereClauses));
                }

                // ORDER BY
                if (!string.IsNullOrEmpty(filters.SortColumn))
                {
                    var columnName = SanitizeColumnName(filters.SortColumn);
                    var direction = filters.SortAscending ? "ASC" : "DESC";
                    query.Append($" ORDER BY {columnName} {direction}");
                }
            }

            return query.ToString();
        }

        private string SanitizeColumnName(string columnName)
        {
            // Validar que el nombre de columna solo contenga caracteres seguros
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be empty");

            // Solo permitir letras, números, guiones bajos y puntos
            if (!System.Text.RegularExpressions.Regex.IsMatch(columnName, @"^[a-zA-Z0-9_\.]+$"))
                throw new ArgumentException($"Invalid column name: {columnName}");

            return columnName;
        }

        /// <summary>
        /// Obtiene una instancia de la entidad por su nombre
        /// </summary>
        public Entidad? GetEntidad(string nombreEntidad)
        {
            return _entidadResolver.GetEntidadInstance(nombreEntidad);
        }

        /// <summary>
        /// Obtiene los valores únicos de una columna, limitado a un máximo especificado
        /// </summary>
        public async Task<List<string>?> GetDistinctValuesAsync(string nombreEntidad, string columnName, int maxCount = 50)
        {
            var entidad = _entidadResolver.GetEntidadInstance(nombreEntidad);
            if (entidad == null)
            {
                throw new InvalidOperationException($"No se encontró configuración para la entidad: '{nombreEntidad}'");
            }

            var sanitizedColumn = SanitizeColumnName(columnName);

            // Primero verificar cuántos valores distintos hay
            var countQuery = $"SELECT COUNT(DISTINCT {sanitizedColumn}) AS DistinctCount FROM {entidad.NombreVista} WHERE {sanitizedColumn} IS NOT NULL";
            var countTable = await _datos.ExecuteQueryAsync(countQuery);

            if (countTable.Rows.Count == 0)
                return null;

            var distinctCount = Convert.ToInt32(countTable.Rows[0]["DistinctCount"]);

            // Si hay más valores distintos que el máximo, retornar null para indicar que debe usar input de texto
            if (distinctCount > maxCount)
            {
                return null;
            }

            // Obtener los valores únicos
            var query = $"SELECT DISTINCT {sanitizedColumn} FROM {entidad.NombreVista} WHERE {sanitizedColumn} IS NOT NULL ORDER BY {sanitizedColumn}";
            var dataTable = await _datos.ExecuteQueryAsync(query);

            var values = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                var value = row[0]?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    values.Add(value);
                }
            }

            return values;
        }
    }
}
