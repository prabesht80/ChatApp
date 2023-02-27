using HermesChatApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ServiceLayer;
using DataLayer;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace HermesChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LoginOperations _loginOperator;
        private readonly IMapper _mapper;

        public HomeController(ILogger<HomeController> logger, LoginOperations loginOperator, IMapper map)
        {
            _logger = logger;
            _loginOperator = loginOperator;
            _mapper = map;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserModel userModel)
        {
            // check if username and password aren't empty
            if (userModel.Username == null || userModel.Password == null)
            {
                return View(userModel);
            }

            // check if user is valid
            var mappedUser = _mapper.Map<UserModel, User>(userModel);
            var validUser = _loginOperator.GetValidUser(mappedUser);
            if (validUser == null)
            {
                ViewBag.error = "Invalid Account";
                return View(userModel);
            }

            //authenticate user
            await _loginOperator.CreateAuthentication(validUser, HttpContext);
            return RedirectToAction("Index", "Chat");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // logging out user(removing cookie)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                //check if user with username exists
                var mappedUser = _mapper.Map<UserModel, User>(userModel);
                var foundUserByUsername = _loginOperator.GetUserByUsername(mappedUser.Username);
                if (foundUserByUsername != null)
                {
                    ViewBag.error = "User with this name already exists";
                    return View(userModel);
                }
                else
                {
                    //adding new user to db
                    var newUser = _mapper.Map<UserModel, User>(userModel);
                    _loginOperator.Register(newUser);
                    ModelState.Clear();
                    TempData["SuccessfulRegister"] = "Person successfully created";
                    return RedirectToAction("Index");
                }
            }
            if (!ModelState.IsValid)
            {
                return Register();
            }
            return LocalRedirect("~/Home/Index");
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var user = _loginOperator.GetUserByClaim(User);
            if (user == null)
            {
                ViewBag.error = "Please Logout. Account Error.";
                return RedirectToAction("Index", "Chat");
            }
            var userModel = _mapper.Map<User, UserModel>(user);
            return View(userModel);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}