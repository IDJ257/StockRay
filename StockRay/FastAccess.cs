using System.Collections.Immutable;

namespace StockRay
{
  

    public interface IFastAccess
    {
        void Swap(ImmutableList<SymbolDto> newData);

        ImmutableList<SymbolDto> GetSymbols();
    }

    //Cache list for fast access without db roundtrips
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
