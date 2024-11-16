using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly JwtTokenHandler _jwtTokenHandler;

        public AccountsController(JwtTokenHandler jwtTokenHandler)
        {
            _jwtTokenHandler = jwtTokenHandler;
        }

        [HttpPost("Authenticate")]
        public ActionResult<AuthenticationResponse?> Authenticate([FromBody] AuthenticationRequest request){
            var authenticationResponse = _jwtTokenHandler.GenerateJwtToken(request);
            if(authenticationResponse == null){
                return Unauthorized();
            }
            return Ok(authenticationResponse);
        }
    }
}
