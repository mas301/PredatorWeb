namespace PredatorWeb.Entidades.Ventas
{
    public class NotaPedido : Entidad
    {
        public NotaPedido()
        {
            NombreEntidad = "NotaPedido";
            NombreTabla ="VenNotaPedido";
            NombreProcedimiento="VenNotaPedidoMantenimimento";
            NombreVista= "VenNotaPedidoVistaLista";
            NombreClavePrimaria= "NotaPedidoId";
            NombrePlural = "Pedidos de Venta";
            Tipo = TipoEntidad.Maestro;
        }
    }
}
