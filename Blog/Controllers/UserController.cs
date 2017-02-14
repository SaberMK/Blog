using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
namespace Blog.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View(new BlogModelDataContext().Users.ToList());
        }

        [HttpGet]
        public ActionResult Info(int? id)
        {
            if (id.HasValue)
            {
                Users usr = new BlogModelDataContext().Users.SingleOrDefault(x => x.UserId == id.Value);
                if (usr == null)
                    return RedirectToAction("UserNotFound", "User");
                return View(usr);
            }
            return RedirectToAction("UserNotFound", "User");
        }

        public ActionResult UserNotFound()
        {
            return View();
        }
    }
}