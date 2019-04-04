using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class eWayCustomerRequestDTO
    {
        public string Reference { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
}
    public class eWayRequestDTO
    {
        public string RedirectUrl { get; set; }
        public string CancelUrl { get; set; }
        public string Method { get; set; }
        public string TransactionType { get; set; }
        public string HeaderText { get; set; }
        public string LogoUrl { get; set; }

        public eWayCustomerRequestDTO Customer { get; set; }
    }

    public class eWayCustomerResponseDTO
    {
        public string CardNumber { get; set; }
        public string CardName { get; set; }
        public string CardExpiryMonth { get; set; }
        public string CardExpiryYear { get; set; }
        public string CardStartMonth { get; set; }
        public string CardStartYear { get; set; }
        public string CardIssueNumber { get; set; }
        public bool IsActive { get; set; }
        public long TokenCustomerID { get; set; }
        public string Reference { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string JobDescription { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Comments { get; set; }
        public string Fax { get; set; }
        public string Url { get; set; }
    }

    public class eWayResponseDTO
    {
        public string SharedPaymentUrl { get; set; }
        public string AccessCode { get; set; }
        public string Errors { get; set; }
    }

    public class eWayAccessCodeResponseDTO
    {
        public string TokenCustomerID { get; set; }
        public string Errors { get; set; }
    }
}