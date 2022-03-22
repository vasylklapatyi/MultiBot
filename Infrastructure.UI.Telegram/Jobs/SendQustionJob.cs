﻿using Infrastructure.Jobs.Core;
using Infrastructure.UI.Core.Interfaces;
using Infrastructure.UI.Core.Types;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.TelegramBot.Jobs
{
    public class QuestionJobConfiguration : IConfiguredJob
    {
        public IJob Job { get; set; }
        public Dictionary<string,string> AdditionalData { get; set; }

        public IJobDetail BuildJob()
        {
            var builder = JobBuilder.Create<SendQustionJob>()
                                     .WithIdentity("questionSender" + AdditionalData[SendQustionJob.QuestionId], "telegram-questions" );
            foreach (var item in AdditionalData)
            {
                builder.UsingJobData(item.Key, item.Value);
            }           
            return builder.Build();
        }

        public ITrigger GetTrigger()
        {
            return TriggerBuilder.Create()
            .WithIdentity("question-trigger" + AdditionalData[SendQustionJob.QuestionId], "telegram-questions" )
            .StartNow()
            //.WithCronSchedule(AdditionalData[JobsConsts.CRON])
            .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(2)).Build();
        }
    }

    public class SendQustionJob : IJob
    {
        public const string QuestionId = "questionId";
        public const string UserId = "userId";
        public const string ChatId = "chatId";
        private readonly IResultSender _sender;
        public SendQustionJob(IResultSender sender)
        {
            _sender = sender;
        }

        public virtual Task Execute(IJobExecutionContext context)
        {
            //TODO: Add better formatting for questions

            var timestamp = DateTime.Now;
            Console.WriteLine($"{context.JobDetail.JobDataMap[QuestionId]} - {timestamp:yyyy-MM-dd HH:mm:ss.fff}");

            _sender.SendMessage(
                new BotMessage()
                {
                    Text = context.JobDetail.JobDataMap[QuestionId] as string
                }
                , Convert.ToInt64(context.JobDetail.JobDataMap[ChatId]));
            return Task.CompletedTask;
        }
    }

}