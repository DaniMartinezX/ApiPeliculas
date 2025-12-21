using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.PeliculasMappers;
using ApiPeliculas.Repositorio.IRepositorio;
using ApiPeliculas.Repositorio.Repositorio;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

// Soporte para Caché
// Contenido que se esté actualizando constantemente no es recomendable cachearlo
builder.Services.AddResponseCaching();

// Repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:ClaveSecreta");

// Soporte para versionamiento (paquetes necesarios: Microsoft.Versioning.Mvc.Versioning y ApiExplorer)
var apiVersioningBuilder = builder.Services.AddApiVersioning(opciones =>
{
    opciones.AssumeDefaultVersionWhenUnspecified = true; // Si no se especifica la versión, se usa la versión por defecto
    opciones.DefaultApiVersion = new ApiVersion(1, 0); // Versión por defecto
    opciones.ReportApiVersions = true; // Reporta las versiones disponibles en los encabezados de respuesta
    //opciones.ApiVersionReader = ApiVersionReader.Combine(
    //    new QueryStringApiVersionReader("api-version") // Leer la versión desde la cadena de consulta
    //    //new HeaderApiVersionReader("X-Version"), // Leer la versión desde el encabezado
    //    //new MediaTypeApiVersionReader("ver") // Leer la versión desde el tipo de medio
    //);
});

apiVersioningBuilder.AddApiExplorer(opciones =>
    {
        opciones.GroupNameFormat = "'v'VVV"; // Formato del nombre del grupo de versiones
        opciones.SubstituteApiVersionInUrl = true; // Sustituye la versión en la URL)
    });

//AutoMapper (paq
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), // Establece la clave de la firma del emisor para validar el token
            ValidateIssuer = false, // Si se necesita validar el emisor del token
            ValidateAudience = false, // No se valida la audiencia del token en este caso, se puede validar si se desea una audiencia específica.
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });


// Beneficios del CACHÉ GLOBAL:
// - Centraliza la configuración de caché
// - Facilita el mantenimiento del código
// - Mejora la legibilidad del código
// - Permite reutilizar perfiles de caché en múltiples controladores o acciones
// - Reduce la duplicación de código
// - Facilita la aplicación de políticas de caché consistentes en toda la aplicación
builder.Services.AddControllers(opcion =>
{
    // Caché profile. Un caché global y así no tener que ponerlo en todas partes
    opcion.CacheProfiles.Add("Default30", new CacheProfile()
    {
        Duration = 30
    });
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega SOLO el token JWT (sin 'Bearer ')."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
    });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo))
            return false;

        var versions = methodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .OfType<ApiVersionAttribute>()
            .SelectMany(attr => attr.Versions);

        return versions?.Any(v => $"v{v.MajorVersion}" == docName) == true;
    });

});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();


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
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var desc in provider.ApiVersionDescriptions)
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName);
    });
}

app.UseHttpsRedirection();

// Soporte para CORS
app.UseCors("PoliticaCors");

// Soporte para Autenticación
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
