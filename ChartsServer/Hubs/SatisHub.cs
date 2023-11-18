using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChartsServer.Hubs
{
    public class SatisHub: Hub
    {
        public async Task SendMessageAsync()
        {
            await Clients.All.SendAsync("receiveMessage", "Merhaba");
        }
    }
}
