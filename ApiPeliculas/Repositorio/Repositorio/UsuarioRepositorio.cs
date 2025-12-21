using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XSystem.Security.Cryptography;

namespace ApiPeliculas.Repositorio.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _bd;
        private string claveSecreta;

        public UsuarioRepositorio(ApplicationDbContext bd, IConfiguration config)
        {
            _bd = bd;
            claveSecreta = config.GetValue<string>("ApiSettings:ClaveSecreta"); // Para recoger la clave secreta de appsettings.json
        }

        public Usuario GetUsuarioById(int usuarioId)
        {
            return _bd.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        }

        public ICollection<Usuario> GetUsuarios()
        {
            return _bd.Usuarios.OrderBy(u => u.NombreUsuario).ToList();
        }

        public bool IsUniqueUser(string usuario)
        {
            var usuarioBd = _bd.Usuarios.FirstOrDefault(u => u.NombreUsuario == usuario);

            if (usuarioBd == null)
            {
                return true;
            }

            return false;
        }

        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {
            var usuario = await _bd.Usuarios.FirstOrDefaultAsync(
                u => u.NombreUsuario.ToLower() == usuarioLoginDto.NombreUsuario.ToLower()
            );

            if (usuario == null)
            {
                return new UsuarioLoginRespuestaDto { Token = "", Usuario = null };
            }

            var hasher = new PasswordHasher<Usuario>();
            var result = hasher.VerifyHashedPassword(usuario, usuario.Password, usuarioLoginDto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return new UsuarioLoginRespuestaDto { Token = "", Usuario = null };
            }

            // Si result == SuccessRehashNeeded, podrías re-hashear y guardar (opcional)
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.Password = hasher.HashPassword(usuario, usuarioLoginDto.Password);
                await _bd.SaveChangesAsync();
            }

            // JWT (solo ajusto UTF8 recomendado)
            var manejadoToken = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(claveSecreta); // y claveSecreta >= 32 bytes

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = manejadoToken.CreateToken(tokenDescriptor);

            return new UsuarioLoginRespuestaDto
            {
                Token = manejadoToken.WriteToken(token),
                Usuario = usuario // ideal: devolver DTO sin Password
            };
        }

        public async Task<Usuario> Registro(UsuarioRegistroDto usuarioRegistroDto)
        {
            var hasher = new PasswordHasher<Usuario>();

            Usuario usuario = new Usuario()
            {
                NombreUsuario = usuarioRegistroDto.NombreUsuario,
                Nombre = usuarioRegistroDto.Nombre,
                Role = usuarioRegistroDto.Role
            };

            // Hash seguro + salt (lo gestiona PasswordHasher)
            usuario.Password = hasher.HashPassword(usuario, usuarioRegistroDto.Password);

            _bd.Usuarios.Add(usuario);
            await _bd.SaveChangesAsync();

            // OJO: no devuelvas el hash al cliente en un proyecto real (mejor DTO), LO MAPEO EN EL CONTROLADOR
            return usuario;
        }


        #region Hashing MD5 Inseguro - No usar en producción

        //public async Task<UsuarioLoginRespuestaDto> LoginMD5(UsuarioLoginDto usuarioLoginDto)
        //{
        //    var passwordEncriptado = obtenermd5(usuarioLoginDto.Password);
        //    var usuario = _bd.Usuarios.FirstOrDefault(
        //        u => u.NombreUsuario.ToLower() == usuarioLoginDto.NombreUsuario.ToLower()
        //        && u.Password == passwordEncriptado
        //    );

        //    //Validamos si el usuario no existe con la combinación de usuario y contraseña
        //    if (usuario == null)
        //    {
        //        return new UsuarioLoginRespuestaDto()
        //        { 
        //            Token = "",
        //            Usuario = null
        //        };
        //    }

        //    //Aquí existe el usuario entonces podemos procesar el login
        //    var manejadoToken = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(claveSecreta);

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new Claim[]
        //        {
        //            new Claim(ClaimTypes.Name, usuario.NombreUsuario.ToString()),
        //            new Claim(ClaimTypes.Role, usuario.Role)
        //        }),
        //        Expires = DateTime.UtcNow.AddDays(7),
        //        SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) //Para verificar la palabra secreta del sistema [FIRMA DE TOKEN]
        //    };

        //    var token = manejadoToken.CreateToken(tokenDescriptor);

        //    UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
        //    {
        //        Token = manejadoToken.WriteToken(token),
        //        Usuario = usuario
        //    };

        //    return usuarioLoginRespuestaDto;
        //}
        //public async Task<Usuario> RegistroMD5(UsuarioRegistroDto usuarioRegistroDto)
        //{
        //    var passwordEncriptado = obtenermd5(usuarioRegistroDto.Password);

        //    Usuario usuario = new Usuario()
        //    {
        //        NombreUsuario = usuarioRegistroDto.NombreUsuario,
        //        Nombre = usuarioRegistroDto.Nombre,
        //        Password = passwordEncriptado,
        //        Role = usuarioRegistroDto.Role
        //    };

        //    _bd.Usuarios.Add(usuario);
        //    await _bd.SaveChangesAsync();
        //    usuario.Password = passwordEncriptado;
        //    return usuario;
        //}

        // Método para encriptar constraseña con MD5 se usa tanto en el Acceso como en el Registro
        //public static string obtenermd5(string valor)
        //{
        //    MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
        //    byte[] data = System.Text.Encoding.UTF8.GetBytes(valor);
        //    data = x.ComputeHash(data);
        //    string resp = "";
        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        resp += data[i].ToString("x2").ToLower();
        //    }
        //    return resp;
        //}
        #endregion
    }
}
