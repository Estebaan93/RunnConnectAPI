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
    private readonly FileService _fileService;

    public UsuarioController(UsuarioRepositorio usuarioRepositorio, JWTService jwtService, PasswordService passwordService, FileService fileService)
    {
      _usuarioRepositorio = usuarioRepositorio;
      _jwtService = jwtService;
      _passwordService = passwordService;
      _fileService = fileService;
    }

    /*Autenticacion (Login comun para ambos RUNNER/ORGAN)
    POST: api/usuario/login - 
    Content-type: application/json*/
    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
      //Validamos el modelo DataAnnotations
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      //Buscar por email
      var usuario = await _usuarioRepositorio.GetByEmailAsync(loginRequestDto.Email);

      if (usuario == null || !_passwordService.VerifyPassword(loginRequestDto.Password, usuario.PasswordHash))
        return Unauthorized(new { message = "Credenciales invalidas" });

      //Generar token
      var token = _jwtService.GenerarToken(usuario);
      var avatarUrl = _fileService.ObtenerUrlCompleta(usuario.ImgAvatar, Request);

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
          tipoUsuario = usuario.TipoUsuario,
          imgAvatar = avatarUrl
        }
      });
    }

    /*Registro de nuevo usuarios RUNNERs (avatar opcional) y retorna un JWT para login automatico
    POST: api/usuarios/RegisterRunner
    Content-type: multipart/form-data
    Si no se envia imgAvatar se asigna uno default_runner.png*/
    [AllowAnonymous]
    [HttpPost("RegisterRunner")]
    public async Task<IActionResult> Register([FromForm] RegisterRunnerDto dto)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // Validaciones
      if (await _usuarioRepositorio.EmailExistenteAsync(dto.Email))
        return Conflict(new { message = "El email ya esta registrado" });

      if (await _usuarioRepositorio.DniExisteAsync(dto.Dni))
        return Conflict(new { message = "El DNI ya esta registrado" });
      
      //Validamos la edad minima
      if(dto.FechaNacimiento.HasValue)
      {
        var edad = DateTime.Today.Year - dto.FechaNacimiento.Value.Year;
        if (dto.FechaNacimiento.Value.Date > DateTime.Today.AddYears(-edad))
          edad--;

        if (edad < 14)
          return BadRequest(new { message = "Debes tener al menos 14 años para registrarte" });

        if (dto.FechaNacimiento.Value > DateTime.Today)
          return BadRequest(new { message = "La fecha de nacimiento no puede ser futura" });
      }

      //Normalizamos genero
      if (!string.IsNullOrEmpty(dto.Genero))
      {
        dto.Genero = dto.Genero.ToUpper().Trim();
        if (dto.Genero != "F" && dto.Genero != "M" && dto.Genero != "X")
          return BadRequest(new { message = "El genero debe ser F, M o X" });
      }
      // Crear usuario (sin avatar todavia)
      var usuario = new Usuario
      {
        Nombre = dto.Nombre.Trim(),
        Apellido = dto.Apellido.Trim(),
        Email = dto.Email.ToLower(),
        PasswordHash = _passwordService.HashPassword(dto.Password),
        Telefono = dto.Telefono,
        TipoUsuario = "runner",
        Dni = dto.Dni,
        FechaNacimiento = dto.FechaNacimiento,
        Genero = dto.Genero,
        Localidad = dto.Localidad.Trim(),
        Agrupacion = dto.Agrupacion.Trim(),
        TelefonoEmergencia = dto.TelefonoEmergencia.Trim(),
        Estado = true
      };

      // Guardar usuario primero para obtener el ID
      await _usuarioRepositorio.CreateAsync(usuario);

      // Gestionar avatar (opcional)
      try
      {
        var avatarUrl = await _fileService.GuardarAvatarRegistroAsync(
            dto.ImgAvatar,
            usuario.IdUsuario,
            "runner"
        );

        usuario.ImgAvatar = avatarUrl;
        await _usuarioRepositorio.UpdateAsync(usuario);
      }
      catch (Exception ex)
      {
        // Si falla la subida, asignar avatar por defecto
        Console.Write(ex.Message);
        usuario.ImgAvatar = _fileService.ObtenerAvatarPorDefecto("runner");
        await _usuarioRepositorio.UpdateAsync(usuario);
      }

      //Generar y obtener el token
      var token = _jwtService.GenerarToken(usuario);
      var avatarUrlCompleta = _fileService.ObtenerUrlCompleta(usuario.ImgAvatar, Request);

      return Ok(new
      {
        message = "Runner registrado exitosamente",
        token,
        usuario = new
        {
          idUsuario = usuario.IdUsuario,
          nombre = usuario.Nombre,
          apellido = usuario.Apellido,
          email = usuario.Email,
          tipoUsuario = usuario.TipoUsuario,
          imgAvatar = avatarUrlCompleta
        }
      });
    }


    /*Registro de nuevos ORGANIZADORES (imgAvatar opcional) y retorna el token
    POST_ api/usuario/RegisterOrganizador 
    Content-type: multipart/form-data
    Si no se envia imgAvatar se asigna uno por defecto default_organization.png*/
    [AllowAnonymous]
    [HttpPost("RegisterOrganizador")]
    public async Task<IActionResult> RegisterOrganizador([FromForm] RegisterOrganizadorDto dto)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // Validaciones email unico
      if (await _usuarioRepositorio.EmailExistenteAsync(dto.Email))
        return Conflict(new { message = "El email ya esta registrado" });

      // Crear usuario (sin avatar todavía)
      var usuario = new Usuario
      {
        Nombre = dto.Nombre.Trim(),
        Apellido = dto.Apellido.Trim(),
        Email = dto.Email.Trim().ToLower(),
        PasswordHash = _passwordService.HashPassword(dto.Password),
        Telefono = dto.Telefono,
        TipoUsuario = "organizador",
        Localidad = dto.Localidad.Trim(),
        Estado = true
      };

      // Guardar usuario primero para obtener el ID
      await _usuarioRepositorio.CreateAsync(usuario);

      // Gestionar avatar (opcional)
      try
      {
        var avatarUrl = await _fileService.GuardarAvatarRegistroAsync(
            dto.ImgAvatar,
            usuario.IdUsuario,
            "organizador"
        );

        usuario.ImgAvatar = avatarUrl;
        await _usuarioRepositorio.UpdateAsync(usuario);
      }
      catch(Exception ex)
      {
        // Si falla la subida, asignar avatar por defecto
        Console.Write(ex.Message);
        usuario.ImgAvatar = _fileService.ObtenerAvatarPorDefecto("organizador");
        await _usuarioRepositorio.UpdateAsync(usuario);
      }

      //Generar y obtener token
      var token = _jwtService.GenerarToken(usuario);
      var avatarUrlCompleta = _fileService.ObtenerUrlCompleta(usuario.ImgAvatar, Request);

      return Ok(new
      {
        message = "Organizador registrado exitosamente",
        token,
        usuario = new
        {
          idUsuario = usuario.IdUsuario,
          nombre = usuario.Nombre,
          apellido = usuario.Apellido,
          email = usuario.Email,
          tipoUsuario = usuario.TipoUsuario,
          imgAvatar = avatarUrlCompleta
        }
      });
    }


    /*GET- Obtener perfil del usuario autenticado
    GET: api/Usuario/Perfil*/
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

      //Obtenemos el imgAvatar
      var avatarUrl = _fileService.ObtenerUrlCompleta(usuario.ImgAvatar, Request);

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
        telefonoEmergencia = usuario.TelefonoEmergencia,
        imgAvatar = avatarUrl,
        esAvatarPorDefecto = _fileService.EsAvatarPorDefecto(usuario.ImgAvatar),
        estado = usuario.Estado
      };
      return Ok(usuarioResponse);
    }


    /*actualizar perfil de un usuario RUNNER
    PUT: api/Usuario/ActualizarPerfilRunner
    Content-Type: application/json*/
    [HttpPut("ActualizarPerfilRunner")]
    public async Task<IActionResult> ActualizarPerfilRunner([FromBody] ActualizarPerfilRunnerDto dto)
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

      // Validar edad mínima (14 años)
      if (dto.FechaNacimiento.HasValue)
      {
        var edad = DateTime.Today.Year - dto.FechaNacimiento.Value.Year;
        if (dto.FechaNacimiento.Value.Date > DateTime.Today.AddYears(-edad))
          edad--;

        if (edad < 14)
          return BadRequest(new { message = "Debes tener al menos 14 años para participar" });

        if (dto.FechaNacimiento.Value > DateTime.Today)
          return BadRequest(new { message = "La fecha de nacimiento no puede ser futura" });
      }

      // Normalizar y validar genero
      if (!string.IsNullOrEmpty(dto.Genero))
      {
        dto.Genero = dto.Genero.ToUpper().Trim();
        if (dto.Genero != "F" && dto.Genero != "M" && dto.Genero != "X")
          return BadRequest(new { message = "El genero debe ser F, M o X" });
      }  

      //ACTUALIZAR CAMPOS CON NORMALIZACION (DNI y Email NO se actualizan)
      usuario.Nombre = dto.Nombre.Trim();
      usuario.Apellido = dto.Apellido?.Trim();
      usuario.Telefono = dto.Telefono;
      usuario.FechaNacimiento = dto.FechaNacimiento;
      usuario.Genero = dto.Genero;
      usuario.Localidad = dto.Localidad?.Trim();
      usuario.Agrupacion = dto.Agrupacion?.Trim();
      usuario.TelefonoEmergencia = dto.TelefonoEmergencia?.Trim();

      await _usuarioRepositorio.UpdateAsync(usuario);

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
    Actualiza el perfil de un Organizador autenticado
    Content-Type: application/json*/
    [HttpPut("ActualizarPerfilOrganizador")]
    public async Task<IActionResult> ActualizarPerfilOrganizador([FromBody] ActualizarPerfilOrganizadorDto dto)
    {
      // Validar modelo
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // Obtener Id del usuario desde el token
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

      // Actualizar campos con normalizacion (Email no se actualiza)
      usuario.Nombre = dto.Nombre.Trim();
      usuario.Apellido = dto.Apellido?.Trim();
      usuario.Telefono = dto.Telefono;
      usuario.Localidad = dto.Localidad?.Trim();      

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

    /*Gestion de contraseña - cambiar la contraseña de usuario
    PUT: api/Usuario/CambiarPassword
    Content-Type: application/json*/
[Authorize]
    [HttpPut("CambiarPassword")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
    {
      // 1. Validar modelo
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      // 2. Obtener ID del usuario desde el token
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userIdClaim.Value);

      // 3. Buscar usuario en la BD
      var usuario = await _usuarioRepositorio.GetByIdAsync(userId);

      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      // 4. Verificar contraseña actual
      if (!_passwordService.VerifyPassword(dto.PasswordActual, usuario.PasswordHash))
        return BadRequest(new { message = "La contraseña actual es incorrecta" });

      // 5. Actualizar contraseña
      usuario.PasswordHash = _passwordService.HashPassword(dto.NuevaPassword);
      await _usuarioRepositorio.UpdateAsync(usuario);

      return Ok(new { message = "Contraseña cambiada exitosamente" });
    }

    //verificar email antes de registrar
    //GET - api/usuarios/verificarEmail?email=test@test.com
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
    //GET - api/usuarios/verificarDni?dni=12345678
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
        message = existe ? "Dni ya registrado" : "Dni disponible"
      });

    }

    /*Ver perfil publico (por ej si un runner consulta la posicion, puede ver quienes son los otros runner)
    tambien ver el perfil de un organizador - conlleva a mostrar una vista personalizada por los datos sensibles*/
    //GET - api/usuarios/{id}
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerUsuarioPorId(int id)
    {
      //Buscar usuario por id (que estan activos)
      var usuario = await _usuarioRepositorio.GetByIdAsync(id);

      //Si no existe o esta eliminado
      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      //Obtenermos el imgAvatar
      var avatarUrl= _fileService.ObtenerUrlCompleta(usuario.ImgAvatar, Request);
      //Retornamos solo informacion publica
      return Ok(new
      {
        idUsuario = usuario.IdUsuario,
        nombre = usuario.Nombre,
        apellido = usuario.Apellido,
        tipoUsuario = usuario.TipoUsuario,
        localidad = usuario.Localidad,
        agrupacion = usuario.Agrupacion,
        imgAvatar= avatarUrl
      });
    }

    /*Obtiene el perfil público detallado de un Organizador 
      GET: api/Usuario/Organizador/{id}
      Incluye informacion de contacto para consultas sobre eventos*/
    [AllowAnonymous]
    [HttpGet("Organizador/{id}")]
    public async Task<IActionResult> ObtenerPerfilOrganizador(int id)
    {
      // Buscar usuario por id (solo activos)
      var usuario = await _usuarioRepositorio.GetByIdAsync(id);

      // Si no existe o esta eliminado
      if (usuario == null)
        return NotFound(new { message = "Usuario no encontrado" });

      // Verificar que sea organizador
      if (usuario.TipoUsuario.ToLower() != "organizador")
        return BadRequest(new { message = "El usuario no es un organizador" });

      //Obtenemos el imgAvatar
      var avatarUrl= _fileService.ObtenerUrlCompleta(usuario.ImgAvatar, Request);

      // Retornar informacion publica del organizador
      return Ok(new
      {
        idUsuario = usuario.IdUsuario,
        nombre = usuario.Nombre,
        apellido = usuario.Apellido,
        tipoUsuario = usuario.TipoUsuario,
        localidad = usuario.Localidad,
        telefono = usuario.Telefono, // Telefono publico para contacto
        email = usuario.Email, // Email publico para consultas
        imgAvatar= avatarUrl    // Nota: No incluimos campos sensibles como passwordHash, estado, etc.
      });
    }

    //Eliminacion logica cambia estado
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

      //Eliminamos avatar si existe y no es el por defecto
      if (!string.IsNullOrEmpty(usuario.ImgAvatar)){
        _fileService.EliminarAvatar(usuario.ImgAvatar);
      };  

      //Eliminado logico su estado pasa a falso
      await _usuarioRepositorio.DeleteLogicoAsync(usuario);

      return Ok(new { message = "Cuenta eliminada exitosamente" });
    }

    /*Gestion de avatar usuario autenticado
    PUT: api/Usuario/Avatar
    Content-Type: multipart/form-data*/
    [Authorize]
    [HttpPut("Avatar")]
    public async Task<IActionResult> ActualizarAvatar([FromForm] IFormFile imagen)
    { 
      //Validar que se envio una imagen
      if (imagen == null || imagen.Length == 0)
        return BadRequest(new { message = "Debe enviar una imagen" });

      //Obtener Id del usuario desde el token
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userIdClaim.Value);

      try
      {
        //Buscar usuario en la BD
        var usuario = await _usuarioRepositorio.GetByIdAsync(userId);
        if (usuario == null)
          return NotFound(new { message = "Usuario no encontrado" });

        // Eliminar avatar anterior si existe y NO es por defecto
        if (!string.IsNullOrEmpty(usuario.ImgAvatar))
        {
          _fileService.EliminarAvatar(usuario.ImgAvatar);
        }

        // Guardar nuevo avatar
        var urlAvatar = await _fileService.GuardarAvatarAsync(imagen, userId);
        usuario.ImgAvatar = urlAvatar;
        await _usuarioRepositorio.UpdateAsync(usuario);

        //Obtener URL completa
        var urlCompleta = _fileService.ObtenerUrlCompleta(urlAvatar, Request);

        return Ok(new
        {
          message = "Avatar actualizado exitosamente",
          imgAvatar = urlCompleta
        });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    /*Eliminar avatar y restaurar el por defecto
    DELETE: api/Usuario/Avatar*/
    [Authorize]
    [HttpDelete("Avatar")]
    public async Task<IActionResult> EliminarAvatar()
    {
      //Obtener el Id del usuario desde el token
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return Unauthorized(new { message = "No autorizado" });

      var userId = int.Parse(userIdClaim.Value);

      try
      {
        //Buscar usuario en la BD
        var usuario = await _usuarioRepositorio.GetByIdAsync(userId);
        if (usuario == null)
          return NotFound(new { message = "Usuario no encontrado" });

        //Eliminar archivo si existe y no es el por defecto
        if (!string.IsNullOrEmpty(usuario.ImgAvatar))
        {
          _fileService.EliminarAvatar(usuario.ImgAvatar);
        }

        //Restaurar avatar por defecto
        usuario.ImgAvatar = _fileService.ObtenerAvatarPorDefecto(usuario.TipoUsuario);
        await _usuarioRepositorio.UpdateAsync(usuario);

        //Obtener URL completa
        var urlCompleta = _fileService.ObtenerUrlCompleta(usuario.ImgAvatar, Request);

        return Ok(new
        {
          message = "Avatar eliminado. Se restauro el avatar por defecto",
          imgAvatar = urlCompleta
        });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }


  }
}