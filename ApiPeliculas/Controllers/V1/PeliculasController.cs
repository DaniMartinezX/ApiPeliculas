using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V1
{
    [Route("api/v{version:apiVersion}/peliculas")]
    [ApiController]
    [ApiVersion("1.0")]
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

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpGet]
        [ResponseCache(CacheProfileName = "Default30")] // Se puede usar un perfil de cacheo definido en Program.cs
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

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
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

            //if (!_pelRepo.CrearPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salió mal guardando el registro {pelicula.Nombre}");
            //    return StatusCode(404, ModelState);
            //}

            // Subida de archivo
            if (crearPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(crearPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot/ImagenesPeliculas/" + nombreArchivo;
                
                string ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    crearPeliculaDto.Imagen.CopyTo(fileStream);
                }


                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;
            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400"; 
            }

            _pelRepo.CrearPelicula(pelicula);
            return CreatedAtRoute("GetPelicula", new { idPelicula = pelicula.Id }, pelicula);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{idPelicula:int}", Name = "ActualizarPatchPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPatchPelicula(int idPelicula, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (actualizarPeliculaDto == null || idPelicula != actualizarPeliculaDto.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_pelRepo.ExistePelicula(idPelicula))
            {
                return NotFound($"No se encontró la película con ID {idPelicula}");
            }

            var pelicula = _mapper.Map<Pelicula>(actualizarPeliculaDto);

            //if (!_pelRepo.ActualizarPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salió mal actualizando el registro {pelicula.Nombre}");
            //    return StatusCode(500, ModelState);
            //}

            if (actualizarPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(actualizarPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot/ImagenesPeliculas/" + nombreArchivo;

                string ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    actualizarPeliculaDto.Imagen.CopyTo(fileStream);
                }


                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;
            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }

            _pelRepo.ActualizarPelicula(pelicula);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
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

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpGet("BuscarPeliculaByCategoria/{idCategoria:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BuscarPeliculaByCategoria(int idCategoria)
        {
            try
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

                if (listaPeliculas == null || !listaPeliculas.Any())
                {
                    return NotFound($"No se encontraron películas en la categoría con ID {idCategoria}.");
                }

                var itemPelicula = listaPeliculas.Select(p => _mapper.Map<PeliculaDto>(p)).ToList();
                //foreach (var pelicula in listaPeliculas)
                //{
                //    itemPelicula.Add(_mapper.Map<PeliculaDto>(pelicula));
                //}

                return Ok(itemPelicula);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al recuperar datos de la aplicación");
            }
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
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
                    return NotFound($"No se encontraron películas con el nombre {nombrePelicula}.");
                }

                var peliculasDto = _mapper.Map<IEnumerable<PeliculaDto>>(resultado);

                return Ok(peliculasDto);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Algo salió mal buscando la película");
            }
        }
    }
}
