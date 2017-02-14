using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Укажите свой Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Придумайте и введите свое уникальное имя")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Повторите ввод пароля")]
        [Compare("Password")]
        public string PasswordConfirmation { get; set; }

        [Required(ErrorMessage = "Укажите свою дату рождения")]
        public DateTime BirthDate { get; set; }
    }
}