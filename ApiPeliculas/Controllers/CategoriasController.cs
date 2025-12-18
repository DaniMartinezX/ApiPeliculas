using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    //[Authorize]
    //[Authorize(Roles = "Admin")]
    [ResponseCache(Duration = 60)] // Se cachean las respuestas del controlador por 60 segundos, también se puede aplicar a nivel de método
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _ctRepo;
        private readonly IMapper _mapper;

        public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[EnableCors("PoliticaCors")] // Aplica la política CORS a este método en específico, también puede se a nivel de controlador
        public IActionResult GetCategorias()
        {
            var listaCategorias = _ctRepo.GetCategorias();

            var listaCategoriasDto = new List<CategoriaDto>();

            foreach (var categoria in listaCategorias)
            {
                listaCategoriasDto.Add(_mapper.Map<CategoriaDto>(categoria));
            }

            return Ok(listaCategoriasDto);
        }

        [HttpGet("{idCategoria:int}", Name = "GetCategoria")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] // No cachear la respuesta de este método
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCategoria(int idCategoria)
        {
            var itemCategoria = _ctRepo.GetCategoria(idCategoria);

            if (itemCategoria == null)
            {
                return NotFound();
            }

            var itemCategoriaDto = _mapper.Map<CategoriaDto>(itemCategoria);

            return Ok(itemCategoriaDto);
        }

        [Authorize(Roles = "Admin")] // No se debe que dejen crear categoría cualquiera persona
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CrearCategoria([FromBody] CrearCategoriaDto crearCategoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearCategoriaDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_ctRepo.ExisteCategoria(crearCategoriaDto.Nombre))
            {
                ModelState.AddModelError("", "La categoría ya existe");
                return StatusCode(404, ModelState);
            }

            var categoria = _mapper.Map<Categoria>(crearCategoriaDto);

            if (!_ctRepo.CrearCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal guardando el registro {categoria.Nombre}");
                return StatusCode(404, ModelState);
            }

            return CreatedAtRoute("GetCategoria", new { idCategoria = categoria.Id }, categoria);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{idCategoria:int}", Name = "BorrarCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult BorrarCategoria(int idCategoria)
        {
            if (!_ctRepo.ExisteCategoria(idCategoria))
            {
                return NotFound();
            }
            var categoria = _ctRepo.GetCategoria(idCategoria);

            if (!_ctRepo.BorrarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal borrando el registro {categoria.Nombre}");
                return StatusCode(404, ModelState);
            } 

            return NoContent();
        }


        [Authorize(Roles = "Admin")]
        [HttpPatch("{idCategoria:int}", Name = "ActualizarPatchCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPatchCategoria(int idCategoria, [FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || idCategoria != categoriaDto.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_ctRepo.ExisteCategoria(idCategoria))
            {
                return NotFound($"No se encontró la categoría con ID {idCategoria}");
            }

            var categoria = _mapper.Map<Categoria>(categoriaDto);

            if (!_ctRepo.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal actualizando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //[AllowAnonymous] EN CASO DE QUE LA CLASE ENTERA ESTÉ PROTEGIDA, PARA PERMITIR EL ACCESO SÓLO A ESTE MÉTODO
        [Authorize(Roles = "Admin")]
        [HttpPut("{idCategoria:int}", Name = "ActualizarPutCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPutCategoria(int idCategoria, [FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || idCategoria != categoriaDto.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_ctRepo.ExisteCategoria(idCategoria))
            {
                return NotFound($"No se encontró la categoría con ID {idCategoria}");
            }

            var categoria = _mapper.Map<Categoria>(categoriaDto);

            if (!_ctRepo.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal actualizando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}
