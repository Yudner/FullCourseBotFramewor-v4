using Clinic.Bot.Common.Models.BotState;
using Clinic.Bot.Common.Models.EntityModel;
using Clinic.Bot.Common.Models.MedicalAppointment;
using Clinic.Bot.Common.Models.User;
using Clinic.Bot.Data;
using Clinic.Bot.Infrastructure.Luis;
using Clinic.Bot.Infrastructure.SendGridEmail;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Clinic.Bot.Dialogs.CreateAppointment
{
    public class CreateAppointmentDialog : ComponentDialog
    {
        static UserModel newUserModel = new UserModel();
        static MedicalAppointmentModel medicalAppointment = new MedicalAppointmentModel();
        private readonly IDataBaseService _dataBaseService;
        private readonly ISendGridEmailService _sendGridEmailService;
        static string userText;
        private readonly ILuisService _luisService;

        private readonly IStatePropertyAccessor<BotStateModel> _userState;

        public CreateAppointmentDialog(IDataBaseService dataBaseService, UserState userState, ISendGridEmailService sendGridEmailService, ILuisService luisService)
        {
            _luisService = luisService;
            _sendGridEmailService = sendGridEmailService;
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));
            _dataBaseService = dataBaseService;
            var waterfallSteps = new WaterfallStep[]
            {
                SetPhone,
                SetFullName,
                SetEmail,
                SetDate,
                SetTime,
                Confirmation,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }


        private async Task<DialogTurnResult> SetPhone(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userText = stepContext.Context.Activity.Text;
            var userStateData = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

            if (userStateData.medicalData)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                string text = $"Por favor ingresa tu número de teléfono en el siguiente formato:" +
                 $"{Environment.NewLine}(Código país) (Número)" +
                 $"{Environment.NewLine}Ejemplo: +51 943662964";

                return await stepContext.PromptAsync(
                  nameof(TextPrompt),
                  new PromptOptions { Prompt = MessageFactory.Text(text) },
                  cancellationToken
                );
            }
        }

        private async Task<DialogTurnResult> SetFullName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateData = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

            if (userStateData.medicalData)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var userPhone = stepContext.Context.Activity.Text;
                newUserModel.phone = userPhone;

                return await stepContext.PromptAsync(
                  nameof(TextPrompt),
                  new PromptOptions { Prompt = MessageFactory.Text("Ahora ingresa tu nombre completo:") },
                  cancellationToken
                );
            }            
        }

        private async Task<DialogTurnResult> SetEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateData = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

            if (userStateData.medicalData)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var userFullName = stepContext.Context.Activity.Text;
                newUserModel.fullName = userFullName;

                return await stepContext.PromptAsync(
                  nameof(TextPrompt),
                  new PromptOptions { Prompt = MessageFactory.Text("Ahora ingresa tu correo:") },
                  cancellationToken
                );
            }
        }

        private async Task<DialogTurnResult> SetDate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userEmail = stepContext.Context.Activity.Text;
            newUserModel.email = userEmail;

            var newStepContext = stepContext;
            newStepContext.Context.Activity.Text = userText;

            var luisResult = await _luisService._recognizer.RecognizeAsync(newStepContext.Context, cancellationToken);
            var Entities = luisResult.Entities.ToObject<EntityLuis>();

            if (Entities.datetime != null)
            {
                var date = Entities.datetime.First().timex.First().Replace("XXXX", DateTime.Now.Year.ToString());
                medicalAppointment.date = DateTime.Parse(date);
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                string text = $"Ahora necesito la fecha de la cita médica con el siguiente formato:" +
                    $"{Environment.NewLine}dd/mm/yyyy";

                return await stepContext.PromptAsync(
                  nameof(TextPrompt),
                  new PromptOptions { Prompt = MessageFactory.Text(text) },
                  cancellationToken
                );
            }            
        }

        private async Task<DialogTurnResult> SetTime(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(medicalAppointment.date == DateTime.MinValue)
            {
                var medicalDate = stepContext.Context.Activity.Text;
                medicalAppointment.date = Convert.ToDateTime(medicalDate);
            }

            return await stepContext.PromptAsync(
              nameof(TextPrompt),
              new PromptOptions { Prompt = CreateButtonsTime() },
              cancellationToken
            );
        }

        private async Task<DialogTurnResult> Confirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var medicalTime = stepContext.Context.Activity.Text;
            medicalAppointment.time = int.Parse(medicalTime);

            return await stepContext.PromptAsync(
              nameof(TextPrompt),
              new PromptOptions { Prompt = CreateButtonsConfirmation() },
              cancellationToken
            );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var userConfirmation = stepContext.Context.Activity.Text;

                if (userConfirmation.ToLower().Equals("si"))
                {
                    // SAVE DATABASE
                    string userId = stepContext.Context.Activity.From.Id;
                    var userModel = _dataBaseService.User.FirstOrDefault(x => x.id == userId);

                    var userStateData = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

                    if (!userStateData.medicalData)
                    {
                        //UPDATE USER
                        userModel.phone = newUserModel.phone;
                        userModel.fullName = newUserModel.fullName;
                        userModel.email = newUserModel.email;

                        _dataBaseService.User.Update(userModel);
                        await _dataBaseService.SaveAsync();
                    }

                    userStateData.medicalData = true;

                    //SAVE MEDICAL APPOINTMENT
                    medicalAppointment.id = Guid.NewGuid().ToString();
                    medicalAppointment.idUser = userId;
                    await _dataBaseService.MedicalAppointment.AddAsync(medicalAppointment);
                    await _dataBaseService.SaveAsync();

                    await stepContext.Context.SendActivityAsync("Tu cita se guardó con éxito.", cancellationToken: cancellationToken);
                    string summaryMedical = $"Para: {userModel.fullName}" +
                        $"{Environment.NewLine}📞 Teléfono: {userModel.phone}" +
                        $"{Environment.NewLine}📧 Email: {userModel.email}" +
                        $"{Environment.NewLine}📅 Fecha: {medicalAppointment.date.ToShortDateString()}" +
                        $"{Environment.NewLine}⏰ Hora: {medicalAppointment.time}";

                    await stepContext.Context.SendActivityAsync(summaryMedical, cancellationToken: cancellationToken);
                    //ENVIAR CORREO
                    await SendEmail(userModel, medicalAppointment);
                    await Task.Delay(1000);
                    await stepContext.Context.SendActivityAsync("¿En qué más puedo ayudarte?", cancellationToken: cancellationToken);
                    medicalAppointment = new MedicalAppointmentModel();

                }
                else
                {
                    await stepContext.Context.SendActivityAsync("No hay problema, la próxima será.", cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                await stepContext.Context.SendActivityAsync(e.InnerException.ToString(), cancellationToken: cancellationToken);
            }

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task SendEmail(UserModel userModel, MedicalAppointmentModel medicalAppointment)
        {
            string contentEmail = $"Hola {userModel.fullName}, <br/><br>Se creó una cita con la siguiente información:" +
                $"<br>Fecha: {medicalAppointment.date.ToShortDateString()}" +
                $"<br>Hora: {medicalAppointment.time.ToString()}<br/><br>Saludos.";

            await _sendGridEmailService.Execute(
              "yudner.paredes@hotmail.com",
              "Yudner",
              userModel.email,
              userModel.fullName,
              "Confirmación de cita",
              "",
              contentEmail
            );
        }

        private Activity CreateButtonsTime()
        {
            var reply = MessageFactory.Text("Ahora selecciona la hora:");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title = "9", Value = "9", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "10", Value = "10", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "11", Value = "11", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "15", Value = "15", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "16", Value = "16", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "17", Value = "17", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "18", Value = "18", Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }
        private Activity CreateButtonsConfirmation()
        {
            var reply = MessageFactory.Text("¿Confirmas la creación de esta cita médica?");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title = "Si", Value = "Si", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "No", Value = "No", Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }
    }
}
