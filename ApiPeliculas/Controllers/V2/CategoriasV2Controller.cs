using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V2
{
    //[Authorize]
    //[Authorize(Roles = "Admin")]
    //[ResponseCache(Duration = 60)] // Se cachean las respuestas del controlador por 60 segundos, también se puede aplicar a nivel de método
    [Route("api/v{version:apiVersion}/categorias")]
    [ApiController]
    [ApiVersion("2.0")]
    public class CategoriasV2Controller : ControllerBase
    {
        private readonly ICategoriaRepositorio _ctRepo;
        private readonly IMapper _mapper;

        public CategoriasV2Controller(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IEnumerable<string> GetCategorias() { 
            return new string[] { "Categoría V2 - 1", "Categoría V2 - 2" };
        }
    }
}
