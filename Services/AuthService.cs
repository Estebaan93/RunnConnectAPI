//Services/AuthService
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RunnConnectAPI.Models;
using RunnConnectAPI.Data;

namespace RunnConnectAPI.Services
{
  public class AuthService
  {
    private readonly DbContext _context;
    private readonly IConfiguration _configuration;
  }
    
}