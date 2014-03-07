﻿using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models
{
    public class LoginModel
    {
        [Required]
        public string Login { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}