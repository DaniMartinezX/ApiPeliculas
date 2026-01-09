# 🎬 Peliculas API – ASP.NET Core (.NET 10)

API REST para la gestión de películas y categorías, desarrollada con **ASP.NET Core (.NET 10)** aplicando buenas prácticas de diseño backend, seguridad y despliegue en la nube.

Este proyecto forma parte de un **proyecto personal** enfocado en consolidar conocimientos reales de desarrollo profesional con .NET moderno y Azure.

---

## 📌 Características principales

- API RESTful con endpoints CRUD
- Versionado de API (`v1`)
- Autenticación y autorización mediante **JWT**
- Gestión de usuarios y roles con **ASP.NET Identity**
- Persistencia de datos con **Entity Framework Core (Code First)**
- Documentación interactiva con **Swagger / OpenAPI**
- Despliegue en **Azure App Service**
- Base de datos en **Azure SQL Database**

---

## 🧱 Arquitectura y buenas prácticas

- Separación de responsabilidades (Controllers / Repositorios / DTOs)
- Uso de **DTO Pattern** y **AutoMapper**
- Inyección de dependencias
- Control de acceso por roles
- Configuración de CORS
- Caché de respuestas
- Configuración por entornos (Development / Production)

---

## 🛠️ Tecnologías utilizadas

- ASP.NET Core (.NET 10)
- Entity Framework Core (Code First)
- ASP.NET Identity
- JWT Authentication & Authorization
- Swagger / OpenAPI
- API Versioning
- AutoMapper
- Azure App Service
- Azure SQL Database
- CORS & Response Caching

---

## 🔐 Autenticación y autorización

La API utiliza **JWT** para proteger los endpoints sensibles.

### Acceso a endpoints
- **GET**: acceso público
- **POST / PUT / PATCH / DELETE**: requieren autenticación JWT

### Flujo para probar endpoints protegidos
1. Registrarse mediante el endpoint de **registro** (endpoints neutrales), asignando el rol **Admin**.
2. Obtener un token JWT usando el endpoint de **login**.
3. En Swagger, pulsar **Authorize** e introducir el token con el formato directo:

`{token}`

> ⚠️ Nota: La asignación del rol **Admin** durante el registro está habilitada únicamente con fines demostrativos.

---

## 📖 Documentación (Swagger)

La API está documentada con **Swagger**, que permite explorar y probar los endpoints directamente desde el navegador.

- En entorno de desarrollo: `/swagger`
- En producción: Swagger se muestra en la raíz de la aplicación

---

## ☁️ Despliegue en Azure

El proyecto está desplegado en:
- **Azure App Service** (Windows)
- **Azure SQL Database**

La configuración sensible (connection strings, claves JWT, etc.) se gestiona mediante **Application Settings** en Azure.

> La aplicación puede tardar unos segundos en responder si ha estado inactiva (cold start).

---

## 🚀 Ejecución en local

### Requisitos
- .NET SDK 10  
- SQL Server (local o Azure)  
- Visual Studio / VS Code  

### Pasos

1. Clonar el repositorio:
```bash
git clone https://github.com/danimartinezx/ApiPeliculas.git
```

2. Configurar la cadena de conexión y la clave JWT en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ConexionSql": "..."
  },
  "ApiSettings": {
    "ClaveSecreta": "..."
  }
}
```

3. Aplicar migraciones:
```bash
dotnet ef database update
```

4. Ejecutar la aplicación:
```bash
dotnet run
```

---

## 👨‍💻 Autor

**Daniel Martínez Carreira**

- GitHub: https://github.com/danimartinezx  
- LinkedIn: https://www.linkedin.com/in/danielmartinezcarreira/  
- Email: danielmartinezcarreira@gmail.com  

---

## 📄 Licencia

Este proyecto se publica con fines educativos y demostrativos.
