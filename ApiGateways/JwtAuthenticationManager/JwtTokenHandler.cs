using JwtAuthenticationManager.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthenticationManager;

public class JwtTokenHandler
{
    public const string JWT_SECURITY_KEY = "96bbfbf90b024a5bbbb68d0729aaa6d8"; // Needs to be placed securely
    private const int JWT_TOKEN_VALITITY_MINS = 20;

    private readonly IList<UserAccount> _userAccounts;
    public JwtTokenHandler()
    {
       _userAccounts = new List<UserAccount>();
        _userAccounts.Add(new UserAccount() 
        {
            UserName="admin",
            Password="admin123",
            Role="Administrator"
        });
        _userAccounts.Add(new UserAccount()
        {
            UserName = "user1",
            Password = "user1",
            Role = "User"
        });
    }

    public AuthenticationResponse? GenerateJwtToken(AuthenticationRequest request)
    {
        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password)) return null;

        var userAcount = _userAccounts.FirstOrDefault(u => u.UserName == request.UserName &&
                                                        u.Password == request.Password);
        if (userAcount == null) return null;

        var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(JWT_TOKEN_VALITITY_MINS);
        var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);
        var claimsIdentity = new ClaimsIdentity(new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, request.UserName),
            new Claim("Role",userAcount.Role)
            //Used "Role" instead of ClaimType.Role to use it in Ocelot
        });

        var signingCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(tokenKey),
                            SecurityAlgorithms.HmacSha256Signature);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject=claimsIdentity,
            Expires =tokenExpiryTimeStamp,
            SigningCredentials = signingCredentials
        };
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
        var token = jwtSecurityTokenHandler.WriteToken(securityToken);

        return new AuthenticationResponse
        {
            UserName= userAcount.UserName,
            ExpiresIn =(int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds,
            JwtToken = token
        };
    }

}
