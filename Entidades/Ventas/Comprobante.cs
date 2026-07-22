namespace PredatorWeb.Entidades.Ventas
{
    public class ComprobanteVenta : Entidad
    {
        public ComprobanteVenta()
        {
            NombreEntidad = "ComprobanteVenta";
            NombreTabla ="VenComprobante";
            NombreProcedimiento="VenComprobantesMantenimimento";
            NombreVista= "VenComprobanteVistaLista";
            NombreClavePrimaria= "ComprobanteId";
            NombrePlural = "Comprobantes";
            FiltrarxEmpresa = true;
        }
    }
}
