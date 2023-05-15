using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private  string secretKey;
        public UserRepository(ApplicationDbContext db,IConfiguration configuration)
        {
            this._db = db;
            secretKey = configuration.GetValue<string>("APISettings:Secret");
        }
        public async Task<bool> IsUniqueUser(string username)
        {
            var user =await _db.LocalUsers.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            //checking for valid user
            var user = _db.LocalUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower() && u.Password == loginRequestDTO.Password);
            if (user == null) 
            {
                return new LoginResponseDTO
                {
                    Token = "",
                    User = null
                };
            };


            //if user was found generate JWT token

            var tokenHandler = new JwtSecurityTokenHandler();
            //converting into bytes
            var key=Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,user.UserName.ToString()),
                    new Claim(ClaimTypes.Role,user.Role)
                }),
                Expires= DateTime.UtcNow.AddDays(7),
                SigningCredentials=new (new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new LoginResponseDTO
            {
                Token = tokenHandler.WriteToken(token),
                User = user,
                
            };
            
            return loginResponseDTO;

        }

        public async Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            LocalUser user = new LocalUser
            {
                 UserName=registrationRequestDTO.UserName,
                 Name=registrationRequestDTO.Name,
                 Password=registrationRequestDTO.Password,
                 Role= registrationRequestDTO.Role
            };
            await _db.LocalUsers.AddAsync(user);
            await _db.SaveChangesAsync();
            user.Password = "";
            return user;
        }
    }
}
