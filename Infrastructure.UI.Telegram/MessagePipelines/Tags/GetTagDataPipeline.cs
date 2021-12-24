﻿using Application.Services;
using Infrastructure.UI.Core.Attributes;
using Infrastructure.UI.Core.Interfaces;
using Infrastructure.UI.Core.MessagePipelines;
using Infrastructure.UI.Core.Types;
using Infrastructure.UI.TelegramBot.ResponseTypes;
using Persistence.Sql.Entites;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.UI.TelegramBot.MessagePipelines.Tags
{
    [Route("/get-tag-data")]
    [Description("Use this command for getting tag data")]
    public class GetTagDataPipeline : MessagePipelineBase
    {
        private readonly TagAppService _tagService;
        public GetTagDataPipeline(TagAppService tagService)
        {
            _tagService = tagService;
        }

        public override void RegisterPipelineStages()
        {
            Stages.Add(AskForSetName);
            Stages.Add(ReturnNotes);
        }

        public ContentResult AskForSetName(MessageContext ctx)
        {
            var tags = _tagService.GetAll(GetCurrentUser().Id);
            
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

        public ContentResult ReturnNotes(MessageContext ctx)
        {
            var id = Guid.Parse(ctx.Message.Text);
            var tag = _tagService.Get(id);
            int counter = 0;
            StringBuilder b = new(Environment.NewLine);
            foreach (var item in tag.Notes)
            {
                b.AppendLine(++counter + " " + item.Name);
            }

            return Text( $"{tag.Name}: "+ b.ToString());
        }
    }
}