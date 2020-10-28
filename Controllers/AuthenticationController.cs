using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using demo.DTOModels;
using demo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace demo.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private readonly TokenParameter _tokenParameter;
        public AuthenticationController(IConfiguration configuration)
        {
            _tokenParameter = configuration.GetSection("tokenParameter").Get<TokenParameter>();
        }

        [HttpPost, Route("requestToken")]
        public ActionResult RequestToken([FromBody] LoginRequestDto request)
        {
            //这儿在做用户的帐号密码校验。我这儿略过了。
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Invalid Request");
            }

            if (request.Username != _tokenParameter.ClientId || request.Password != _tokenParameter.ClientScrect)
            {
                return BadRequest("Invalid UserName or Password");
            }

            //生成Token和RefreshToken
            var token = GenUserToken(request.Username, "testUser");
            var refreshToken = "123456";//改成随机的
            return Ok(new { Token=token, RefreshToken= refreshToken });
        }

        //这儿是真正的生成Token代码
        private string GenUserToken(string username, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenParameter.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(_tokenParameter.Issuer, null, claims, expires: DateTime.UtcNow.AddMinutes(_tokenParameter.AccessExpiration), signingCredentials: credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return token;
        }

        [HttpPost, Route("refreshToken")]
        public ActionResult RefreshToken([FromBody] RefreshTokenDto request)
        {
            if (request.Token == null && request.RefreshToken == null)
                return BadRequest("Invalid Request");

            //这儿是验证Token的代码
            var handler = new JwtSecurityTokenHandler();
            try
            {
                ClaimsPrincipal claim = handler.ValidateToken(request.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenParameter.Secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                }, out SecurityToken securityToken);

                var username = claim.Identity.Name;

                //这儿是生成Token的代码
                var token = GenUserToken(username, "testUser");

                var refreshToken = "654321";//改成随机的

                return Ok(new[] { token, refreshToken });
            }
            catch (Exception)
            {
                return BadRequest("Invalid Request");
            }
        }
    }
}