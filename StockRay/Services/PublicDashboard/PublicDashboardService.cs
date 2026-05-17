using StockRay.Other;
using System.Collections.Immutable;
namespace StockRay.Services.PublicDashboard
{

    public record OutBoundPublicDashBoardStock(int Id, string Name, float Open, float High, float Low, float CurrentPrice);

    public record PublicDashboardDto(ImmutableList<OutBoundPublicDashBoardStock> Symbols);


    public class PublicDashboardService
    {

        private readonly IFastAccess _fastAccess;

        public PublicDashboardService(IFastAccess fastAccess)
        {
            _fastAccess = fastAccess;
        }

        public ServiceResult<PublicDashboardDto> GivePublicDashboard()
        {

            //moje tuk da se formatira s Math.Round ama za sega sh go ostavim na front-enda
            var getPublicDashboardSymb = _fastAccess
                .GetSymbols()
                .Where(s => s.IsTopNine)
                .Select(s => new OutBoundPublicDashBoardStock(s.Id, s.Name, s.Open, s.High, s.Low, s.CurrentPrice))
                .ToImmutableList();

            if (getPublicDashboardSymb == null)
            {
                return new ServiceResult<PublicDashboardDto>(false);
            }


            return new ServiceResult<PublicDashboardDto>(true, new PublicDashboardDto(getPublicDashboardSymb));


        }





    }











}
