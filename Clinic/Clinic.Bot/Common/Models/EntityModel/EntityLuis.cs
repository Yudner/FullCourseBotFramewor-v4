using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Bot.Common.Models.EntityModel
{
    public class EntityLuis
    {
        public List<DatetimeEntity> datetime { get; set; }
    }
    public class DatetimeEntity
    {
        public List<string> timex { get; set; }
        public string type { get; set; }
    }
    
}
