using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using olympo_webapi.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Linq;
using olympo_webapi.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;

    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IFileUploadService fileUploadService, ILogger<AuthController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var appUser = await _userManager.FindByEmailAsync(request.Username);
        if (appUser == null)
            return Unauthorized("Usuário ou senha inválidos.");

        var result = await _signInManager.CheckPasswordSignInAsync(appUser, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Usuário ou senha inválidos.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == appUser.UserId);
        if (user == null)
            return NotFound("Dados do usuário não encontrados.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("treinix@app@2025_secureJWT_key_2025!");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, appUser.Id),
                new Claim(ClaimTypes.Name, appUser.UserName),
                new Claim(ClaimTypes.Email, appUser.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new
        {
            Message = "Login realizado com sucesso.",
            Token = tokenHandler.WriteToken(token),
            User = new
            {
                IdentityData = new
                {
                    appUser.Id,
                    appUser.UserName,
                    appUser.Email
                },
                UserData = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.CPF,
                    user.Phone,
                    user.BirthDate,
                    user.Type
                }
            }
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok("Logout realizado com sucesso.");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Iniciando registro de usuário: {Email}", request.Email);

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Tentativa de registro com e-mail duplicado: {Email}", request.Email);
                return BadRequest("O e-mail informado já está em uso.");
            }

            if (!string.IsNullOrEmpty(request.CPF) && await _context.Users.AnyAsync(u => u.CPF == request.CPF))
            {
                _logger.LogWarning("Tentativa de registro com CPF duplicado: {CPF}", request.CPF);
                return BadRequest("O CPF informado já está em uso.");
            }

            if (request.Type == null || !Enum.IsDefined(typeof(UserType), request.Type))
            {
                _logger.LogWarning("Tipo de usuário inválido ou nulo: {Type}", request.Type);
                return BadRequest("Tipo de usuário inválido.");
            }

            var userType = (UserType)request.Type;

            var applicationUser = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(applicationUser, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Erro ao criar usuário no Identity: {@Errors}", result.Errors);
                return BadRequest(result.Errors);
            }

            var userRecord = new User
            {
                Name = request.Username,
                Email = request.Email,
                CPF = request.CPF,
                Phone = request.Phone,
                BirthDate = request.BirthDate,
                Type = userType, 
                IdentityId = applicationUser.Id
            };

            _context.Users.Add(userRecord);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuário salvo na tabela User com ID: {UserId}", userRecord.Id);

            _logger.LogInformation("Usuário registrado com sucesso: {Email}", request.Email);
            return Ok("Usuário registrado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário. Detalhes: {Mensagem} | Email: {Email}", ex.Message, request.Email);

            var errorDetails = new
            {
                error = "Erro interno no servidor.",
                message = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            };

            return StatusCode(500, errorDetails);
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetUserData()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var appUser = await _userManager.Users.FirstOrDefaultAsync(au => au.UserId == user.Id);
            if (appUser == null)
                return NotFound("Dados de login não encontrados.");

            return Ok(new
            {
                IdentityData = new
                {
                    appUser.Id,
                    appUser.UserName,
                    appUser.Email,
                    appUser.LockoutEnabled,
                    appUser.AccessFailedCount
                },
                UserData = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.CPF,
                    user.Phone,
                    user.BirthDate,
                    user.Type,
                    user.ImagePath
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro interno no servidor.", details = ex.Message });
        }
    }

    /*
    * feat(#): Permitir apenas que o administrdor veja apenas os usuários vinculados a sua.
    */

    [HttpGet("full-users/{type}")]
    public async Task<IActionResult> GetFullUsers(int type)
    {
        try
        {
            var users = await _userManager.Users
                .Include(u => u.User)
                .ThenInclude(user => user.Gyms!)
                .ThenInclude(gymUser => gymUser.Gym!) 
                .Where(u => u.User != null && u.User.Type.HasValue && (int)u.User.Type == type) 
                .Select(u => new
                {
                    IdentityId = u.Id,
                    u.UserName,
                    u.Email,
                    u.PasswordHash,
                    AppUser = u.User != null ? new
                    {
                        u.User.Id,
                        u.User.Name,
                        u.User.Email,
                        u.User.CPF,
                        u.User.Phone,
                        u.User.BirthDate,
                        u.User.Type,
                        u.User.ImagePath,
                        Gym = u.User.Gyms != null ? u.User.Gyms.Select(gymUser => new
                        {
                            gymUser.Gym!.Id, // Usa operador de nulidade para evitar erros
                            gymUser.Gym.Name,
                            gymUser.Gym.Address,
                            gymUser.Gym.PhoneNumber
                        }).FirstOrDefault() : null
                    } : null
                })
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound("Nenhum usuário encontrado para o tipo especificado.");
            }

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro interno no servidor.", details = ex.Message });
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            var filePath = await _fileUploadService.UploadFileAsync(file);
            return Ok(new { filePath });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex}");
            return StatusCode(500, "An error occurred while processing the request. ==> " + ex.Message);
        }
    }
    

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUserData(int? id, [FromBody] UpdateUserRequest request, [FromForm] IFormFile? image)
    {
        try 
        {
            if (id == null)
                return BadRequest("O ID do usuário (rota) é obrigatório.");
            if (request.IdentityId == null)
                return BadRequest("O ID do usuário (payload) é obrigatório.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("Usuário não encontrado na tabela User.");

            var appUser = await _userManager.Users.FirstOrDefaultAsync(au => au.Id == request.IdentityId.ToString());
            if (appUser == null)
                return NotFound("Usuário não encontrado na tabela Identity.");

            if (appUser.UserId != user.Id)
                return BadRequest("O ID do usuário no Identity não corresponde ao ID do usuário na tabela User.");

            if (!string.IsNullOrEmpty(request.Email) && request.Email != appUser.Email)
            {
                var emailExists = await _userManager.Users.AnyAsync(u => u.Email == request.Email && u.Id != appUser.Id);
                if (emailExists)
                    return BadRequest("O e-mail informado já está em uso por outro usuário.");
                appUser.Email = request.Email;
            }

            appUser.UserName = request.UserName ?? appUser.UserName;

            if (!string.IsNullOrEmpty(request.Password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
                var passwordResult = await _userManager.ResetPasswordAsync(appUser, resetToken, request.Password);
                if (!passwordResult.Succeeded)
                    return BadRequest("Erro ao atualizar a senha: " + string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
            }

            if (image != null)
            {
                try
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(image);
                    if (!string.IsNullOrEmpty(uploadedImagePath))
                    {
                        user.ImagePath = uploadedImagePath;
                    }
                    else
                    {
                        return BadRequest("Falha ao fazer upload da imagem.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest($"Erro ao fazer upload da imagem: {ex.Message}");
                }
            }

            var identityResult = await _userManager.UpdateAsync(appUser);
            if (!identityResult.Succeeded)
                return BadRequest("Erro ao atualizar os dados de login: " + string.Join(", ", identityResult.Errors.Select(e => e.Description)));

            user.Name = request.UserName ?? user.Name;
            user.Email = request.Email ?? user.Email;
            user.CPF = request.CPF ?? user.CPF;
            user.Phone = request.Phone ?? user.Phone;

            if (request.BirthDate.HasValue)
            {
                user.BirthDate = DateTime.SpecifyKind(request.BirthDate.Value, DateTimeKind.Utc);
            }

            if (!string.IsNullOrEmpty(request.ImagePath))
                user.ImagePath = request.ImagePath;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Dados atualizados com sucesso.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro interno no servidor.", details = ex.Message });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string? CPF { get; set; }
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public UserType? Type { get; set; }
    }

    public class UpdateUserRequest
    {
        public Guid? IdentityId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? CPF { get; set; }
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ImagePath { get; set; }
        public UserType? Type { get; set; }
    }
};
