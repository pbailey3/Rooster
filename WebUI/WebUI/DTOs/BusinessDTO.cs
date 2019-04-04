using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebUI.Common;

namespace WebUI.DTOs
{
    public class BusinessDetailsDTO
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(40)]
        public string Name { get; set; }

        [Display(Name = "Has multiple business locations")]
        public bool HasMultiBusLocations { get; set; }
        [Display(Name = "Has internal locations")]
        public bool HasMultiInternalLocations { get; set; }

        //Business type properties
        [MustBeSelectedAttribute(ErrorMessage = "Please select business type details")]
        [Display(Name = "Type")]
        public int TypeId { get; set; }
        [Required]
        [Display(Name = "Business industry")]
        public string TypeIndustry { get; set; }
        [Display(Name = "Business type")]
        public string TypeDetail { get; set; }

        //Location
        public BusinessLocationDTO BusinessLocation { get; set; }

        ////Roles
        //public List<RoleDTO> Roles { get; set; }

        ////ShiftBlocks
        //public List<ShiftBlockDTO> ShiftBlocks { get; set; }



    }

    public class BusinessPreferencesDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BusinessLocationId { get; set; }
        [DisplayName("Allow staff to cancel shifts? Manager approval is still required")]
        public bool CancelShiftAllowed { get; set; }
        [DisplayName("Timeframe in which an employee can request to cancel a shift")]
        public int CancelShiftTimeframe { get; set; }
    }

    public class BusinessLocationSummaryDTO
    {
        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool HasInternalLocations { get; set; }
        public bool? UserIsAdmin { get; set; }
    }
    public class BusinessLocationDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }

        [Required]
        [DisplayName("Location name")]
        public string Name { get; set; }

        [Required]
        public string Phone { get; set; }

        public AddressModelDTO Address { get; set; }
       
        public bool Enabled { get; set; }

        //Location
        public List<InternalLocationDTO> InternalLocations { get; set; }

        //ShiftBlocks
        public List<ShiftBlockDTO> ShiftBlocks { get; set; }

        //Location
        public List<InternalLocationDTO> GetEnabledInternalLocations()
        {
            return this.InternalLocations.Where(l => l.Enabled == true).ToList();
        }
    }
    public class BusinessDTO
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(40)]
        public string Name { get; set; }

        //Business type properties
        [MustBeSelectedAttribute(ErrorMessage = "Please Select Business Type details")]
        public int TypeId { get; set; }
        [Required]
        [Display(Name = "Business Industry")]
        public string TypeIndustry { get; set; }
        [Display(Name = "Business Type")]
        public string TypeDetail { get; set; }
        [Display(Name = "Has multiple business locations")]
        public bool HasMultiBusLocations { get; set; }
        [Display(Name = "Has internal locations")]
        public bool HasMultiInternalLocations { get; set; }

        //Location
        public List<BusinessLocationDTO> BusinessLocations { get; set; }

        //Roles: NOTE, this also returns disabled roles. For enabled roles only use property "EnabledRoles"
        public List<RoleDTO> Roles;

        [JsonIgnore]
        public List<RoleDTO> EnabledRoles
        {
            get
            {
                return this.Roles.Where(r => r.Enabled).ToList();
            }
        }
    }
}