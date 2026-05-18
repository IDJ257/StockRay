namespace StockRay.Services.GetAllSymbols
{
    public class GetAllSymbolsService
    {
        private readonly IFastAccess _fastAccess;

        public GetAllSymbolsService(IFastAccess fastAccess)
        {
            _fastAccess = fastAccess;
        }

        //moje i da e rechnik za po-burzo deistvie
        public List<AllSymbolsDto> GetAllSymbols()
        {

            if(_fastAccess.GetSymbols() is null || _fastAccess.GetSymbols().Count == 0)
            {
                throw new ApplicationException("RAM List was either null or empty.");
            }

            return _fastAccess
                .GetSymbols()
                .Select(s => new AllSymbolsDto(s.Id, s.Name))
                .ToList();
        }

    }













}
