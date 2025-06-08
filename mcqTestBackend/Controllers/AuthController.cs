using mcqTestBackend.Dtos.Auth;
using mcqTestBackend.Model;
using mcqTestBackend.Repositories.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace mcqTestBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        ApiResponse _response;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
            _response = new ApiResponse();

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO model)
        {
            try
            {
                var res = await _authRepository.RegisterAsync(model);
                ApiResponse response = new ApiResponse();
                _response.Result = res;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessage.Add(ex.Message);
                return BadRequest(_response);
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO model)
        {
            try
            {
                var res = await _authRepository.LoginAsync(model);
                ApiResponse response = new ApiResponse();
                _response.Result = res;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessage.Add(ex.Message);
                return BadRequest(_response);
            }
        }

    }
}