﻿using Infrastructure.Kernel;
using System.Threading.Tasks;

namespace Infrastructure.TextUI.Core.Interfaces
{
    public interface IResultSender : IStartStop
    {
        Task SendMessage(ContentResult message);
    }
}