using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public enum RosterToCreateEnum : int
    {
        Week = 0,
        Fortnight = 1,
        Month = 2
    }

    public enum ShiftRequestTypeDTO : int
    {
        Cancel = 0,
        Swap = 1,
        TakeOpenShift = 2
    }


    public enum ExternalShiftRequestTypeDTO : int
    {
        Cancel = 0,
        Swap = 1,
        TakeExternalShift = 2
    }
    public class ShiftDTO
    {
        public Guid Id { get; set; }
        public Guid RosterId { get; set; }
        public Guid? ExternalShiftBroadcastId { get; set; }
        //public ExternalBroadcastDTO ExternalShiftBroadcast { get; set; }
        [Display(Name = "Published")]
        public bool IsPublished { get; set; }
        public Guid? BusinessId { get; set; }
        public String BusinessName { get; set; }
        public Guid? BusinessLocationId { get; set; }
        [Display(Name = "Business Location")]
        public String BusinessLocationName { get; set; }
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }
        [Display(Name = "First Name")]
        public String EmployeeFirstName { get; set; }
        [Display(Name = "Last Name")]
        public String EmployeeLastName { get; set; }
        [Display(Name = "Pay rate")]
        public Decimal? EmployeePayRate { get; set; }

        [Display(Name = "Start date")]
        [DataType(DataType.Date)]
        public DateTime StartDay { get; set; }

        [Display(Name = "Start time")]
        //[Required(ErrorMessage = @"Please provide an end time.")]
        [RegularExpression(@"(20|21|22|23|[01]\d|\d)(([:][0-5]\d){1,2})", ErrorMessage = @"Please provide a valid time.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [JsonIgnore]
        public DateTime StartDateTime
        {
            get
            {
                return StartDay + StartTime;
            }
            set
            {
                this.StartDay = value.Date;
                this.StartTime = value.TimeOfDay;
            }
        }


        [Display(Name = "Finish date")]
        [DataType(DataType.Date)]
        public DateTime FinishDay { get; set; }

        [Display(Name = "Finish time")]
        //[Required(ErrorMessage = @"Please provide an end time.")]
        [RegularExpression(@"(20|21|22|23|[01]\d|\d)(([:][0-5]\d){1,2})", ErrorMessage = @"Please provide a valid time.")]
        [DataType(DataType.Time)]
        public TimeSpan FinishTime { get; set; }

        [JsonIgnore]
        public DateTime FinishDateTime
        {
            get
            {
                return FinishDay + FinishTime;
            }
            set
            {
                this.FinishDay = value.Date;
                this.FinishTime = value.TimeOfDay;
            }
        }

        [Display(Name = "Role")]
        public Guid RoleId { get; set; }
        [Display(Name = "Role")]
        public string RoleName { get; set; }

        [Display(Name = "Internal location")]
        public Guid InternalLocationId { get; set; }
        [Display(Name = "Internal location")]
        public string InternalLocationName { get; set; }

        public Guid? ShiftTemplateId { get; set; }

        [Display(Name = "Save Shift block?")]
        public bool SaveAsShiftBlock { get; set; }

        //Properties
        public string EmployeeFullName { get { return this.EmployeeFirstName + " " + this.EmployeeLastName; } }

        [Display(Name = "Length")]
        public TimeSpan ShiftLength
        {
            get
            {
                return FinishDateTime - StartDateTime;
            }
        }
    }

    public class RosterCreateDTO
    {
        public Guid BusinessId { get; set; }
        [Required]
        [Display(Name = "Business Location")]
        public Guid BusinessLocationId { get; set; }
        [Display(Name = "Week starting")]
        public DateTime WeekStartDate { get; set; }
        [Display(Name = "Roster(s) to create")]
        public RosterToCreateEnum RostersToCreate { get; set; }
        [Display(Name = "Use shift templates")]
        public bool UseShiftTemplates { get; set; }
    }

    public class RosterDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BusinessLocationId { get; set; }
        public String BusinessLocationName { get; set; }
        public DateTime WeekStartDate { get; set; }
        public List<ShiftDTO> Shifts { get; set; }
    }

    public class ShiftChangeActionDTO
    {
        public Guid Id { get; set; }
        [MaxLength(400)]
        public string Reason { get; set; }
    }
    public class ShiftChangeRequestListsDTO
    {
        public IEnumerable<ShiftChangeRequestDTO> ShiftChangeRequests { get; set; }
        public IEnumerable<RecurringShiftChangeRequestDTO> RecurringShiftChangeRequests { get; set; }
    }

    public class ShiftChangeRequestDTO
    {
        public Guid Id { get; set; }
        public Guid ShiftId { get; set; }
        public Guid ShiftTemplateId { get; set; }
        [Display(Name = "Employee")]
        public string EmployeeName { get; set; }
        [Display(Name = "Business")]
        public string BusinessName { get; set; }
        public Guid BusinessLocationId { get; set; }
        public string BusinessLocationName { get; set; }
        [Display(Name = "Start")]
        public DateTime StartDateTime { get; set; }
        [Display(Name = "Finish")]
        public DateTime FinishDateTime { get; set; }
        public ShiftRequestTypeDTO Type { get; set; }
        [MaxLength(400)]
        public string Reason { get; set; }
        public RequestStatusDTO Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class RecurringShiftChangeRequestDTO : ShiftChangeRequestDTO
    {
        [Display(Name = "Start")]
        public TimeSpan StartTime { get; set; }
        [Display(Name = "Finish")]
        public TimeSpan FinishTime { get; set; }
        [Display(Name = "Occurence Date")]
        public DateTime OccurenceDate { get; set; }
    }

    public class RosterBroadcastCheckDTO
    {
        public List<ShiftDTO> ConflictingShifts { get; set; }
        public List<RosterBroadcastDTO> RosterBroadCasts { get; set; }

    }

    public class RosterBroadcastDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BusinessLocationId { get; set; }
        public Guid LocationId { get; set; }
        public DayOfWeek Day { get; set; }
        public IEnumerable<ShiftDTO> UnassignedShifts { get; set; }
    }

    public class OpenShiftsEmployeeIndexDTO
    {
        public IEnumerable<ShiftDTO> OpenShifts { get; set; }
        public IEnumerable<Guid> OpenShiftRequests { get; set; }

    }


    public class ExternalShiftRequestDTO
    {
        public Guid Id { get; set; }
        public Guid ShiftId { get; set; }
        public Guid ExternalShiftBroadCastID { get; set; }
        public Guid ShiftTemplateId { get; set; }
        public byte[] ProfileImageData { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }
        [Display(Name = "Business")]
        public string BusinessName { get; set; }
        public Guid BusinessLocationId { get; set; }
        public string BusinessLocationName { get; set; }
        [Display(Name = "Start")]
        public DateTime StartDateTime { get; set; }
        [Display(Name = "Finish")]
        public DateTime FinishDateTime { get; set; }
        public ExternalShiftRequestTypeDTO Type { get; set; }
        [MaxLength(400)]
        public string Reason { get; set; }
        [MaxLength(1000)]
        public string Message { get; set; }
        public RequestStatusDTO Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }



    public class ExternalShiftRequestListsDTO
    {
        public IEnumerable<ExternalShiftRequestDTO> ExternalShiftRequests { get; set; }
    }

    public class ExternalShiftActionDTO
    {
        public Guid Id { get; set; }
        [MaxLength(400)]
        public string Reason { get; set; }
    }

    public class ExternalShiftEmployeeIndexDTO
    {
        public IEnumerable<ExternalBroadcastDTO> ExternalShits { get; set; }
        public IEnumerable<Guid> ExternalShiftRequests { get; set; }
    }

}