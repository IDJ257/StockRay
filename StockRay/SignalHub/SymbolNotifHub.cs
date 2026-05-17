using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StockRay.Other;
using StockRay.Shared;

namespace StockRay.SignalHub
{

    public interface ISymbolNotifClient
    {

        Task ReceiveGroupUpdate(List<OutboundStockPrice> outBoundValues);

        Task ReceivePublicUpdate(List<OutboundStockPrice> outboundStockPrices);


    }

    //[Authorize]
    public class SymbolNotifHub : Hub<ISymbolNotifClient>
    {
        private readonly IActiveGroup   _activeGroup;


        public SymbolNotifHub(IActiveGroup activeGroup)
        {
            _activeGroup = activeGroup;
        }


        //SAMO ZA PUBLIC GROUPATA 
        //AUTHORIZE
        public async Task JoinGroup(string group)
        {
            await Groups.AddToGroupAsync($"{Context.ConnectionId}", group);

        }

        public async Task JoinGroups(params string[] groups)
        {

            var connId = Context.ConnectionId;

            _activeGroup.AddToGroup(connId, groups.ToList());

            //I tuk moje da se hangne zaradi countera i++
            for (int i = 0; i < groups.Length; i++)
            {
                await Groups.AddToGroupAsync(connId, groups[i]);

               
            }

            //Moje s TASKALL

        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _activeGroup.RemoveWhenLostConnection(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

    }
}
