// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Clinic.Bot.Common.Models.Conversation;
using Clinic.Bot.Common.Models.User;
using Clinic.Bot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Clinic.Bot
{
    public class ClinicBot<T> : ActivityHandler where T: Dialog
    {
        BotState _conversationState;
        BotState _userState;
        Dialog _dialog;
        IDataBaseService _dataBaseService;

        public ClinicBot(ConversationState conversationState, UserState userState, T dialog, IDataBaseService dataBaseService)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _dataBaseService = dataBaseService;
        }

        // PARA MOSTRAR MENSAJE DE BIENVENIDA EN EL CANAL WEB CHAT
        //protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    if(string.IsNullOrEmpty(turnContext.Activity.From.Name))
        //        await turnContext.SendActivityAsync(MessageFactory.Text($"Bienvenido"), cancellationToken);
            
        //    await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        //}
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bienvenido"), cancellationToken);
                }
            }
        }

        private async Task SaveUser(ITurnContext<IMessageActivity> turnContext)
        {
            var userModel = new UserModel();
            userModel.id = turnContext.Activity.From.Id;
            userModel.userNameChannel = turnContext.Activity.From.Name;
            userModel.channel = turnContext.Activity.ChannelId;
            userModel.registerDate = DateTime.Now.Date;

            var user = _dataBaseService.User.FirstOrDefault(x => x.id == turnContext.Activity.From.Id);
            if (user == null)
            {
                await _dataBaseService.User.AddAsync(userModel);
                await _dataBaseService.SaveAsync();
            }
        }

        //private async Task SaveUserConversation(ITurnContext<IMessageActivity> turnContext)
        //{
        //    // AQUÍ SE CONSTRUYE EL MODELO
        //    var conversationModel = new ConversationModel();
        //    conversationModel.id = Guid.NewGuid().ToString();
        //    conversationModel.idUser = turnContext.Activity.From.Id;
        //    conversationModel.registerDate = DateTime.Now.Date;
        //    conversationModel.message = turnContext.Activity.Text;
            
        //    await _dataBaseService.Conversation.AddAsync(conversationModel); // AQUÍ SE GUARDA EN LA BD
        //    await _dataBaseService.SaveAsync(); // AQUÍ SE CONFIRMA LA OPERACIÓN
        //}

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);

        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await SaveUser(turnContext);
            //await SaveUserConversation(turnContext);
            await _dialog.RunAsync(
              turnContext,
              _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
              cancellationToken
            );
        }
    }
}
