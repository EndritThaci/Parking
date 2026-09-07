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
            return await _db.Useri.Where(a => a.active).AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var user = await _db.Useri.Where(a => a.active).Include(u=> u.UserOrgs).FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower());

                if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Passwordi, loginDTO.Password) == PasswordVerificationResult.Failed)
                {
                    return new LoginResponseDTO
                    {
                        UserReadDTO = null,
                        Token = null
                    };
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

        public async Task<UserReadDTO> RegisterAsync(UserCreateDTO userCreate, string role)
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
                    UserOrgs = new List<UserOrg>()
                };

                if (userCreate.UserOrgs != null && userCreate.UserOrgs.Any())
                {
                    user.UserOrgs = _mapper.Map<List<UserOrg>>(userCreate.UserOrgs);

                    foreach (var userOrg in user.UserOrgs)
                    {
                        userOrg.User = user;
                    }
                }

                await _db.Useri.AddAsync(user);
                await _db.SaveChangesAsync();

                return _mapper.Map<UserReadDTO>(user);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during registration", ex);
            }
        }

        public async Task<string> ChangePassword(Useri user, string OldPassword, string NewPassword)
        {
            try
            {
                if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Passwordi, OldPassword) == PasswordVerificationResult.Failed)
                {
                    return "";
                }

                return _passwordHasher.HashPassword(null, NewPassword);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during registration", ex);
            }
        }

        public string GenerateToken(Useri useri)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]!);

            var singleOrg = new UserOrg();
            try
            {
                singleOrg = useri.UserOrgs.SingleOrDefault();
            }
            catch {
                singleOrg?.BiznesId = 0;
                singleOrg?.NjesiaId = 0;
            }
            var org = _db.Organizata.FindAsync(singleOrg?.BiznesId);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, useri.UserId.ToString()),
                    new Claim(ClaimTypes.Email, useri.Email),
                    new Claim(ClaimTypes.Name, useri.Emri),
                    new Claim(ClaimTypes.Role, useri.Role),
                    new Claim("BiznesId", singleOrg?.BiznesId.ToString() ?? "0"),
                    new Claim("NjesiaId", singleOrg?.NjesiaId.ToString() ?? "0"),
                    new Claim("OrgName", org.Result?.EmriBiznesit.ToString() ?? "Parking"),
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
