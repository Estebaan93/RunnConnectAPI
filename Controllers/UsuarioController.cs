//Controllers/UsuarioController
using Microsoft.AspNetCore.Authorization; //Para Authorize y AllowAnonymous
using Microsoft.AspNetCore.Mvc; //Para controllerBase, IActionResult, [HTTPGet]
using RunnConnectAPI.Models.Dto.Usuario;
using RunnConnectAPI.Models;
using RunnConnectAPI.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;
using RunnConnectAPI.Services;

namespace RunnConnectAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize] //Todos los endpoint requieren autenticacion JWT
  public class UsuarioController : ControllerBase
  {
    private readonly UsuarioRepositorio _usuarioRepositorio;
    private readonly JWTService _jwtService;
    private readonly PasswordService _passwordService;

    public UsuarioController(UsuarioRepositorio usuarioRepositorio, JWTService jwtService, PasswordService passwordService)
    {
      _usuarioRepositorio = usuarioRepositorio;
      _jwtService = jwtService;
      _passwordService = passwordService;
    }

    /*Autenticacion (Login y Registro)
    POST: api/usuario/login*/
    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
      //Validamos el modelo DataAnnotations
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      //Buscar por email
      var usuario = await _usuarioRepositorio.GetByEmailAsync(loginRequestDto.Email);

      if (usuario == null)
      {
        return Unauthorized(new { message = "Email o contraseña incorrecta" });
      }

      //Verificamos la contraseña
      var passwordValido = _passwordService.VerifyPassword(loginRequestDto.Password, usuario.PasswordHash);

      if (!passwordValido)
      {
        return Unauthorized(new { mesage = "Email o contraseña incorrectos" });

      }

      //Generar token
      var token = _jwtService.GenerarToken(usuario);

      //Retornar token
      return Ok(new
      {
        token,
        message = "Login exitoso",
        usuario = new
        {
          idUsuario = usuario.IdUsuario,
          nombre = usuario.Nombre,
          apellido = usuario.Apellido,
          email = usuario.Email,
          tipoUsuario = usuario.TipoUsuario
        }
      });
    }

  /*Registro de nuevo usuarios y retorna un JWT para login automatico
  POST: api/usuarios/registro*/
  [AllowAnonymous]
  [HttpPost("Register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
    {
      //Validamos el modelo
      if(!ModelState.IsValid)
        return BadRequest(ModelState);


      //Validar que el email exista (reutilzmos el metod que ya tenemos)
      var emailExistente= await _usuarioRepositorio.EmailExistenteAsync(registerRequestDto.Email);
      if (emailExistente)
      {
        return BadRequest(new {message="El email ya esta registrado"});
      }

      //Validar el DNI no exista
      var dniExiste= await _usuarioRepositorio.DniExisteAsync(registerRequestDto.Dni);
      if (dniExiste)
      {
        return BadRequest(new {message="El DNI ya esta registrado"});
      }

      //Crear obj usuario
      var usuario= new Usuario
            {
                Nombre = registerRequestDto.Nombre,
                Apellido = registerRequestDto.Apellido,
                Email = registerRequestDto.Email,
                Telefono = registerRequestDto.Telefono,
                Dni = registerRequestDto.Dni,
                TipoUsuario = registerRequestDto.TipoUsuario,
                FechaNacimiento = registerRequestDto.FechaNacimiento,
                Genero = registerRequestDto.Genero,
                Localidad = registerRequestDto.Localidad,
                Agrupacion = registerRequestDto.Agrupacion,
                TelefonoEmergencia = registerRequestDto.TelefonoEmergencia,
                Password = _passwordService.HashPassword(registerRequestDto.Password), // Hashear password
                Estado = true // Usuario activo por defecto
            };





    }





    /*GET- Obtener perfil*/
    [HttpGet("perfil")]
    public async Task<IActionResult> ObtenerPerfil()
    {
      //Obtenemos el Id del usuario desde el token JWT
      var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userClaim.Value);

      //Buscamos el usuario en la BD (Solo los activos)
      var usuario = await _usuarioRepositorio.GetByIdAsync(userId);

      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      //Mapeamos a obj anonimo (sin Pass ni Estado)
      var usuarioResponse = new
      {
        idUsuario = usuario.IdUsuario,
        nombre = usuario.Nombre,
        apellido = usuario.Apellido,
        email = usuario.Email,
        telefono = usuario.Telefono,
        tipoUsuario = usuario.TipoUsuario,
        fechaNacimiento = usuario.FechaNacimiento,
        genero = usuario.Genero,
        dni = usuario.Dni,
        localidad = usuario.Localidad,
        agrupacion = usuario.Agrupacion,
        telefonoEmergencia = usuario.TelefonoEmergencia
      };
      return Ok(usuarioResponse);
    }


    //PUT- actualizar perfil de un usuario 
    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto request)
    {
      // Validar el modelo segun las Data Annotations del DTO
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // Obtener ID del usuario autenticado desde el token
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userIdClaim.Value);

      // Buscar el usuario en la BD
      var usuario = await _usuarioRepositorio.GetByIdAsync(userId);

      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      // Actualizar campos (Nombre es requerido y telefono, siempre viene)
      usuario.Nombre = request.Nombre;
      usuario.Telefono = request.Telefono;

      // Campos opcionales
      if (request.Apellido != null)
        usuario.Apellido = request.Apellido;

      if (request.FechaNacimiento.HasValue)
        usuario.FechaNacimiento = request.FechaNacimiento;

      if (!string.IsNullOrEmpty(request.Genero))
        usuario.Genero = request.Genero;

      if (!string.IsNullOrEmpty(request.Localidad))
        usuario.Localidad = request.Localidad;

      if (!string.IsNullOrEmpty(request.Agrupacion))
        usuario.Agrupacion = request.Agrupacion;

      if (!string.IsNullOrEmpty(request.TelefonoEmergencia))
        usuario.TelefonoEmergencia = request.TelefonoEmergencia;

      // Guardar cambios en la BD
      await _usuarioRepositorio.UpdateAsync(usuario);

      // Retornar usuario actualizado
      return Ok(new
      {
        message = "Perfil actualizado exitosamente",
        usuario = new
        {
          idUsuario = usuario.IdUsuario,
          nombre = usuario.Nombre,
          apellido = usuario.Apellido,
          email = usuario.Email,
          telefono = usuario.Telefono,
          tipoUsuario = usuario.TipoUsuario,
          fechaNacimiento = usuario.FechaNacimiento,
          genero = usuario.Genero,
          dni = usuario.Dni,
          localidad = usuario.Localidad,
          agrupacion = usuario.Agrupacion,
          telefonoEmergencia = usuario.TelefonoEmergencia
        }
      });
    }


    //verificar email antes de registrar
    //GET - api/usuarios/verificarEmail
    [AllowAnonymous]
    [HttpGet("verificarEmail")]
    public async Task<IActionResult> VerificarEmail([FromQuery] string email)
    {
      //Validar que el email no este vacio
      if (string.IsNullOrEmpty(email))
        return BadRequest(new { message = "Email requerido" });

      //Verificar si existe un usuario activo con ese email
      var existe = await _usuarioRepositorio.EmailExistenteAsync(email);

      //Renornar disponibilidad
      return Ok(new
      {
        disponible = !existe,
        message = existe ? "Email ya registrado" : "Email disponible"
      });
    }

    //verificar dni si esta disponible antes de registrase
    //GET - api/usuarios/verificarDni
    [AllowAnonymous]
    [HttpGet("verificarDni")]
    public async Task<IActionResult> VerificarDni([FromQuery] int dni)
    {
      //Validar que el Dni sea valido mayor a 0
      if (dni <= 0)
        return BadRequest(new { message = "DNI invalido" });

      //Verificar si existe ese Dni en la BD
      var existe = await _usuarioRepositorio.DniExisteAsync(dni);

      //Retornamos si esta disponible
      return Ok(new
      {
        disponible = !existe,
        message = existe ? "Dni ya resgistrado" : "Dni disponible"
      });

    }

    /*ver perfil publico (por ej si un runner consulta la posicion, puede ver quienes son los otros runner)
    tambien ver el perfil de un organizador - conlleva a mostrar una vista personalizada por los datos sensibles*/
    //GET - api/usuarios/{id}
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerUsuarioPorId(int id)
    {
      //Buscar usuario por id (que estan activos)
      var usuario = await _usuarioRepositorio.GetByIdAsync(id);

      //Sino existe o esta eliminado
      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      //Retornamos solo informacion publica
      return Ok(new
      {
        idUsuario = usuario.IdUsuario,
        nombre = usuario.Nombre,
        apellido = usuario.Apellido,
        tipoUsuario = usuario.TipoUsuario,
        localidad = usuario.Localidad,
        agrupacion = usuario.Agrupacion
      });
    }

    //eliminiacion logica cambia estado
    //DELETE - /api/usuarios/perfil
    /*Quien puede eliminar usuarios?*/
    [Authorize] //Requiere estar autenticado
    [HttpDelete("perfil")]
    public async Task<IActionResult> EliminarCuenta()
    {
      //Obtenemos el id del usuario desde el token
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userIdClaim.Value);

      //Buscamos el usuario en la BD
      var usuario = await _usuarioRepositorio.GetByIdAsync(userId);

      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      //Eliminado logico su estado pasa a falso
      await _usuarioRepositorio.DeleteLogicoAsync(usuario);

      return Ok(new { message = "Cuenta eliminada exitosamente" });
    }



  }
}