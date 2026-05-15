using Microsoft.AspNetCore.SignalR;

namespace StockRay.SignalHub
{
    public class SymbolNotifHub : Hub<ISymbolNotifClient>
    {


        //AUTHORIZE
        public async Task JoinGroup(string group)
        {
            await Groups.AddToGroupAsync($"{Context.ConnectionId}", group);

        }

    }
}
