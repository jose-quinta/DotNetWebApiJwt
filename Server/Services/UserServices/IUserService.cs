using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services.UserServices {
    public interface IUserService {
        string GetName();
        string GetRole();
    }
}