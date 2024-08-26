using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RemoteController.Common.Dtos.API;
using RemoteController.Common.Dtos.API.Equipment;
using RemoteController.Common.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RemoteController.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentsController : ControllerBase
    {
        private readonly ILogger<EquipmentsController> _logger;
        private readonly ConnectionsManager _connectionManager;
        private readonly IConfiguration _configuration;
        public EquipmentsController(ILogger<EquipmentsController> logger, ConnectionsManager connectionManager, IConfiguration configuration)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _configuration = configuration;
        }

        [Route("regist")]
        [HttpPost]
        public ActionResult RegistAsync(RegistDto registDto)
        {
            try
            {
                var result = _connectionManager.Regist(registDto);

                return new ServiceResult<RegistResponseDto>(new RegistResponseDto()
                {
                    Token = result ? CreateToken(registDto.EquipmentId) : null,
                    Success = result
                }).ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        private string CreateToken(string id)
        {
            var claims = new[]
             {
                new Claim(ClaimTypes.Name, id),
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:JwtSecret"]));
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(secretKey, algorithm);
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["System:Name"],    //Issuer
                _configuration["Jwt:Audience"],   //Audience
                claims,                          //Claims,
                DateTime.Now,                    //notBefore
                DateTime.Now.AddSeconds(_configuration.GetValue<int>("Jwt:TokenExpireSeconds")),    //expires
                signingCredentials               //Credentials
            );
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return token;
        }
    }
}
