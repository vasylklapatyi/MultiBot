﻿using Application.Services;
using Autofac;
using Infrastructure.TextUI.Core.PipelineBaseKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.TelegramBot.MessagePipelines.Tags
{
    [Route("/get_tag", "📁 Get Tag Data")]
    public class GetTagDataPipeline : MessagePipelineBase
    {
        private readonly TagAppService _tagService;

        public GetTagDataPipeline(TagAppService tagService, ILifetimeScope scope) : base(scope)
        {
            _tagService = tagService;
        }

        public override void RegisterPipelineStages()
        {
            RegisterStage(AskForSetName);
            RegisterStage(ReturnNotes);
        }

        public ContentResult AskForSetName(MessageContext ctx)
        {
            var tags = _tagService.GetAll(GetCurrentUser().Id);

            var markups = new List<InlineKeyboardButton>();
            foreach (var item in tags)
            {
                markups.Add(InlineKeyboardButton.WithCallbackData(item.Name, item.Id.ToString()));
            }

            return new ContentResult()
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
                b.AppendLine(++counter + " " + item.Text);
            }

            return Text($"{tag.Name}: " + b.ToString());
        }
    }
}