﻿namespace WB.UI.Designer.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The password model.
    /// </summary>
    public class PasswordModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the confirm password.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password", Order = 3)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        [Required]
        [PasswordStringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.")]
        [PasswordRegularExpression(ErrorMessage = "Password must contain at least one number, one upper case character and one lower case character")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }

        #endregion
    }
}