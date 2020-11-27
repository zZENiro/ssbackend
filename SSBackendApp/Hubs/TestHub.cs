using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSBackendApp.Hubs
{
    public interface IContract
    {
        Task SendMessage(string message);
    }

    public class TestHub : Hub<IContract>
    {
        public Task GetMessage(string message)
        {
            return Clients.Caller.SendMessage(message + " <-- received");
        }
    }
}
