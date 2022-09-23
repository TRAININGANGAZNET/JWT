using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using JWT.JWT;
using logindemo.Model;
using Microsoft.Extensions.Configuration;
using Npgsql;
using BCHash = BCrypt.Net.BCrypt;
using logindemo.Controllers;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace logindemo.DAL
{
    public class GetDataReadersAsync : IGetDataReadersAsync
    {

        private readonly IConfiguration _configuration;
        private readonly string myConnectionString;

        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        public readonly ITokenRefresher _tokenRefresher;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        public GetDataReadersAsync(IConfiguration configuration, IJwtAuthenticationManager jwtAuthenticationManager, ITokenRefresher tokenRefresher, IRefreshTokenGenerator refreshTokenGenerator)
        {
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _refreshTokenGenerator = refreshTokenGenerator;
            _tokenRefresher = tokenRefresher;
            _configuration = configuration;
            myConnectionString = _configuration["ConnectionStrings:DBConnectionString"];
        }

        public async Task<List<UserLoginResponse>> AuthenticateUser(UserLoginRequest userLoginRequest)
        {
            List<UserLoginResponse> listUserLoginResponse = await this.GetUserLoginDataAsync(userLoginRequest);
                if (listUserLoginResponse.Count > 0 )
                {
                //string username = userLoginRequest.loginname;
                string username = userLoginRequest.USERNAME;
                var AccessToken = _jwtAuthenticationManager.GenerateTokenString(username, DateTime.UtcNow);
                var RefreshToken = _refreshTokenGenerator.GenerateRefreshToken();
                //listUserLoginResponse[0].JwtToken = AccessToken;
                //listUserLoginResponse[0].RefreshToken = RefreshToken;
                listUserLoginResponse[0].AccessToken = AccessToken;
                //listUserLoginResponse[0].RefreshToken = RefreshToken;

                if (_jwtAuthenticationManager.UserRefreshTokens.ContainsKey(username))
                {
                    _jwtAuthenticationManager.UserRefreshTokens[username] = RefreshToken;
                }
                else
                {
                    _jwtAuthenticationManager.UserRefreshTokens.Add(username, RefreshToken);
                }
            }
            return listUserLoginResponse;
        }

        public async Task<List<UserLoginResponse>> GetUserLoginDataAsync(UserLoginRequest userLoginRequest)
        {
            string sqlGetData = _configuration["Queries:GetUserLoginData"];
            try
            {

                //return await LoadData<UserLoginResponse, dynamic>(sqlGetData, new { userLoginRequest.loginname }, myConnectionString);
                return await LoadData<UserLoginResponse, dynamic>(sqlGetData, new { userLoginRequest.USERNAME }, myConnectionString);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<T>> LoadData<T, U>(string sql, U Parameters, string connectionstring)
        {
            try
            {
                using (IDbConnection connection = new SqlConnection(connectionstring))
                {
                    var rows = await connection.QueryAsync<T>(sql, Parameters);
                    return rows.ToList();
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
