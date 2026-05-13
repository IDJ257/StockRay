using StockRay.Other;
using System.Collections.Immutable;
namespace StockRay.Services.PublicDashboard
{

    public record PublicDashboardDto(ImmutableList<SymbolDto> Symbols);


    public class PublicDashboardService
    {

        private readonly IFastAccess _fastAccess;

        public PublicDashboardService(IFastAccess fastAccess)
        {
            _fastAccess = fastAccess;
        }

        public ServiceResult<PublicDashboardDto> GivePublicDashboard()
        {

            var getPublicDashboardSymb = _fastAccess.GetSymbols().Where(s => s.IsTopNine).ToImmutableList();

            if (getPublicDashboardSymb == null)
            {
                return new ServiceResult<PublicDashboardDto>(false);
            }


            return new ServiceResult<PublicDashboardDto>(true, new PublicDashboardDto(getPublicDashboardSymb));


        }





    }











}
