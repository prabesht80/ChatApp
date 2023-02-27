using DataLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ServiceLayer
{
    public class LoginOperations
    {
        private readonly ChatRepository _repository;

        public LoginOperations(ChatRepository repository)
        {
            _repository = repository;
        }

        public User? GetUserById(int id)
        {
            var userList = _repository.GetListOfUsers();
            var foundUser = userList.FirstOrDefault(u => u.UserId == id);
            return foundUser;
        }
        public User? GetUserByUsername(string user)
        {
            var userList = _repository.GetListOfUsers();
            var foundUserByUsername = userList.FirstOrDefault(u => u.Username == user);
            return foundUserByUsername;
        }

        // Check if user with such username & password exists; return user
        public User? GetValidUser(User user)
        {
            var userList = _repository.GetListOfUsers();
            var validUser = userList.FirstOrDefault(
                u => u.Username == user.Username && BCrypt.Net.BCrypt.Verify(user.Password, u.Password));
            return validUser;
        }

        // Add new registered user to db
        public void Register(User user)
        {
            var hashedUser = new User()
            {
                Username = user.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Email = user.Email
            };
            _repository.Register(hashedUser);
        }

        // Method to update password
        public User UpdateUserPassword(User user, string newPassword)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return _repository.UpdateUserPassword(user);
        }

        // Check password is secure
        public bool IsSecurePassword(string password)
        {
            var regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            var match = Regex.Match(password, regex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return false;
            }

            return true;
        } 
        
        // Method to sign in user, asign claims
        public async Task CreateAuthentication(User user, HttpContext context)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties();

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);
        }

        // Method to get user that is authenticated by id stored in claims
        public User? GetUserByClaim(ClaimsPrincipal principal)
        {
            //get user claim(by id)
            var userClaim = principal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier)
                   .Select(c => c.Value).SingleOrDefault();
            int userId;

            if (!Int32.TryParse(userClaim, out userId))
            {
                return null;
            }

            return GetUserById(userId);
        }
    }
}