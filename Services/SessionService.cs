namespace PredatorWeb.Services
{
    /// <summary>
    /// Servicio para mantener información de sesión del usuario
    /// </summary>
    public class SessionService
    {
        /// <summary>
        /// Cadena de conexión activa para la sesión actual
        /// </summary>
        public string? ActiveConnectionString { get; set; }

        /// <summary>
        /// Indica si el usuario ha iniciado sesión
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// Código de la empresa actual
        /// </summary>
        public string? CompanyCode { get; set; }

        /// <summary>
        /// Nombre de usuario actual
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// ID de la empresa (EmpresaId) obtenido en el login
        /// </summary>
        public int? EmpresaId { get; set; }

        /// <summary>
        /// ID del usuario (UsuarioId) obtenido en el login
        /// </summary>
        public int? UsuarioId { get; set; }
    }
}
