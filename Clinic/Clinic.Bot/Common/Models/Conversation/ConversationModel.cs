using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Bot.Common.Models.Conversation
{
    public class ConversationModel
    {
        public string id { get; set; }
        public string idUser { get; set; }
        public DateTime registerDate { get; set; }
        public string message { get; set; }
    }
}
