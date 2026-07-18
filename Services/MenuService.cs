using Microsoft.Data.SqlClient;
using PredatorWeb.Models;

namespace PredatorWeb.Services;

public class MenuService
{
    private readonly Datos _datos;
    private readonly ILogger<MenuService> _logger;

    public MenuService(Datos datos, ILogger<MenuService> logger)
    {
        _datos = datos;
        _logger = logger;

        _logger.LogInformation("MenuService inicializado");
    }

    public async Task<List<GrlMenu>> GetMenuItemsAsync()
    {
        var menuItems = new List<GrlMenu>();

        try
        {
            _logger.LogInformation("Intentando conectar a la base de datos...");

            using (var connection = _datos.CreateConnection())
            {
                await connection.OpenAsync();
                _logger.LogInformation("Conexión establecida exitosamente");

                var query = @"SELECT MenuId, MenuGrupalId, Menu, NombreEntidad, CodigoMenu, Abierto
                             FROM GrlMenu 
                             WHERE MenuGrupalId IS NULL
                             ORDER BY CodigoMenu";

                _logger.LogInformation($"Ejecutando query: {query}");

                using (var reader = await _datos.ExecuteReaderAsync(query, connection))
                {
                    while (await reader.ReadAsync())
                    {
                        var menu = new GrlMenu
                        {
                            MenuId = reader.GetInt32(reader.GetOrdinal("MenuId")),
                            MenuGrupalId = reader.IsDBNull(reader.GetOrdinal("MenuGrupalId")) 
                                ? null 
                                : reader.GetInt32(reader.GetOrdinal("MenuGrupalId")),
                            Menu = reader.GetString(reader.GetOrdinal("Menu")),
                            NombreEntidad = reader.GetString(reader.GetOrdinal("NombreEntidad")),
                            CodigoMenu = reader.GetString(reader.GetOrdinal("CodigoMenu")),
                            Abierto = reader.GetBoolean(reader.GetOrdinal("Abierto"))
                        };

                        menuItems.Add(menu);
                        _logger.LogInformation($"Menu item cargado: {menu.Menu} (Código: {menu.CodigoMenu}, NombreEntidad: {menu.NombreEntidad})");
                    }
                }
            }

            _logger.LogInformation($"Total de items cargados: {menuItems.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar items del menú desde la base de datos");
            throw;
        }

        return menuItems;
    }

    public async Task<List<GrlMenu>> GetSubMenuItemsAsync(int menuGrupalId)
    {
        var subMenuItems = new List<GrlMenu>();

        try
        {
            _logger.LogInformation($"Cargando submenús para MenuGrupalId: {menuGrupalId}");

            using (var connection = _datos.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT MenuId, MenuGrupalId, Menu, NombreEntidad, CodigoMenu, Abierto
                             FROM GrlMenu 
                             WHERE MenuGrupalId = @MenuGrupalId
                             ORDER BY CodigoMenu";

                var parameter = new SqlParameter("@MenuGrupalId", menuGrupalId);

                using (var reader = await _datos.ExecuteReaderAsync(query, connection, parameter))
                {
                    while (await reader.ReadAsync())
                    {
                        var menu = new GrlMenu
                        {
                            MenuId = reader.GetInt32(reader.GetOrdinal("MenuId")),
                            MenuGrupalId = reader.IsDBNull(reader.GetOrdinal("MenuGrupalId")) 
                                ? null 
                                : reader.GetInt32(reader.GetOrdinal("MenuGrupalId")),
                            Menu = reader.GetString(reader.GetOrdinal("Menu")),
                            NombreEntidad = reader.GetString(reader.GetOrdinal("NombreEntidad")),
                            CodigoMenu = reader.GetString(reader.GetOrdinal("CodigoMenu")),
                            Abierto = reader.GetBoolean(reader.GetOrdinal("Abierto"))
                        };

                        subMenuItems.Add(menu);
                        _logger.LogInformation($"Submenu item cargado: {menu.Menu} (Código: {menu.CodigoMenu}, NombreEntidad: {menu.NombreEntidad})");
                    }
                }
            }

            _logger.LogInformation($"Total de subitems cargados: {subMenuItems.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al cargar subitems del menú para MenuGrupalId {menuGrupalId}");
            throw;
        }

        return subMenuItems;
    }
}
