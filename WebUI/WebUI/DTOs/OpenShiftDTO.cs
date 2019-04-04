using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class OpenShiftDTO
    {
        public Guid Id { get; set; }
        public Guid LoctionId { get; set; }
        public Nullable<int> TotalPayID { get; set; }
        public Nullable<int> DistanceID { get; set; }
        public Nullable<int> FilterID { get; set; }
        public string Name { get; set; }
        public List<ShiftDTO> Shifts { get; set; }
        public OpenShiftsEmployeeIndexDTO OpenShiftsEmployee { get; set; }
        public ExternalShiftEmployeeIndexDTO ExternalShiftBroadCastEmpoyee { get; set; }
        public List<DropDownDTO> Distance { get; set; }
        public List<DropDownDTO> Filter { get; set; }
        public List<openShiftsList> OpenShiftList { get; set; }
        //Address properties  
        public OpenShiftAddress Address { get; set; }
        public double distance { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsExternalandEmp { get; set; }
    }

    public class openShiftsList
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public string locationName { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public double totalhourse { get; set; }
        public decimal rate { get; set; }
        public decimal totalPrice { get; set; }
        public bool IsExternalShift { get; set; }
        //if an External user 
        public string Description { get; set; }
        public int Distance { get; set; }
    }

    public class OpenShiftAddress
    {
        public string Line1 { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Line2 { get; set; }
        public string PlaceId { get; set; }
        public Nullable<double> PlaceLatitude { get; set; }
        public Nullable<double> PlaceLongitude { get; set; }
    }
}