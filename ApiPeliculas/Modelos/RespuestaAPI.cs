using System.Net;

namespace ApiPeliculas.Modelos
{
    /// <summary>
    /// Modelo que se crea para poder acceder a las propiedades de la respuesta de la API
    /// </summary>
    public class RespuestaAPI
    {
        public RespuestaAPI()
        {
            ErrorMessages = new List<string>();
        }

        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }

        public object Result { get; set; }
    }
}
