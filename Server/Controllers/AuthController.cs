using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Entities;
using Server.Services.UserServices;

namespace Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public AuthController(ApplicationDbContext context,IConfiguration configuration, IUserService userService) {
            _context = context;
            _configuration = configuration;
            _userService = userService;
        }
        [HttpGet("Name"), Authorize]
        public ActionResult<object> GetName() {
            var name = _userService.GetName();
            return Ok(name);
        }
        [HttpGet("Role"), Authorize]
        public ActionResult<object> GetRole() {
            var role = _userService.GetRole();
            return Ok(role);
        }
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserDto request) {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User response = new User() {
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };

            _context.Add(response);
            await _context.SaveChangesAsync();

            return Ok(response);
        }
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserDto request) {
            var response = await _context.Users.Where(x => x.Username == request.Username).FirstOrDefaultAsync();
            if (response == null)
                return BadRequest("User not found.");
            if (!VerifyPasswordHash(request.Password, response.PasswordHash, response.PasswordSalt))
                return BadRequest("Wrong password.");
            string token = CreateToken(response);
            return Ok(token);
        }
        private string CreateToken(User request) {
            List<Claim> claims = new List<Claim>() {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using (var hmac = new HMACSHA512()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt) {
            using (var hmac = new HMACSHA512(passwordSalt)) {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}