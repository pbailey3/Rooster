using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.NotificationHubs;
using System.Configuration;

namespace WebUI.Models
{
    class Notifications
    {
        public static Notifications Instance = new Notifications();

        public NotificationHubClient Hub { get; set; }

        private Notifications()
        {
             var notificationHubConnString = ConfigurationManager.ConnectionStrings["NotificationHub"].ConnectionString;
             var notificationHubName = ConfigurationManager.ConnectionStrings["NotificationHub"].ProviderName;

             Hub = NotificationHubClient.CreateClientFromConnectionString(notificationHubConnString, notificationHubName);
        }
    }
}
