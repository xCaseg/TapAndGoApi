namespace TapAndGo.Api.Models.Dto
{
    public class ActualizarUsuarioDto
    {
        public string Email { get; set; }
        public string Rol { get; set; }
        public string? Name { get; set; } // opcional
        public string? PasswordHash { get; set; } // opcional
    }
}
