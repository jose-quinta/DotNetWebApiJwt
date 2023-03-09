using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Server.Services.UserServices {
    public class UserService : IUserService {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;
        public string GetName() {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null) {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }
        public string GetRole() {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null) {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            }
            return result;
        }
    }
}