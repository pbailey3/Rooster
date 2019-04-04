using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUI.Models;

namespace WebUI.DTOs
{
    public class MessagesDTO
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public Nullable<System.DateTime> DateSent { get; set; }

        public UserProfile From { get; set; }
        public UserProfile To { get; set; }
    }
}