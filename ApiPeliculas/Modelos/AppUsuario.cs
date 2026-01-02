using Microsoft.AspNetCore.Identity;

namespace ApiPeliculas.Modelos
{
    /// <summary>
    /// Este modelo extiende de IdentityUser para agregar propiedades adicionales al usuario.
    /// </summary>
    public class AppUsuario : IdentityUser
    {
        public string Nombre { get; set; }
    }
}
