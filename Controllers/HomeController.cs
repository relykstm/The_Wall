using System;
using Microsoft.AspNetCore.Mvc;
using The_Wall.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace The_Wall.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet("logout")]

        public ViewResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        [HttpGet("")]

        public ViewResult Index()
        {
            
            return View(); 
        }
        [HttpPost("register")]
        public IActionResult RegisterUser(RegisterUser regFromForm)
        {

            if(ModelState.IsValid)
            {
                PasswordHasher<RegisterUser> Hasher = new PasswordHasher<RegisterUser>();
                regFromForm.Password = Hasher.HashPassword(regFromForm, regFromForm.Password);

                if(dbContext.Users.Any(u => u.Email == regFromForm.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                else{
                    dbContext.Add(regFromForm);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetObjectAsJson("LoggedInUser", regFromForm);
                    return RedirectToAction("Success");
                }
            }

            return View("Index");

        }
        [HttpPost("login")]
        public IActionResult LoginUser(LoginUser LoginFromForm)
        {
           if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == LoginFromForm.LoginEmail);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LoginEmail", "Invalid Email or Password");
                    return View("Index");
                }
                
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(LoginFromForm, userInDb.Password, LoginFromForm.LoginPassword);
                
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email or Password");
                    return View("Index");
                } 
                else
                {
                    HttpContext.Session.SetObjectAsJson("LoggedInUser", userInDb);
                    return RedirectToAction("Success");
                }
                
            }

            return View("Index");
        }
        [HttpGet("yourwall")]

        public IActionResult Success()
        {
            RegisterUser fromLogin = HttpContext.Session.GetObjectFromJson<RegisterUser>("LoggedInUser");
            if(fromLogin == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.LoggedInUser = fromLogin;

            List<Message> AllMessages = Enumerable.Reverse(dbContext.Messages
                .Include(m => m.User)
                .Include(m => m.Comments)
                .ThenInclude(c => c.User))
                .ToList();

            Wrapper Wrapper = new Wrapper();
            Wrapper.Messages = AllMessages;

            return View("Main", Wrapper);
        }


        [HttpPost("newmessage")]

        public IActionResult NewMessage(int UserId, Wrapper fromForm)
        {
        
            fromForm.Message.UserId = UserId;
            if(ModelState.IsValid)
            {
                dbContext.Add(fromForm.Message);
                dbContext.SaveChanges();
                return RedirectToAction("Success");
            }
            RegisterUser fromLogin = HttpContext.Session.GetObjectFromJson<RegisterUser>("LoggedInUser");
            ViewBag.LoggedInUser = fromLogin;
            List<Message> AllMessages = Enumerable.Reverse(dbContext.Messages
            .Include(m => m.User)
            .Include(m => m.Comments)
            .ThenInclude(c => c.User))
            .ToList();

            Wrapper Wrapper = new Wrapper();
            Wrapper.Messages = AllMessages;
            return View("Main", Wrapper);
        }

        [HttpPost("newComment")]

        public IActionResult NewComment(int UserId, Wrapper fromForm)
        {
            fromForm.Comment.UserId = UserId;
            if(ModelState.IsValid)
            {
                dbContext.Add(fromForm.Comment);
                dbContext.SaveChanges();
                return RedirectToAction("Success");
            }
            RegisterUser fromLogin = HttpContext.Session.GetObjectFromJson<RegisterUser>("LoggedInUser");
            ViewBag.LoggedInUser = fromLogin;
            List<Message> AllMessages = Enumerable.Reverse(dbContext.Messages
            .Include(m => m.User)
            .Include(m => m.Comments)
            .ThenInclude(c => c.User))
            .ToList();
            Wrapper Wrapper = new Wrapper();
            Wrapper.Messages = AllMessages;
            return View("Main", Wrapper);
        }





    }
        public static class SessionExtensions
    {
        // We can call ".SetObjectAsJson" just like our other session set methods, by passing a key and a value
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            // This helper function simply serializes theobject to JSON and stores it as a string in session
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        
        // generic type T is a stand-in indicating that we need to specify the type on retrieval
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            // Upon retrieval the object is deserialized based on the type we specified
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}