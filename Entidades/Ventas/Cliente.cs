namespace PredatorWeb.Entidades.Ventas
{
    public class Cliente : Entidad
    {
        public Cliente()
        {
            NombreEntidad = "Cliente";
            NombreTabla = "VenCliente";
            NombreProcedimiento = "VenClienteMantenimiento";
            NombreVista = "VenClienteVistaLista";
            NombreClavePrimaria = "ClienteId";
            NombrePlural = "Clientes";
            FiltrarxEmpresa = true;
        }
    }
}
