using Microsoft.Data.SqlClient;
using PredatorWeb.Models;

namespace PredatorWeb.Services;

public class MenuService
{
    private readonly string _connectionString;
    private readonly ILogger<MenuService> _logger;

    public MenuService(IConfiguration configuration, ILogger<MenuService> logger)
    {
        _connectionString = configuration.GetConnectionString("DLK") 
            ?? "Data Source=caleb.pe;Initial Catalog=DLK;User Id=sagesnet;Password=D0br@nOc;TrustServerCertificate=True;Encrypt=True;";
        _logger = logger;

        _logger.LogInformation($"MenuService inicializado con connection string: {MaskConnectionString(_connectionString)}");
    }

    public async Task<List<GrlMenu>> GetMenuItemsAsync()
    {
        var menuItems = new List<GrlMenu>();

        try
        {
            _logger.LogInformation("Intentando conectar a la base de datos...");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                _logger.LogInformation("Conexión establecida exitosamente");

                var query = @"SELECT MenuId, MenuGrupalId, Menu, NombreEntidad, CodigoMenu, Abierto
                             FROM GrlMenu 
                             WHERE MenuGrupalId IS NULL
                             ORDER BY CodigoMenu";

                _logger.LogInformation($"Ejecutando query: {query}");

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
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
                            _logger.LogInformation($"Menu item cargado: {menu.Menu} (Código: {menu.CodigoMenu})");
                        }
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

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT MenuId, MenuGrupalId, Menu, NombreEntidad, CodigoMenu, Abierto
                             FROM GrlMenu 
                             WHERE MenuGrupalId = @MenuGrupalId
                             ORDER BY CodigoMenu";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuGrupalId", menuGrupalId);

                    using (var reader = await command.ExecuteReaderAsync())
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
                            _logger.LogInformation($"Submenu item cargado: {menu.Menu} (Código: {menu.CodigoMenu})");
                        }
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

    private string MaskConnectionString(string connectionString)
    {
        // Ocultar la contraseña para el log
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"Password=([^;]+)", 
            "Password=***");
    }
}
