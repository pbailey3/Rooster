using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
   
    public class RequestsDTO
    {
        public IEnumerable<EmployerRequestDTO> EmployerRequests { get; set; }
        public ShiftChangeRequestListsDTO ShiftChangeRequests { get; set; }
        public ExternalShiftRequestListsDTO ExternalShiftRequests { get; set; }
    }
}