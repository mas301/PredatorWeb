namespace PredatorWeb.Entidades.Ventas
{
    public class GuiaRemision : Entidad
    {
        public GuiaRemision()
        {
            NombreEntidad = "GuiaRemision";
            NombreTabla ="VenGuiaRemision";
            NombreProcedimiento="VenGuiaRemisionMantenimimento";
            NombreVista= "VenGuiaRemisionVistaLista";
            NombreClavePrimaria= "GuiaRemisionId";
            NombrePlural = "Guias de Remisión";
            Tipo = TipoEntidad.Maestro;
        }
    }
}
