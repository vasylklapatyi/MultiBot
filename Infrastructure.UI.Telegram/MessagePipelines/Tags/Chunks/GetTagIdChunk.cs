﻿using Application.Services;
using Autofac;
using Infrastructure.UI.Core.Interfaces;
using Infrastructure.UI.Core.MessagePipelines;
using Infrastructure.UI.Core.Types;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.UI.TelegramBot.MessagePipelines
{
    public class GetTagIdChunk : PipelineChunk
    {
        private readonly TagAppService _tagService;
        public GetTagIdChunk(TagAppService tagAppService, ILifetimeScope scope) : base(scope)
        {
            _tagService = tagAppService;
        }
        public override void RegisterPipelineStages()
        {
            RegisterStage(AskForSetName);
        }
        public ContentResult AskForSetName(MessageContext ctx)
        {
            var tags = _tagService.GetAll(GetCurrentUser(ctx).Id);

            var markups = new List<InlineKeyboardButton>();
            foreach (var item in tags)
            {
                markups.Add(InlineKeyboardButton.WithCallbackData(item.Name, item.Id.ToString()));
            }

            return new BotMessage()
            {
                Text = "Choose the set you want to open:",
                Buttons = new InlineKeyboardMarkup(markups.ToArray())
            };
        }
    }
}