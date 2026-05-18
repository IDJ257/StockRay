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
                //moje ServiceResutl

                return _fastAccess
                    .GetSymbols()
                    .Select(s => new AllSymbolsDto(s.Id, s.Name))
                    .ToList();
            }

        }

    











}
