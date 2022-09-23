using logindemo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace logindemo.DAL
{
    public interface IGetDataReadersAsync
    {
        Task<List<UserLoginResponse>> GetUserLoginDataAsync(UserLoginRequest userLoginRequest);
        Task<List<UserLoginResponse>> AuthenticateUser(UserLoginRequest userLoginRequest);
    }
}


