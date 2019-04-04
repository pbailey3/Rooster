using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebUI.Common;

namespace WebUI.DTOs
{
    public enum EmployerSearchTypeDTO : int
    {
        Business_Location_Name = 0,
        Manager_Name = 1
    }

    public enum RequestStatusDTO : int
    {
        Pending = 0,
        Approved = 1,
        Denied = 2
    }

    public class EmployerSearchDTO
    {
        public Guid id { get; set; }
        [MustBeSelectedAttribute(ErrorMessage = "Please Select Search Type details")]
        [Display(Name = "Search type")]
        public EmployerSearchTypeDTO SearchType { get; set; }

        [Required]
        [MinLength(4)]
        public string Name { get; set; }

        public List<BusinessLocationSummaryDTO> SearchResults { get; set; }
        public List<EmployerRequestDTO> SearchRequests { get; set; }
        public List<Guid> CurrentBusinesses { get; set; } //List of Business ID's the user is already currently an employee of

    }

    public class EmployerRequestDTO
    {
        public Guid Id { get; set; }
        public RequestStatusDTO Status { get; set; }
        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; }
        public Guid Business_Id { get; set; }
        public Guid BusinessLocation_Id { get; set; }
        [Display(Name = "Business Name")]
        public string Business_Name { get; set; }
        [Display(Name = "Location Name")]
        public String Location_Name { get; set; }
        [Display(Name = "Requestor")]
        public string Requester_Name { get; set; }

    }

    public class EmployerSummaryDTO
    {
        public List<BusinessLocationSummaryDTO> Employers { get; set; }
        public List<EmployeeRequestDTO> EmployeeRequests { get; set; }
    }


}
