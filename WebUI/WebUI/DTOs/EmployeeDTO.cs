using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using WebUI.Common;
using System.Web.Mvc;

namespace WebUI.DTOs
{
    public class EmployeeRoleSummaryDTO
    {
        public Guid Id { get; set; }
        public String FullName { get; set; }
        public List<Guid> Roles { get; set; }
    }

    public class EmployeeSummaryDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobilePhone { get; set; }
        //Properties
        public string FullName { get { return this.FirstName + " " + this.LastName; } }

    }

    public enum EmployeeTypeDTO : int
    {
        Full_Time = 0,
        Part_Time = 1,
        Casual = 2,
        External = 3
    }

    public class EmployeeRequestDTO
    {
        public Guid Id { get; set; }
        public RequestStatusDTO Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid BusinessLocation_Business_Id { get; set; }
        public string BusinessLocation_Business_Name { get; set; }
        public Guid BusinessLocation_Id { get; set; }
        public string BusinessLocation_Name { get; set; }
    }


    public class EmployeeDTO
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(20)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [MaxLength(20)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Display(Name = "Mobile number")]
        [RegularExpression(@"^(\d{10})$", ErrorMessage = "Invalid mobile number")]
        public string MobilePhone { get; set; }
        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Pay rate")]
        [RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "Amount must be a number with two decimal place")]
        [Range(0.01, 999.99)]
        [DataType(DataType.Currency)]
        public Decimal? PayRate { get; set; }
        [MustBeSelectedAttribute(ErrorMessage = "Please select employee type details")]
        public EmployeeTypeDTO Type { get; set; }
        [Display(Name = "Administrator rights")]
        public bool IsAdmin { get; set; }
        //Linked Business ID
        public Guid BusinessId { get; set; }
        //Linked Business ID
        public Guid BusinessLocationId { get; set; }
        //Linked Userprofile ID
        public Guid UserProfileId { get; set; }

        public bool IsActive { get; set; }

        public byte[] QRCode { get; set; }

        public bool AddAnother { get; set; }

        //Roles
        public List<RoleDTO> Roles { get; set; }

        //Properties
        public string FullName { get { return this.FirstName + " " + this.LastName; } }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        #region View Helper Methods
        public IEnumerable<SelectListItem> EmployeeTypesSelectList()
        {
            return Enum.GetNames(typeof(EmployeeTypeDTO)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
        }
        #endregion
    }
}