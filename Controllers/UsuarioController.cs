//Controllers/UsuarioController
using Microsoft.AspNetCore.Authorization; //Para Authorize y AllowAnonymous
using Microsoft.AspNetCore.Mvc; //Para controllerBase, IActionResult, [HTTPGet]
using RunnConnectAPI.Models.Dto.Usuario;
using RunnConnectAPI.Models;
using RunnConnectAPI.Repositories;
using System.Security.Claims;
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

    /*Autenticacion (Login comun para ambos RUNNER/ORGAN)
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

    /*Registro de nuevo usuarios RUNNERs y retorna un JWT para login automatico
    POST: api/usuarios/RegisterRunner*/
    [AllowAnonymous]
    [HttpPost("RegisterRunner")]
    public async Task<IActionResult> Register([FromBody] RegisterRunnerDto dto)
    {
     // 1. Validar modelo
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // 2. Validar que el email no exista
      var emailExiste = await _usuarioRepositorio.EmailExistenteAsync(dto.Email);
      if (emailExiste)
      {
        return BadRequest(new { message = "El email ya esta registrado" });
      }

      // 3. Validar que el DNI no exista
      var dniExiste = await _usuarioRepositorio.DniExisteAsync(dto.Dni);
      if (dniExiste)
      {
        return BadRequest(new { message = "El DNI ya esta registrado" });
      }

      // 4. Crear usuario Runner
      var usuario = new Usuario
      {
        Nombre = dto.Nombre,
        Apellido = dto.Apellido,
        Email = dto.Email,
        Telefono = dto.Telefono,
        Dni = dto.Dni, // Requerido para runners
        TipoUsuario = "runner", // Forzado a "runner"
        FechaNacimiento = dto.FechaNacimiento,
        Genero = dto.Genero,
        Localidad = dto.Localidad,
        Agrupacion = dto.Agrupacion,
        TelefonoEmergencia = dto.TelefonoEmergencia,
        PasswordHash = _passwordService.HashPassword(dto.Password),
        Estado = true
      };

      // 5. Guardar en BD
      await _usuarioRepositorio.CreateAsync(usuario);

      // 6. Generar token JWT para login automático
      var token = _jwtService.GenerarToken(usuario);

      // 7. Retornar respuesta
      return Ok(new
      {
        token,
        message = "Runner registrado correctamente",
        usuario = new
        {
          idUsuario = usuario.IdUsuario,
          nombre = usuario.Nombre,
          apellido = usuario.Apellido,
          email = usuario.Email,
          tipoUsuario = usuario.TipoUsuario,
          dni = usuario.Dni
        }
      }); 
    }


    /*Registro de nuevos ORGANIZADORES y retorna el token
    POST_ api/usuario/RegisterOrganizador */    
    [AllowAnonymous]
    [HttpPost("RegisterOrganizador")]
    public async Task<IActionResult> RegisterOrganizador([FromBody] RegisterOrganizadorDto dto)
    {
      // 1. Validar modelo
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // 2. Validar que el email no exista
      var emailExiste = await _usuarioRepositorio.EmailExistenteAsync(dto.Email);
      if (emailExiste)
      {
        return BadRequest(new { message = "El email ya esta registrado" });
      }

      // 3. Crear usuario Organizador
      var usuario = new Usuario
      {
        Nombre = dto.Nombre,
        Apellido = dto.Apellido,
        Email = dto.Email,
        Telefono = dto.Telefono,
        Dni = null, // NULL para organizadores
        TipoUsuario = "organizador", // Forzado a "organizador"
        FechaNacimiento = null, // NULL para organizadores
        Genero = null, // NULL para organizadores
        Localidad = dto.Localidad,
        Agrupacion = null, // NULL para organizadores
        TelefonoEmergencia = null, // NULL para organizadores
        PasswordHash = _passwordService.HashPassword(dto.Password),
        Estado = true
      };

      // 4. Guardar en BD
      await _usuarioRepositorio.CreateAsync(usuario);

      // 5. Generar token JWT para login automatico
      var token = _jwtService.GenerarToken(usuario);

      // 6. Retornar respuesta
      return Ok(new
      {
        token,
        message = "Organizador registrado correctamente",
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


    //PUT- actualizar perfil de un usuario RUNNER
    [HttpPut("ActualizarPerfilRunner")]
    public async Task<IActionResult> ActualizarPerfilRunner([FromBody] ActualizarPerfilRunnerDto request)
    {
      // Validar modelo
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // Obtener ID del usuario desde el token
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userIdClaim.Value);

      // Buscar usuario en la BD
      var usuario = await _usuarioRepositorio.GetByIdAsync(userId);

      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      // Verificar que el usuario sea runner
      if (usuario.TipoUsuario.ToLower() != "runner")
        return BadRequest(new { message = "Este endpoint es solo para runners" });

      // Actualizar campos
      usuario.Nombre = request.Nombre;
      usuario.Telefono = request.Telefono;

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
        message = "Perfil de runner actualizado exitosamente",
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


    /*PUT: api/Usuario/ActualizarPerfilOrganizador
    Actualiza el perfil de un Organizador autenticado*/
    [HttpPut("ActualizarPerfilOrganizador")]
    public async Task<IActionResult> ActualizarPerfilOrganizador([FromBody] ActualizarPerfilOrganizadorDto request)
    {
      // Validar modelo
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // Obtener ID del usuario desde el token
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userIdClaim.Value);

      // Buscar usuario en la BD
      var usuario = await _usuarioRepositorio.GetByIdAsync(userId);

      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      // Verificar que el usuario sea organizador
      if (usuario.TipoUsuario.ToLower() != "organizador")
        return BadRequest(new { message = "Este endpoint es solo para organizadores" });

      // Actualizar campos
      usuario.Nombre = request.Nombre;
      usuario.Telefono = request.Telefono;

      if (request.Apellido != null)
        usuario.Apellido = request.Apellido;

      if (!string.IsNullOrEmpty(request.Localidad))
        usuario.Localidad = request.Localidad;

      // Guardar cambios en la BD
      await _usuarioRepositorio.UpdateAsync(usuario);

      // Retornar usuario actualizado
      return Ok(new
      {
        message = "Perfil de organizador actualizado exitosamente",
        usuario = new
        {
          idUsuario = usuario.IdUsuario,
          nombre = usuario.Nombre,
          apellido = usuario.Apellido,
          email = usuario.Email,
          telefono = usuario.Telefono,
          tipoUsuario = usuario.TipoUsuario,
          localidad = usuario.Localidad
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

    //verificar dni si esta disponible antes de registrase solo RUNNERS
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

    /* GET: api/Usuario/Organizador/{id}
      Obtiene el perfil público detallado de un Organizador*/
    [AllowAnonymous]
[HttpGet("Organizador/{id}")]
public async Task<IActionResult> ObtenerPerfilOrganizador(int id)
{
  // Buscar usuario por id (solo activos)
  var usuario = await _usuarioRepositorio.GetByIdAsync(id);

  // Si no existe o está eliminado
  if (usuario == null)
    return NotFound(new { message = "Usuario no encontrado" });

  // Verificar que sea organizador
  if (usuario.TipoUsuario.ToLower() != "organizador")
    return BadRequest(new { message = "El usuario no es un organizador" });

  // Retornar información pública del organizador
  return Ok(new
  {
    idUsuario = usuario.IdUsuario,
    nombre = usuario.Nombre,
    apellido = usuario.Apellido,
    tipoUsuario = usuario.TipoUsuario,
    localidad = usuario.Localidad,
    telefono = usuario.Telefono, // Telefono público para contacto
    email = usuario.Email // Email público para consultas
    // Nota: No incluimos campos sensibles como passwordHash, estado, etc.
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