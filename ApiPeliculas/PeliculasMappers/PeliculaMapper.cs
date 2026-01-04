using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using AutoMapper;

namespace ApiPeliculas.PeliculasMappers
{
    public class PeliculaMapper : Profile
    {
        public PeliculaMapper()
        {
            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<Categoria, CrearCategoriaDto>().ReverseMap();
            CreateMap<Pelicula, PeliculaDto>().ReverseMap();
            CreateMap<Pelicula, CrearPeliculaDto>().ReverseMap();
            CreateMap<Pelicula, ActualizarPeliculaDto>().ReverseMap();

            CreateMap<AppUsuario, UsuarioDatosDto>().ReverseMap();
            CreateMap<AppUsuario, UsuarioDto>().ReverseMap();
        }
    }
}
