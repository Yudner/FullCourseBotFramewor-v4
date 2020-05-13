using Microsoft.Bot.Builder.AI.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Bot.Infrastructure.Luis
{
    public interface ILuisService
    {
        LuisRecognizer _recognizer { get; }
    }
}
