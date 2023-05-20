using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private  string secretKey;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db,IConfiguration configuration, 
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            this._db = db;
            secretKey = configuration.GetValue<string>("APISettings:Secret");
            _userManager=userManager;
            _mapper= mapper;
            _roleManager= roleManager;
        }
        public async Task<bool> IsUniqueUser(string username)
        {
            var user =await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            //checking for valid user
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());
            //checking password is valid or not
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            
            if (user == null || isValid==false) 
            {
                return new LoginResponseDTO
                {
                    Token = "",
                    User = null
                };
            };


            //if user was found generate JWT token

            //getting roles
            var roles=await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            //converting into bytes
            var key=Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,user.UserName.ToString()),
                    new Claim(ClaimTypes.Role,roles.FirstOrDefault())
                }),
                Expires= DateTime.UtcNow.AddDays(7),
                SigningCredentials=new (new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new LoginResponseDTO
            {
                Token = tokenHandler.WriteToken(token),
                User = _mapper.Map<UserDTO>(user),
                Role= roles.FirstOrDefault(),
                
            };
            
            return loginResponseDTO;

        }

        public async Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationRequestDTO.UserName,
                Email = registrationRequestDTO.UserName,
                NormalizedEmail = registrationRequestDTO.UserName.ToUpper(),
                Name = registrationRequestDTO.Name,

            };
            try 
            {
                var result=await _userManager.CreateAsync(user,registrationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }
                    await _userManager.AddToRoleAsync(user,"admin");
                    var userToReturn = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == registrationRequestDTO.UserName);
                    //return new UserDTO
                    //{
                    //    ID=userToReturn.Id,
                    //    UserName=userToReturn.UserName,
                    //    Name=userToReturn.Name,
                    //};

                    return _mapper.Map<UserDTO>(userToReturn);

                }
            }
            catch (Exception ex) 
            {
                
            }
            return new UserDTO();
        }
    }
}
