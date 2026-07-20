namespace PredatorWeb.Entidades.Ventas
{
    public class Cotizacion : Entidad
    {
        public Cotizacion()
        {
            NombreEntidad = "Cotizacion";
            NombreTabla ="VenCotizacion";
            NombreProcedimiento="VenCotizacionMantenimimento";
            NombreVista= "VenCotizacionVistaLista";
            NombreClavePrimaria= "CotizacionId";
            NombrePlural = "Cotizaciones";
            Tipo = TipoEntidad.Maestro;
        }
    }
}
