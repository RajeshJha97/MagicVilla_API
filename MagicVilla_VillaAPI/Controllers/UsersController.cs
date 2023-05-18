using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/UsersAuth")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private  APIResponse _response;
        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _response = new APIResponse();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse=await _userRepo.Login(model);
            if (loginResponse.User == null ||string.IsNullOrEmpty(loginResponse.Token)) 
            {
                _response.ErrorMessage = new List<string> { "Username and password is incorrect"};
                _response.StatusCode=HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
                //return BadRequest(new { message = "Username and password is incorrect" });
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
           
            _response.Result= loginResponse;
            return Ok(_response);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            bool isUniqueUser = await _userRepo.IsUniqueUser(model.UserName);
            if (!isUniqueUser)
            {
                _response.ErrorMessage.Add("User already exist");
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var user =await _userRepo.Register(model);
            if (user == null)
            {
                _response.ErrorMessage.Add("Error while registering");
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }
    }
}
