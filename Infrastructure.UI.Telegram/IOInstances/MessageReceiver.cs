﻿using Application.Services;
using Infrastructure.Queuing;
using Infrastructure.TextUI.Core.Types;

namespace Infrastructure.TelegramBot.IOInstances
{
    internal class MessageReceiver
    {
        private QueueListener<Message> _listener;
        private readonly MessageHandler _messageHandler;

        public MessageReceiver(MessageHandler handler)
        {
            _messageHandler = handler;

            var service = new ConfigurationAppService();
            var hostName = service.Get("telegramQueueHost");
            var queueName = service.Get("telegramHandleMessageQueue");
            var username = service.Get("username");
            var password = service.Get("password");

            _listener = new(hostName, queueName, username, password);
        }

        public void StartReceiving()
        {
            _listener.AddMessageHandler(message =>
            {
                _messageHandler.ConsumeMessage(message);
            });
            _listener.StartConsuming();
        }
    }
}