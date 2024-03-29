﻿using Application.Services;
using Autofac;
using Infrastructure.TextUI.Core.PipelineBaseKit;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.TelegramBot.MessagePipelines.Notes
{
    [Route("/get-notes", "📋 Notes")]
    [Description("Get's all your notes")]
    //TODO: think about reconstruction of notes. adding attachments,etc
    public class GetNotesPipeline : MessagePipelineBase
    {
        private readonly NoteAppService _noteService;

        public GetNotesPipeline(NoteAppService noteService, ILifetimeScope scope) : base(scope)
        {
            _noteService = noteService;
        }

        public override void RegisterPipelineStages()
        {
            RegisterStage(GetNotes);
            IsLooped = true;
        }

        private ContentResult GetNotes(MessageContext ctx)
        {
            var result = _noteService.GetByUserId(GetCurrentUser().Id);
            //todo: add markup
            var messagesToSend = new List<ContentResult>();

            messagesToSend.AddRange(result.Select(x => new ContentResult()
            {
                Text = x.Text,
                Buttons = new InlineKeyboardMarkup(
                        new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Delete","")
                            }
                        })
            }));

            return new()
            {
                Text = "Here are your notes:",
                MultiMessages = messagesToSend
            };
        }
    }
}