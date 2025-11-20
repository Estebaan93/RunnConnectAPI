//Services/FileService
using System.IO;

namespace RunnConnectAPI.Services
{
  /// Servicio para manejo de archivos (avatares, comprobantes, etc.)

  public class FileService
  {
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public FileService(IWebHostEnvironment environment, IConfiguration configuration)
    {
      _environment = environment;
      _configuration = configuration;
    }

    /// Guarda un avatar y retorna la URL relativa
    public async Task<string> GuardarAvatarAsync(IFormFile archivo, int idUsuario)
    {
      // Validar extension
      var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
      var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

      if (!extensionesPermitidas.Contains(extension))
        throw new Exception("Formato de imagen no permitido. Use: jpg, jpeg, png, gif o webp");

      // Validar tamaño (maximo 20MB)
      if (archivo.Length > 20 * 1024 * 1024)
        throw new Exception("La imagen no puede superar 5MB");

      // Crear carpeta si no existe
      var carpetaAvatares = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
      if (!Directory.Exists(carpetaAvatares))
        Directory.CreateDirectory(carpetaAvatares);

      // Generar nombre unico: userid_timestamp.extension
      var nombreArchivo = $"{idUsuario}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
      var rutaCompleta = Path.Combine(carpetaAvatares, nombreArchivo);

      // Guardar archivo
      using (var stream = new FileStream(rutaCompleta, FileMode.Create))
      {
        await archivo.CopyToAsync(stream);
      }

      // Retornar URL relativa
      return $"/uploads/avatars/{nombreArchivo}";
    }

  
    /// Elimina un avatar del servidor
    public bool EliminarAvatar(string urlRelativa)
    {
      try
      {
        if (string.IsNullOrEmpty(urlRelativa))
          return false;

        // Convertir URL relativa a ruta fisica
        var rutaFisica = Path.Combine(_environment.WebRootPath, urlRelativa.TrimStart('/'));

        if (File.Exists(rutaFisica))
        {
          File.Delete(rutaFisica);
          return true;
        }

        return false;
      }
      catch
      {
        return false;
      }
    }


    /// Obtiene la URL completa del avatar
    public string ObtenerUrlCompleta(string urlRelativa, HttpRequest request)
    {
      if (string.IsNullOrEmpty(urlRelativa))
        return string.Empty;

      // Si ya es una URL completa, retornarla tal cual
      if (urlRelativa.StartsWith("http://") || urlRelativa.StartsWith("https://"))
        return urlRelativa;

      // Construir URL completa
      var scheme = request.Scheme; // http o https
      var host = request.Host.Value; // localhost:5000
      return $"{scheme}://{host}{urlRelativa}";
    }

    /// Valida que una imagen sea valida
    public bool ValidarImagen(IFormFile archivo)
    {
      var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
      var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

      return extensionesPermitidas.Contains(extension) && archivo.Length <= 20 * 1024 * 1024;
    }
  } 
}