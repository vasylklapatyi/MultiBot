﻿using Autofac;
using Domain;
using Infrastructure.UI.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.UI.TelegramBot.IOInstances
{
    //todo: segregate into new file
    public class MessageUpdateHandler : IUpdateHandler
	{
		private static IMessageReceiver _messageReceiver;
		private static IQueryReceiver _queryReceiver;

		static MessageUpdateHandler()
		{
			var scope = DependencyAccessor.LifetimeScope.BeginLifetimeScope();
			_messageReceiver = scope.Resolve<IMessageReceiver>();
			_queryReceiver = scope.Resolve<IQueryReceiver>();
		}
		public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			switch (update.Type)
			{
				case UpdateType.Unknown:
					break;
				case UpdateType.Message:
                    {
						var message = new Core.Types.Message()
						{
							ChatId = update.Message.Chat.Id,
							Text = update.Message.Text
						};
						_messageReceiver.ConsumeMessage(message);
                    }
					break;
				case UpdateType.InlineQuery:
					{
						var message = new Core.Types.Message()
						{
							Text = update.CallbackQuery.Data,
							ChatId = update.CallbackQuery.Message.Chat.Id
						};
						_queryReceiver.ConsumeQuery(message); break;
					}
					case UpdateType.ChosenInlineResult:
					break;
				case UpdateType.CallbackQuery:
					{
						var message = new Infrastructure.UI.Core.Types.Message()
						{
							Text = update.CallbackQuery.Data,
							ChatId = update.CallbackQuery.Message.Chat.Id
						};
						_queryReceiver.ConsumeQuery(message);
						//_messageReceiver.ConsumeMessage(update.CallbackQuery);
					}
					break;
				case UpdateType.EditedMessage:
					break;
				case UpdateType.ChannelPost:
					break;
				case UpdateType.EditedChannelPost:
					break;
				case UpdateType.ShippingQuery:
					break;
				case UpdateType.PreCheckoutQuery:
					break;
				case UpdateType.Poll:
					break;
				case UpdateType.PollAnswer:
					break;
				case UpdateType.MyChatMember:
					break;
				case UpdateType.ChatMember:
					break;
				default:
					break;
			}
			return Task.FromResult(new object());
		}


	}
}
