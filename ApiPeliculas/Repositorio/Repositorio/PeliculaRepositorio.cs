using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Repositorio.Repositorio
{
    public class PeliculaRepositorio : IPeliculaRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public PeliculaRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;

            var peliculaExistente = _bd.Peliculas.FirstOrDefault(c => c.Id == pelicula.Id);

            // Arreglo del problema del PUT
            if (peliculaExistente != null)
            {
                _bd.Entry(peliculaExistente).CurrentValues.SetValues(pelicula);
            }
            else
            {
                _bd.Peliculas.Update(pelicula);
            }

            return Guardar();
        }

        public bool BorrarPelicula(Pelicula pelicula)
        {
            _bd.Peliculas.Remove(pelicula);
            return Guardar();
        }

        public IEnumerable<Pelicula> BuscarPelicula(string nombre)
        {
            IQueryable<Pelicula> query = _bd.Peliculas;

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(e => e.Nombre.ToLower().Trim().Contains(nombre.ToLower().Trim()) || e.Descripcion.ToLower().Trim().Contains(nombre.ToLower().Trim()));
            }

            return query.ToList();
        }

        public bool CrearPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            _bd.Peliculas.Add(pelicula);
            return Guardar();
        }

        public bool ExistePelicula(int id)
        {
            return _bd.Peliculas.Any(c => c.Id == id);
        }

        public bool ExistePelicula(string nombre)
        {
            bool valor = _bd.Peliculas.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public Pelicula GetPelicula(int peliculaId)
        {
            return _bd.Peliculas.FirstOrDefault(c => c.Id == peliculaId);
        }

        // V1
        public ICollection<Pelicula> GetPeliculas()
        {
            return _bd.Peliculas.OrderBy(c => c.Nombre).ToList();
        }

        // V2
        public ICollection<Pelicula> GetPeliculasPaginadas(int pageNumber, int pageSize)
        {
            return _bd.Peliculas.OrderBy(c => c.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public ICollection<Pelicula> GetPeliculasEnCategoria(int catId)
        {
            return _bd.Peliculas.Include(ca => ca.Categoria).Where(ca => ca.CategoriaId == catId).ToList();
        }

        public int GetTotalPeliculas()
        {
            return _bd.Peliculas.Count();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0;
        }
    }
}
