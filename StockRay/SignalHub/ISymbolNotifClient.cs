using StockRay.Shared;

namespace StockRay.SignalHub
{
    public interface ISymbolNotifClient
    {

        Task ReceiveGroupUpdate(OutboundStockPrice outBoundValues);

        Task ReceivePublicUpdate(List<OutboundStockPrice> outboundStockPrices);


    }
}
