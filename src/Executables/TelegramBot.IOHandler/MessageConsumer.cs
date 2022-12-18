﻿using Application.Services;
using Common.Configuration;
using Infrastructure.Queuing;
using Infrastructure.TextUI.Core.PipelineBaseKit;

namespace LifeTracker.TelegramBot.IOHandler
{
    internal class MessageConsumer
    {
        private static QueuePublisher _publisher;

        public MessageConsumer()
        {
            var configuration = ConfigurationHelper.GetConfiguration();
            var queueName = configuration["Telegram:HandleMessageQueue"];

            _publisher = QueuingHelper.CreatePublisher(queueName);
        }

        public void ConsumeMessage(TelegramMessage msg)
        {
            _publisher.Publish(msg);
        }
    }
}