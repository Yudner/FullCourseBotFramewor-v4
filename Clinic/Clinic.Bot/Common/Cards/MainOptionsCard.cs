using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Clinic.Bot.Common.Cards
{
    public class MainOptionsCard
    {
        public static async Task ToShow(DialogContext dc, CancellationToken cancellationToken)
        {
            await dc.Context.SendActivityAsync(activity: CreateCarousel(), cancellationToken);
        }
        private static Activity CreateCarousel()
        {
            var cardCitasMedicas = new HeroCard
            {
                Title = "Citas Médicas",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/menu_01.jpg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Crear cita médica", Value = "Crear cita médica", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Ver mis citas", Value = "Ver mis citas", Type = ActionTypes.ImBack}
                }
            };
            var cardInformacionContacto = new HeroCard
            {
                Title = "Información de contacto",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/menu_02.jpg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Centro de contacto", Value = "Centro de contacto", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Sitio web", Value = "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-add-media-attachments?view=azure-bot-service-4.0&tabs=csharp", Type = ActionTypes.OpenUrl},
                }
            };
            var cardSiguenosRedes = new HeroCard
            {
                Title = "Síguenos en las redes",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/menu_03.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Facebook", Value = "https://www.facebook.com/YudnerParedes", Type = ActionTypes.OpenUrl},
                    new CardAction(){Title = "Instagram", Value = "https://www.instagram.com/yudner_paredes/", Type = ActionTypes.OpenUrl},
                    new CardAction(){Title = "Twitter", Value = "https://twitter.com/YudnerParedes", Type = ActionTypes.OpenUrl},
                }
            };

            var cardCalificación = new HeroCard
            {
                Title = "Calificación",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/menu_04.jpg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Calificar Bot", Value = "Calificar Bot", Type = ActionTypes.ImBack}
                }
            };

            var optionsAttachments = new List<Attachment>()
            {
                cardCitasMedicas.ToAttachment(),
                cardInformacionContacto.ToAttachment(),
                cardSiguenosRedes.ToAttachment(),
                cardCalificación.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionsAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }
    }
}
