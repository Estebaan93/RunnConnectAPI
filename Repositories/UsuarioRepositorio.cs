//Repositories/UsuarioRepositorio
using Microsoft.EntityFrameworkCore; //Para las consultas y operaciones con la BD
using RunnConnectAPI.Data;          // DbContext del proyecto
using RunnConnectAPI.Models;        //Entidades

namespace RunnConnectAPI.Repositories
{
  //Encapsular la logica de acceso a datos
  public class UsuarioRepositorio
  {
    private readonly RunnersContext _context; //campo privado para el dbContext

    public UsuarioRepositorio(RunnersContext context) //Constructor e inyectamos el DbContext por dependencias
    {
      _context= context;
    }

    //Obtenemos usuario por ID(clave primaria)
    public async Task<Usuario?> GetByIdAsync(int id)
    {
      return await _context.Usuarios.FirstOrDefaultAsync(u=>u.IdUsuario== id && u.Estado); //Busca por PK y que esten activos
    }

    //Obtener por email y que esten activos
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
      return await _context.Usuarios.FirstOrDefaultAsync(u=> u.Email==email && u.Estado); //Devuelve el primero o null 
    }

    //Verificar si existe un email (solo activos runner/organizadores)
    public async Task<bool> EmailExistenteAsync(string email)
    {
      return await _context.Usuarios.AnyAsync(u=> u.Email==email && u.Estado); //True si existe
    } 

    //Verificar si existe un DNI (solo activos y runner)
    public async Task<bool> DniExisteAsync(int dni)
    {
      return await _context.Usuarios.AnyAsync(u=>u.Dni==dni && u.Estado); //True si existe
    }

    //Crear nuevo usuario por defecto true
    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
      _context.Usuarios.Add(usuario);       //Marca la entidad como nueva
      await _context.SaveChangesAsync();    //Persite en la BD
      return usuario;                       //Devuelve el obj creado
    }

    //Actualizar usuario
    public async Task UpdateAsync(Usuario usuario)
    {
      _context.Usuarios.Update(usuario);  //Marca la entidad como modificacda
      await _context.SaveChangesAsync();  //Persiste cambios 
    }

    //Eliminar persistente
    public async Task DeleteAsync(Usuario usuario)
    {
      _context.Usuarios.Remove(usuario);    // Marca la entidad como eliminada
      await _context.SaveChangesAsync();    // Persiste eliminacion
    }

    //Eliminado logico
    public async Task DeleteLogicoAsync(Usuario usuario)
    {
      usuario.Estado=false; //Pasa el estado a desactivado
      _context.Usuarios.Update(usuario);
      await _context.SaveChangesAsync();
    }

    //Verificar si existe un usuario (solo activo)
    public async Task<bool> ExisteAsync(int id)
    {
      return await _context.Usuarios.AnyAsync(u=>u.IdUsuario==id && u.Estado); //True si existe
    }

  }
}