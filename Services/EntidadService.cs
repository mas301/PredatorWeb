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
        private readonly SessionService _sessionService;

        public EntidadService(Datos datos, EntidadResolverService entidadResolver, SessionService sessionService)
        {
            _datos = datos;
            _entidadResolver = entidadResolver;
            _sessionService = sessionService;
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

            var query = BuildFilteredQuery(entidad, filters);

            // Verificar cancelación antes de ejecutar la consulta
            cancellationToken.ThrowIfCancellationRequested();

            var dataTable = await _datos.ExecuteQueryAsync(query, cancellationToken);

            return dataTable;
        }

        private string BuildFilteredQuery(Entidad entidad, ServerFilter? filters)
        {
            var query = new StringBuilder($"SELECT * FROM {entidad.NombreVista}");
            var whereClauses = new List<string>();

            // Filtro automático por EmpresaId si la entidad lo requiere
            if (entidad.FiltrarxEmpresa && _sessionService.EmpresaId.HasValue)
            {
                whereClauses.Add($"EmpresaId = {_sessionService.EmpresaId.Value}");
            }

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

                // Filtros de string con tipo de coincidencia
                foreach (var filter in filters.StringFilters)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Value.Value))
                    {
                        var columnName = SanitizeColumnName(filter.Key);
                        var filterValue = filter.Value.Value.Replace("'", "''"); // Escapar comillas simples

                        switch (filter.Value.Type)
                        {
                            case StringFilterType.Contains:
                                whereClauses.Add($"{columnName} LIKE '%{filterValue}%'");
                                break;

                            case StringFilterType.StartsWith:
                                whereClauses.Add($"{columnName} LIKE '{filterValue}%'");
                                break;

                            case StringFilterType.EndsWith:
                                whereClauses.Add($"{columnName} LIKE '%{filterValue}'");
                                break;

                            case StringFilterType.Equals:
                                whereClauses.Add($"{columnName} = '{filterValue}'");
                                break;
                        }
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

                        case DateFilterType.Yesterday:
                            whereClauses.Add($"CAST({columnName} AS DATE) = CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)");
                            break;

                        case DateFilterType.ThisWeek:
                            whereClauses.Add($"{columnName} >= DATEADD(DAY, 1-DATEPART(WEEKDAY, GETDATE()), CAST(GETDATE() AS DATE))");
                            whereClauses.Add($"{columnName} < DATEADD(DAY, 8-DATEPART(WEEKDAY, GETDATE()), CAST(GETDATE() AS DATE))");
                            break;

                        case DateFilterType.LastWeek:
                            whereClauses.Add($"{columnName} >= DATEADD(DAY, 1-DATEPART(WEEKDAY, GETDATE())-7, CAST(GETDATE() AS DATE))");
                            whereClauses.Add($"{columnName} < DATEADD(DAY, 8-DATEPART(WEEKDAY, GETDATE())-7, CAST(GETDATE() AS DATE))");
                            break;

                        case DateFilterType.ThisMonth:
                            whereClauses.Add($"YEAR({columnName}) = YEAR(GETDATE()) AND MONTH({columnName}) = MONTH(GETDATE())");
                            break;

                        case DateFilterType.LastMonth:
                            whereClauses.Add($"YEAR({columnName}) = YEAR(DATEADD(MONTH, -1, GETDATE())) AND MONTH({columnName}) = MONTH(DATEADD(MONTH, -1, GETDATE()))");
                            break;

                        case DateFilterType.ThisYear:
                            whereClauses.Add($"YEAR({columnName}) = YEAR(GETDATE())");
                            break;

                        case DateFilterType.LastYear:
                            whereClauses.Add($"YEAR({columnName}) = YEAR(GETDATE()) - 1");
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

                // Filtros numéricos
                foreach (var filter in filters.NumericFilters)
                {
                    var numericFilter = filter.Value;
                    if (numericFilter.Type != NumericFilterType.None)
                    {
                        var columnName = SanitizeColumnName(filter.Key);

                        switch (numericFilter.Type)
                        {
                            case NumericFilterType.Equal:
                                if (numericFilter.Value1.HasValue)
                                    whereClauses.Add($"{columnName} = {numericFilter.Value1.Value}");
                                break;

                            case NumericFilterType.NotEqual:
                                if (numericFilter.Value1.HasValue)
                                    whereClauses.Add($"{columnName} <> {numericFilter.Value1.Value}");
                                break;

                            case NumericFilterType.GreaterThan:
                                if (numericFilter.Value1.HasValue)
                                    whereClauses.Add($"{columnName} > {numericFilter.Value1.Value}");
                                break;

                            case NumericFilterType.GreaterThanOrEqual:
                                if (numericFilter.Value1.HasValue)
                                    whereClauses.Add($"{columnName} >= {numericFilter.Value1.Value}");
                                break;

                            case NumericFilterType.LessThan:
                                if (numericFilter.Value1.HasValue)
                                    whereClauses.Add($"{columnName} < {numericFilter.Value1.Value}");
                                break;

                            case NumericFilterType.LessThanOrEqual:
                                if (numericFilter.Value1.HasValue)
                                    whereClauses.Add($"{columnName} <= {numericFilter.Value1.Value}");
                                break;

                            case NumericFilterType.Between:
                                if (numericFilter.Value1.HasValue && numericFilter.Value2.HasValue)
                                    whereClauses.Add($"{columnName} BETWEEN {numericFilter.Value1.Value} AND {numericFilter.Value2.Value}");
                                break;
                        }
                    }
                }
            }

            // WHERE (se construye fuera del if(filters) para incluir el filtro de EmpresaId siempre)
            if (whereClauses.Any())
            {
                query.Append(" WHERE ");
                query.Append(string.Join(" AND ", whereClauses));
            }

            // ORDER BY (debe ir después del WHERE)
            if (filters != null && !string.IsNullOrEmpty(filters.SortColumn))
            {
                var columnName = SanitizeColumnName(filters.SortColumn);
                var direction = filters.SortAscending ? "ASC" : "DESC";
                query.Append($" ORDER BY {columnName} {direction}");
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

            // Construir la cláusula WHERE con el filtro de empresa si aplica
            var whereClause = $"{sanitizedColumn} IS NOT NULL";
            if (entidad.FiltrarxEmpresa && _sessionService.EmpresaId.HasValue)
            {
                whereClause += $" AND EmpresaId = {_sessionService.EmpresaId.Value}";
                Console.WriteLine($"🔵 GetDistinctValues - Filtro EmpresaId aplicado: {_sessionService.EmpresaId.Value}");
            }
            else
            {
                Console.WriteLine($"🔴 GetDistinctValues - Filtro EmpresaId NO aplicado - FiltrarxEmpresa: {entidad.FiltrarxEmpresa}, EmpresaId: {_sessionService.EmpresaId}");
            }

            // Primero verificar cuántos valores distintos hay
            var countQuery = $"SELECT COUNT(DISTINCT {sanitizedColumn}) AS DistinctCount FROM {entidad.NombreVista} WHERE {whereClause}";
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
            var query = $"SELECT DISTINCT {sanitizedColumn} FROM {entidad.NombreVista} WHERE {whereClause} ORDER BY {sanitizedColumn}";
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
