using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUI.Models;

namespace WebUI.DTOs
{
    public class UserRecommendationsDTO
    {
        public Guid Id { get; set; }
        public IEnumerable<Guid> UserRecommendedByList { get; set; }
        public IEnumerable<Guid> UserBeingRecommendedList { get; set; }
        public Guid RecommendedUser { get; set; }
        public UserProfilesDTO UserRecommending { get; set; }
        public UserProfilesDTO UserBeingRecommended { get; set; }
    }
}