using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class AddCardDetailsDTO
    {
        public string SharedPaymentUrl { get; set; }
    }

    public class PaymentDetailsDTO
    {
        public Guid EmployeeID { get; set; }
        [Display(Name = "First name")]
        public string EmployeeFirstName { get; set; }
        [Display(Name = "Last name")]
        public string EmployeeLastName { get; set; }
        public Guid BusinessLocationID { get; set; }
        public long TokenCustomerID{ get; set; }
        [Display(Name = "Created date")]
        public DateTime CreatedDate { get; set; }
    }
}