using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Введите свой логин")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(3)]
        public string Password { get; set; }
    }
}