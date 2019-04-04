using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using System.Web.Security;

namespace WebUI.DTOs
{
    public class LoginModelDTO
    {
        [Required]
        public string Email{ get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }


    public class ResetPasswordDTO
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }


    public class OAuthLoginModelDTO
    {
        public string ProviderName { get; set; }
        public string ProviderUserID { get; set; }
    }

    public class OAuthLoginModelResultDTO : OAuthLoginModelDTO
    {
        public bool AccountLinked { get; set; }
        public string HashedPassword { get; set; }
        public string UserEmail { get; set; }
    }
    public class AddressModelDTO
    {
        //Address properties  
        [Required]
        [Display(Name = "Address Line1")]
        public string Line1 { get; set; }
        [Display(Name = "Address Line2")]
        public string Line2 { get; set; }
        [Required]
        [Display(Name = "Suburb")]
        public string Suburb { get; set; }
        [Required]
        [Display(Name = "State")]
        public string State { get; set; }
        [Required]
        [Display(Name = "Postcode")]
        public string Postcode { get; set; }
        public double? Long { get; set; }
        public double? Lat { get; set; }
        public string PlaceId { get; set; }
        // public DbGeography Location { get; set; } 

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(this.Line1))
                sb.Append(this.Line1).Append(", ");
            if (!String.IsNullOrEmpty(this.Line2))
                sb.Append(this.Line2).Append(", ");
            if (!String.IsNullOrEmpty(this.Suburb))
                sb.Append(this.Suburb).Append(", ");
            if (!String.IsNullOrEmpty(this.State))
                sb.Append(this.State).Append(", ");
            if (!String.IsNullOrEmpty(this.Postcode))
                sb.Append(this.Postcode);

            return sb.ToString().TrimEnd(new char[] { ',', ' ' });
        }
    }
    public class RegisterModelDTO
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(20)]
        [Display(Name="First name")]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(20)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Mobile number")]
        [RegularExpression(@"^(\d{10})$", ErrorMessage = "Invalid mobile number")]
        public string MobilePhone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public Nullable<System.DateTime> DateofBirth { get; set; }

        //Address properties  
        public AddressModelDTO Address { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept terms and conditions")]
        public bool TermsAndConditions { get; set; }

        //OAuth provider details
        public string ExternalLoginData { get; set; }
        public string ExternalProvider { get; set; }
        public string ExternalProviderUserId { get; set; }

        //User profil pic
        public byte[] ImageData { get; set; }
        public string ImageType { get; set; }
       
    }

    //public class ExternalLogin
    //{
    //    public string Provider { get; set; }
    //    public string ProviderDisplayName { get; set; }
    //    public string ProviderUserId { get; set; }
    //}
}
