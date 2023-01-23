﻿using Application.Chatting.Core.Repsonses;
using Application.Chatting.Core.Routing;
using Application.Chatting.Core.StageMap;
using Application.Services;
using Application.TelegramBot.Commands.Core.Context;
using Application.TelegramBot.Commands.Core.Interfaces;
using Application.TelegramBot.Pipelines.V2.Pipelines.ToDo.ToDoItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Chatting.Core.Repsonses.Menu;

namespace Application.TelegramBot.Commands.Pipelines.ToDo.ToDoItems
{
    [Route("/get_todo", "📋 My ToDo List")]
    public class GetToDoListCommand : ITelegramCommand
    {
        public const string TODOSORDER_CACHEKEY = "ToDosOrder";
        private readonly ToDoAppService _todoService;
        private readonly RoutingTable _routingTable;
        public GetToDoListCommand(ToDoAppService todoService, RoutingTable routingTable)
        {
            _todoService = todoService;
            _routingTable = routingTable;
        }
        public void DefineStages(StageMapBuilder builder)
        {
        }

        public Task<StageResult> Execute(TelegramMessageContext ctx)
        {
            var userId = ctx.User.Id;
            var categories = _todoService.GetAllCategories(userId);
            var sb = new StringBuilder();
            int counter = 0;
            var orders = new Dictionary<int, Guid>();
            if (categories.Any())
            {
                sb.AppendLine("Yor TODOs:");
                categories.ToList()
                .ForEach(c =>
                {
                    //TODO: Додати щоб одна категорія була синя,друга - зелена
                    sb.AppendLine($"🔶{c.Name}");
                    c.ToDoItems.ForEach(ti =>
                    {
                        sb.AppendLine($"   🔸 {++counter}.{ti.Text}");
                        orders.Add(counter, ti.Id);
                    });
                });
            }
            else
            {
                sb.Append("No ToDos yet");
            }
            ctx.Cache.Set(TODOSORDER_CACHEKEY, orders);
            return ContentResponse.New(new()
            {
                Text = sb.ToString(),
                Menu = new(MenuType.MenuKeyboard, new[]
                {
                    new[]
                    {
                            new Button(_routingTable.AlternativeRoute<CreateToDoCommand>()),
                            //new Button(_routingTable.AlternativeRoute<MakeToDoAsReminderCommand>()),
                    },
                    new[]
                    {
                         new Button(_routingTable.AlternativeRoute<RemoveToDoCommand>())
                    }
                })
            });
        }
    }
}
/*
    public class GetToDoListPipeline : MessagePipelineBase
    {
        public GetToDoListPipeline(ILifetimeScope scope) : base(scope)
        {
            var todoService = scope.Resolve<ToDoAppService>();
            RegisterStage(() =>
            {
              
            });
        }

    }

 */