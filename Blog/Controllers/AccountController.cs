using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
namespace Blog.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegistrationViewModel rvm)
        {
            var db = new BlogModelDataContext();
            if (ModelState.IsValid)
            {
                if(db.Users.Any(x=>x.Login==rvm.Login || x.Email==rvm.Email))
                {
                    ModelState.AddModelError("HasInSystem", "Пользователь уже существует");
                    return View();
                }
                else
                {
                    Users user = new Users();
                    user.Email = rvm.Email;
                    user.Password = rvm.Password;
                    user.Login = rvm.Login;
                    user.BirthDate = rvm.BirthDate;
                    user.About = rvm.About ?? "";
                    db.Users.InsertOnSubmit(user);
                    db.SubmitChanges();

                    int id = db.Users.Single(x => x.Login == rvm.Login).UserId;

                    return RedirectToAction("Index", "Home", new { id = id });
                }
            }
            else
                return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel lvw)
        {
            if (ModelState.IsValid)
            {
                Users currentUser = new BlogModelDataContext().Users.SingleOrDefault(x => x.Login == lvw.Login && x.Password == lvw.Password);
                if (currentUser != null)
                {
                    Session["id"] = currentUser.UserId;
                    Session["login"] = currentUser.Login;
                    return RedirectToAction("Index", "Home", new { id = currentUser.UserId });
                }
                ModelState.AddModelError("NotExist", "Такого пользователя не существует!");
                return View();
            }
            else
                return View();
        }

        public ActionResult Logoff()
        {
            Session["id"] = null;
            return RedirectToAction("Index", "Home");
        }
    }
}