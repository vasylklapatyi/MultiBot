﻿using Application.Services;
using Autofac;
using Common;
using Common.Entites;
using NLog;
using Persistence.Sql;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.TelegramBot.Jobs
{
    public class QustionSchedulingJobConfiguration : IConfiguredJob
    {
        public IJob Job { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; } = new();

        public IJobDetail BuildJob()
        {
            var builder = JobBuilder.Create<QustionSchedulingJob>()
                                     .WithIdentity("questionSenderSetup", "telegram-questions-setup");
            foreach (var item in AdditionalData)
            {
                builder.UsingJobData(item.Key, item.Value);
            }
            return builder.Build();
        }

        public ITrigger GetTrigger()
        {
            return TriggerBuilder.Create()
            .WithIdentity("question-setup-trigger", "telegram-questions-setup")
            .StartNow()
            .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForTotalCount(1)).Build();
        }
    }

    public class QustionSchedulingJob : IJob
    {
        private readonly ILifetimeScope _scope;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public QustionSchedulingJob(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Info("Scheduling questions...");
                var executor = _scope.Resolve<IJobExecutor>();
                var userRepo = _scope.Resolve<Repository<User>>();
                var questionsToLoad = _scope.Resolve<QuestionAppService>()
                    .GetScheduledQuestions();

                var users = userRepo.GetAll().ToList();

                int counter = 0;
                questionsToLoad.ForEach(async q =>
                {
                    counter++;
                    logger.Trace($"Question: {q.Id} loaded to scheduler");
                    var currentUser = users.FirstOrDefault(u => u.Id == q.UserId);
                    //TODO: навести порядок з юзер айді і чат айді
                    var dictionary = new Dictionary<string, string>
                {
                    { SendQustionJob.QuestionId, q.Id.ToString() },
                    { SendQustionJob.UserId, q.UserId.ToString() },
                    { SendQustionJob.ChatId, currentUser.TelegramUserId.ToString() },
                    { JobsConsts.cron, q.CronExpression }
                };
                    await executor.ScheduleJob(new QuestionJobConfiguration()
                    {
                        AdditionalData = dictionary
                    });
                });
                logger.Trace($"{counter} questions scheduled");
                logger.Trace("Scheduling questions completed");
            }
            catch (System.Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}