namespace PredatorWeb.Entidades
{
    public enum TipoEntidad
    {
        Maestro,
        Documento
    }

    public class Entidad
    {
        public string NombreEntidad { get; set; } = "";
        public string NombreTabla { get; set; } = "";
        public string NombreVista { get; set; } = "";
        public string NombreProcedimiento { get; set; } = "";
        public string NombreClavePrimaria { get; set; } = "";
        public string NombrePlural { get; set; } = "";
        public TipoEntidad Tipo { get; set; } = TipoEntidad.Maestro;
        public bool FiltrarxEmpresa { get; set; } = false;
        
    }
}
