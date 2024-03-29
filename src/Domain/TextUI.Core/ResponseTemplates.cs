﻿using Infrastructure.TextUI.Core.PipelineBaseKit;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.TextUI.Core
{
    public static class ResponseTemplates
    {
        public static ContentResult Text(string text, bool invokeNextImmediately = false)
        {
            //todo: implement delayd messages and fire-and-forget messages
            return new()
            {
                Text = text,
                InvokeNextImmediately = invokeNextImmediately
            };
        }

        public static ContentResult YesNo(string question)
        {
            var markups = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("Yes", true.ToString()),
                InlineKeyboardButton.WithCallbackData("No", false.ToString())
            };

            return new ContentResult()
            {
                Text = question,
                Buttons = new InlineKeyboardMarkup(markups.ToArray())
            };
        }
    }
}