using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db)
        {
            this._db = db;
        }
        public bool IsUniqueUser(string username)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            throw new NotImplementedException();
        }

        public Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            throw new NotImplementedException();
        }
    }
}
