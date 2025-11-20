//Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using RunnConnectAPI.Data;
using RunnConnectAPI.Services;
using RunnConnectAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration; //Obtenemos la confi para usarla

//Habilitar controllers
builder.Services.AddControllers();


// Habilitar Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CONEXION A MySQL
builder.Services.AddDbContext<RunnersContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    )
);

//Registrar JWTService para inyeccion de dependencias
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<PasswordService>();

//Repos
builder.Services.AddScoped<UsuarioRepositorio>();
builder.Services.AddScoped<EventoRepositorio>();

//CORS (Para que la app se pueda conectar)
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAllPolicy",
    policy =>
    {
      policy.AllowAnyOrigin() //Cualquier origen para el desarrollo
            .AllowAnyMethod() //Permite cualquier metodo HTTP (GET, POST, PUT etc)
            .AllowAnyHeader(); //PErmite cualquier cabecera
    });
});


// Configurar Autenticacion JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        // Que validar:
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // Valores validos (leidos desde appsettings.json):
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
      };
    });
  
//Configurar autorizacion
builder.Services.AddAuthorization();  


//Construccion de la app y config del Pipeline HTTP

var app = builder.Build();

// Configure the HTTP request pipeline (El orden importa).
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Aplicar politicas de CORS
app.UseCors("AllowAllPolicy");

//Aplicar Autenticacion (Antes de Autorizacion)
app.UseAuthentication();

//Aplicar Autorizacion
app.UseAuthorization();

//Mapear los Controllers 
app.MapControllers();

app.Run();


