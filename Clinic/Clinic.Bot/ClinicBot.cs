// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);

        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await SaveUser(turnContext);
            await _dialog.RunAsync(
              turnContext,
              _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
              cancellationToken
            );
        }
    }
}
