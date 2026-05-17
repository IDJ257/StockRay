using System.Collections.Immutable;

namespace StockRay
{
    //SINGLETON

    public interface IFastAccess
    {
        void Swap(ImmutableList<SymbolDto> newData);

        ImmutableList<SymbolDto> GetSymbols();
    }

    //RAM LIST 
    //VSICHKI ENDPOINTI NATISKAT TUK A NE SE ZANIMAVAT S DATABASE
    //INTERNAL DTO IMmutable-lista raboti s tazi informaciq navsqkude 
    public record SymbolDto(int Id, string Name, float Open, float High, float Low, float CurrentPrice, bool IsTopNine);
    public class FastAccess : IFastAccess
    {

        private ImmutableList<SymbolDto> _immutableSymbols;


        public FastAccess()
        {
            _immutableSymbols = ImmutableList.Create<SymbolDto>();
        }


        public void Swap(ImmutableList<SymbolDto> newData)
        {
            _immutableSymbols = newData;
        }

        public ImmutableList<SymbolDto> GetSymbols()
        {
            return _immutableSymbols;
        }

    }
}
