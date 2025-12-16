namespace ApiPeliculas.Modelos.Dtos
{
    public class UsuarioLoginRespuestaDto
    {
        public Usuario Usuario { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
