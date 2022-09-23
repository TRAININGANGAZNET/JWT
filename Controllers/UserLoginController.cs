using JWT.JWT;
using logindemo.DAL;
using logindemo.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCHash = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Authorization;

namespace logindemo.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        public readonly ITokenRefresher _tokenRefresher;
        private readonly IGetDataReadersAsync _getDataReadersAsync;
        private readonly IConfiguration _configuration;
        private readonly UserLoginResponse userLoginResponse = new();

        public UserLoginController(IConfiguration configuration,
                                   IGetDataReadersAsync getDataReadersAsync, IJwtAuthenticationManager jwtAuthenticationManager, ITokenRefresher tokenRefresher)
        {
            _configuration = configuration;
            _getDataReadersAsync = getDataReadersAsync;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _tokenRefresher = tokenRefresher;
        }


        [AllowAnonymous]
        [Route("LoginData")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<UserLoginResponse>> GetUserLoginData(UserLoginRequest userLoginRequest)
        {

            List<UserLoginResponse> listUserLoginResponse = new List<UserLoginResponse>();
            try
            {
                

                listUserLoginResponse = await _getDataReadersAsync.AuthenticateUser(userLoginRequest);
                if (listUserLoginResponse.Count > 0 )
                    {

                    //listUserLoginResponse[0].ResponseCode = Convert.ToInt32(_configuration["ResponsesCodes:SuccessCode"]);
                    //listUserLoginResponse[0].Responses = _configuration["Responses:SuccessMessage"];
                   // listUserLoginResponse[0].PASSWORD = listUserLoginResponse[0].RefreshToken;
                }
                else
                {

                    //userLoginResponse.ResponseCode = Convert.ToInt32(_configuration["ResponsesCodes:FailureCode"]);
                    //userLoginResponse.Responses = _configuration["Responses:FailMessage"];
                    if (listUserLoginResponse.Count > 0)
                        listUserLoginResponse.RemoveAt(0);

                    listUserLoginResponse.Add(userLoginResponse);
                }
                return listUserLoginResponse;
            }
            catch (Exception ex)
            {
                //userLoginResponse.ResponseCode = Convert.ToInt32(_configuration["ResponsesCodes:ExceptionCode"]);
                //userLoginResponse.Responses = _configuration["Responses:FailMessage"];
                listUserLoginResponse.Add(userLoginResponse);
                return listUserLoginResponse;
            }
        }


        [AllowAnonymous]
        [Route("RefreshToken")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult RefreshToken([FromBody] RefreshCred refreshCred)
        {
            AuthenticationResponse token = new AuthenticationResponse();
            try
            {
                token = _tokenRefresher.Refresh(refreshCred);
                if (token == null)
                {
                    
                    token.JwtToken = null;
                    token.RefreshToken = null;
                    token.ResponseCode = Convert.ToInt32(_configuration["ResponsesCodes:FailureCode"]);
                    token.ResponseMessage = _configuration["Responses:RefreshTokenFailMessage"];
                    return StatusCode(StatusCodes.Status401Unauthorized, token);
                }
                else
                {
                    token.ResponseCode = Convert.ToInt32(_configuration["ResponsesCodes:SuccessCode"]);
                    token.ResponseMessage = _configuration["Responses:RefreshTokenSuccessMessage"];
                    return Ok(token);
                }
            }
            catch (Exception ex)
            {
                token.JwtToken = null;
                token.RefreshToken = null;
                token.ResponseCode = Convert.ToInt32(_configuration["ResponsesCodes:ExceptionCode"]);
                token.ResponseMessage = ex.Message;
                return Ok(token);
            }
        }
    }
}
