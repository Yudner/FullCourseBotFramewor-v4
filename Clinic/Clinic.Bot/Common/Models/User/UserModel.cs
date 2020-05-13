using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Bot.Common.Models.User
{
    public class UserModel
    {
        public string id { get; set; }
        public string userNameChannel { get; set; }
        public string channel { get; set; }
        public DateTime registerDate { get; set; }
        public string phone { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
    }
}
