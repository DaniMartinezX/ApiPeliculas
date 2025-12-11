using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _pelRepo;
        private readonly ICategoriaRepositorio _catRepo;
        private readonly IMapper _mapper;

        public PeliculasController(IPeliculaRepositorio pelRepo, ICategoriaRepositorio catRepo, IMapper mapper)
        {
            _pelRepo = pelRepo;
            _catRepo = catRepo;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetPeliculas()
        {
            var listaPeliculas = _pelRepo.GetPeliculas();

            var listaPeliculasDto = new List<PeliculaDto>();

            foreach (var pelicula in listaPeliculas)
            {
                listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(pelicula));
            }

            return Ok(listaPeliculasDto);
        }

        [HttpGet("{idPelicula:int}", Name = "GetPelicula")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPelicula(int idPelicula)
        {
            var itemPelicula = _pelRepo.GetPelicula(idPelicula);

            if (itemPelicula == null)
            {
                return NotFound();
            }

            var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);

            return Ok(itemPeliculaDto);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CrearPelicula([FromBody] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearPeliculaDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_pelRepo.ExistePelicula(crearPeliculaDto.Nombre))
            {
                ModelState.AddModelError("", "La película ya existe");
                return StatusCode(404, ModelState);
            }

            var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);

            if (!_pelRepo.CrearPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salió mal guardando el registro {pelicula.Nombre}");
                return StatusCode(404, ModelState);
            }

            return CreatedAtRoute("GetPelicula", new { idPelicula = pelicula.Id }, pelicula);
        }

        [HttpPatch("{idPelicula:int}", Name = "ActualizarPatchPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPatchPelicula(int idPelicula, [FromBody] PeliculaDto peliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (peliculaDto == null || idPelicula != peliculaDto.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_pelRepo.ExistePelicula(idPelicula))
            {
                return NotFound($"No se encontró la película con ID {idPelicula}");
            }

            var pelicula = _mapper.Map<Pelicula>(peliculaDto);

            if (!_pelRepo.ActualizarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salió mal actualizando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{idPelicula:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult BorrarPelicula(int idPelicula)
        {
            if (!_pelRepo.ExistePelicula(idPelicula))
            {
                return NotFound();
            }
            var pelicula = _pelRepo.GetPelicula(idPelicula);

            if (!_pelRepo.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salió mal borrando el registro {pelicula.Nombre}");
                return StatusCode(404, ModelState);
            }

            return NoContent();
        }

        [HttpGet("BuscarPeliculaByCategoria/{idCategoria:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BuscarPeliculaByCategoria(int idCategoria)
        {
            if (idCategoria <= 0)
            {
                return BadRequest();
            }

            if (!_catRepo.ExisteCategoria(idCategoria))
            {
                return NotFound();
            }

            var listaPeliculas = _pelRepo.GetPeliculasEnCategoria(idCategoria);

            if (listaPeliculas == null || listaPeliculas.Count == 0)
            {
                return NotFound();
            }

            var listAuxPeliculas = new List<PeliculaDto>();
            foreach (var pelicula in listaPeliculas)
            {
                listAuxPeliculas.Add(_mapper.Map<PeliculaDto>(pelicula));
            }

            return Ok(listAuxPeliculas);
        }

        [HttpGet("BuscarPelicula")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BuscarPelicula(string nombrePelicula)
        {
            try
            {
                var resultado = _pelRepo.BuscarPelicula(nombrePelicula);

                if (resultado == null || resultado.Count() == 0)
                {
                    return NotFound();
                }

                var listaPeliculasDto = new List<PeliculaDto>();

                foreach (var pelicula in resultado)
                {
                    listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(pelicula));
                }

                return Ok(listaPeliculasDto);
            }
            catch (Exception)
            {
                return StatusCode(500, "Algo salió mal buscando la película");
            }
        }
    }
}
