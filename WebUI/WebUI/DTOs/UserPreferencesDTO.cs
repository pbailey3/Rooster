using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI.DTOs
{
    public class UserPreferencesDTO
    {

        public System.Guid Id { get; set; }
        public System.Guid UserProfileId { get; set; }
        public String UserProfileFirstName { get; set; }
        public String UserProfileEmail { get; set; }
        [Display(Name = "Receive notifications of available shifts at your employers?")]
        public bool InternalAvailableShifts { get; set; }
        [Display(Name = "Receive information about external shifts?")]
        public bool ExternalShiftInfo { get; set; }
        [Display(Name = "Receive notifications of available external shifts?")]
        public bool ExternalAvailableShifts { get; set; }
        [Display(Name = "Distance you would be willing to travel for external roles and shifts?")]
        public int DistanceTravel { get; set; }
        [Display(Name = "Shift reminder notifications - how far many hours before shift?")]
        public int ShiftReminderLength { get; set; }
        [Display(Name = "Receive notification by App")]
        public bool NotifyByApp { get; set; }
        [Display(Name = "Receive notification by SMS")]
        public bool NotifyBySMS { get; set; }
        [Display(Name = "Receive notification by Email")]
        public bool NotifyByEmail { get; set; }
        [Display(Name = "Show 24Hr time format")]
        public bool TimeFormat24Hr { get; set; }
        [Display(Name = "Default monthly calendar view")]
        public bool MonthCalView { get; set; }
        [DataType(DataType.Upload)]
        [Display(Name = "Image")]
        public byte[] ImageData { get; set; }
        public string ImageType{ get; set; }
    }
}
