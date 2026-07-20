using Microsoft.Data.SqlClient;
using PredatorWeb.Models;

namespace PredatorWeb.Services;

public class MenuService
{
    private readonly Datos _datos;

    public MenuService(Datos datos)
    {
        _datos = datos;
    }

    public async Task<List<GrlMenu>> GetMenuItemsAsync()
    {
        var menuItems = new List<GrlMenu>();

        try
        {
            using (var connection = _datos.CreateConnection())
            {
                await connection.OpenAsync();

                var query = @"SELECT MenuId, MenuGrupalId, Menu, NombreEntidad, CodigoMenu, Abierto
                             FROM GrlMenu 
                             WHERE MenuGrupalId IS NULL
                             ORDER BY CodigoMenu";

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
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }

        return menuItems;
    }

    public async Task<List<GrlMenu>> GetSubMenuItemsAsync(int menuGrupalId)
    {
        var subMenuItems = new List<GrlMenu>();

        try
        {
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
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }

        return subMenuItems;
    }
}
