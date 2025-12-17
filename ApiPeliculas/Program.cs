using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMappers;
using ApiPeliculas.Repositorio.IRepositorio;
using ApiPeliculas.Repositorio.Repositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

// Repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:ClaveSecreta");

//AutoMapper
builder.Services.AddAutoMapper(typeof(PeliculaMapper).Assembly);

// Aquí se configura la Autenticación
builder.Services.AddAuthentication
    (
        x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false; // En PRODUCCIÓN es mejor tenerlo en true para una mayor seguridad
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // La clave de la firma del emisor debe ser validado
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), // Establece la clave de la firma del emisor para validar el token
            ValidateIssuer = false, // Si se necesita validar el emisor del token
            ValidateAudience = false // No se valida la audiencia del token en este caso, se puede validar si se desea una audiencia específica.
        };
    });


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// CORS
// Configura CORS para permitir solicitudes desde el frontend
// Se pueden habilitar orígenes específicos según sea necesario
// Aquí se usa el ejèmplo el dominio http://localhost:3223
// Se usa * para permitir cualquier origen (no recomendado para producción)
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build =>
{
    build.WithOrigins("http://localhost:3223")
           .AllowAnyMethod()
           .AllowAnyHeader();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Soporte para CORS
app.UseCors("PoliticaCors");

// Soporte para Autenticación
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
