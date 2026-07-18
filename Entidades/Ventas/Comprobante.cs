namespace PredatorWeb.Entidades.Ventas
{
    public class ComprobanteVenta : Entidad
    {
        public ComprobanteVenta()
        {
            NombreEntidad = "Comprobante";
            NombreTabla ="VenComprobante";
            NombreProcedimiento="VenComprobantesMantenimimento";
            NombreVista= "VenComprobanteVistaLista";
            NombreClavePrimaria= "ComprobanteId";
            Tipo = TipoEntidad.Maestro;
        }
    }
}
