using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;


namespace Parking_project.Services
{
    public class AuthService : IAuthService
    {
        private readonly AplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<Useri> _passwordHasher = new();

        public AuthService(AplicationDbContext db,IConfiguration configuration, IMapper mapper)
        {
            _db = db;
            _configuration = configuration;
            _mapper = mapper;
        }


        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _db.Useri.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var user = await _db.Useri.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower());

                if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Passwordi, loginDTO.Password) == PasswordVerificationResult.Failed)

                {
                    return null;
                }

                var token = GenerateToken(user);

                return new LoginResponseDTO
                {
                    UserReadDTO = _mapper.Map<UserReadDTO>(user),
                    Token = token
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during registration", ex);
            }

        }

        public async Task<UserReadDTO?> RegisterAsync(UserCreateDTO userCreate, string role)
        {
            try
            {
                if (await IsEmailExistsAsync(userCreate.Email))
                {
                    throw new InvalidOperationException($"User with email '{userCreate.Email}' already exists");
                }

                Useri user = new()
                {
                    Email = userCreate.Email,
                    Emri = userCreate.Emri,
                    Mbiemri = userCreate.Mbiemri,
                    Passwordi = _passwordHasher.HashPassword(null, userCreate.Passwordi),
                    Role = role,
                    BiznesId = userCreate.BiznesId,
                    NjesiaId = userCreate.NjesiaId
                };

                await _db.Useri.AddAsync(user);
                await _db.SaveChangesAsync();

                return _mapper.Map<UserReadDTO>(user);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during registration", ex);
            }

        }

        private string GenerateToken(Useri useri)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, useri.UserId.ToString()),
                    new Claim(ClaimTypes.Email, useri.Email),
                    new Claim(ClaimTypes.Name, useri.Emri),
                    new Claim(ClaimTypes.Role, useri.Role),
                    new Claim("BiznesId", useri.BiznesId.ToString() ?? ""),
                    new Claim("NjesiaId", useri.NjesiaId.ToString() ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
