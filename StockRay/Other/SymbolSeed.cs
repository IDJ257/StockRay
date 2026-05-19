using System.Collections.Generic;
using System.Linq;
using StockRay.Models;

namespace StockRay.Other
{
    public static class SymbolSeed
    {
        private static readonly (string Name, float Open, float High, float Low, float CurrentPrice, bool IsTopNine)[] SeedData =
        [
            ("AAPL", 101f, 103.5f, 99.5f, 102.2f, true),
            ("MSFT", 102f, 104.5f, 100.5f, 103.2f, true),
            ("TSM", 103f, 105.5f, 101.5f, 104.2f, true),
            ("NVDA", 104f, 106.5f, 102.5f, 105.2f, true),
            ("CRM", 105f, 107.5f, 103.5f, 106.2f, true),
            ("ADBE", 106f, 108.5f, 104.5f, 107.2f, true),
            ("INTC", 107f, 109.5f, 105.5f, 108.2f, true),
            ("ASML", 108f, 110.5f, 106.5f, 109.2f, true),
            ("CSCO", 109f, 111.5f, 107.5f, 110.2f, true),
            ("ORCL", 110f, 112.5f, 108.5f, 111.2f, false),
            ("QCOM", 111f, 113.5f, 109.5f, 112.2f, false),
            ("AVGO", 112f, 114.5f, 110.5f, 113.2f, false),
            ("ACN", 113f, 115.5f, 111.5f, 114.2f, false),
            ("TXN", 114f, 116.5f, 112.5f, 115.2f, false),
            ("SAP", 115f, 117.5f, 113.5f, 116.2f, false),
            ("SHOP", 116f, 118.5f, 114.5f, 117.2f, false),
            ("SNE", 117f, 119.5f, 115.5f, 118.2f, false),
            ("IBM", 118f, 120.5f, 116.5f, 119.2f, false),
            ("AMD", 119f, 121.5f, 117.5f, 120.2f, false),
            ("NOW", 120f, 122.5f, 118.5f, 121.2f, false),
            ("SQ", 121f, 123.5f, 119.5f, 122.2f, false),
            ("FIS", 122f, 124.5f, 120.5f, 123.2f, false),
            ("INTU", 123f, 125.5f, 121.5f, 124.2f, false),
            ("UBER", 124f, 126.5f, 122.5f, 125.2f, false),
            ("SNOW", 125f, 127.5f, 123.5f, 126.2f, false),
            ("FISV", 126f, 128.5f, 124.5f, 127.2f, false),
            ("AMAT", 127f, 129.5f, 125.5f, 128.2f, false),
            ("MU", 128f, 130.5f, 126.5f, 129.2f, false),
            ("INFY", 129f, 131.5f, 127.5f, 130.2f, false),
            ("LRCX", 130f, 132.5f, 128.5f, 131.2f, false),
            ("VMW", 131f, 133.5f, 129.5f, 132.2f, false),
            ("ADSK", 132f, 134.5f, 130.5f, 133.2f, false),
            ("TEAM", 133f, 135.5f, 131.5f, 134.2f, false),
            ("DELL", 134f, 136.5f, 132.5f, 135.2f, false),
            ("ADI", 135f, 137.5f, 133.5f, 136.2f, false),
            ("WDAY", 136f, 138.5f, 134.5f, 137.2f, false),
            ("NXPI", 137f, 139.5f, 135.5f, 138.2f, false),
            ("CTSH", 138f, 140.5f, 136.5f, 139.2f, false),
            ("ERIC", 139f, 141.5f, 137.5f, 140.2f, false),
            ("DOCU", 140f, 142.5f, 138.5f, 141.2f, false),
            ("PLTR", 141f, 143.5f, 139.5f, 142.2f, false),
            ("KLAC", 142f, 144.5f, 140.5f, 143.2f, false),
            ("APH", 143f, 145.5f, 141.5f, 144.2f, false),
            ("TEL", 144f, 146.5f, 142.5f, 145.2f, false),
            ("U", 145f, 147.5f, 143.5f, 146.2f, false),
            ("MCHP", 146f, 148.5f, 144.5f, 147.2f, false),
            ("STM", 147f, 149.5f, 145.5f, 148.2f, false),
            ("SNPS", 148f, 150.5f, 146.5f, 149.2f, false),
            ("XLNX", 149f, 151.5f, 147.5f, 150.2f, false),
            ("CRWD", 150f, 152.5f, 148.5f, 151.2f, false),
            ("SPLK", 151f, 153.5f, 149.5f, 152.2f, false),
            ("CDNS", 152f, 154.5f, 150.5f, 153.2f, false),
            ("MRVL", 153f, 155.5f, 151.5f, 154.2f, false),
            ("OKTA", 154f, 156.5f, 152.5f, 155.2f, false),
            ("HPQ", 155f, 157.5f, 153.5f, 156.2f, false),
            ("MSI", 156f, 158.5f, 154.5f, 157.2f, false),
            ("PANW", 157f, 159.5f, 155.5f, 158.2f, false),
            ("GLW", 158f, 160.5f, 156.5f, 159.2f, false),
            ("DDOG", 159f, 161.5f, 157.5f, 160.2f, false),
            ("FTV-PA", 160f, 162.5f, 158.5f, 161.2f, false),
            ("ANSS", 161f, 163.5f, 159.5f, 162.2f, false),
            ("WIT", 162f, 164.5f, 160.5f, 163.2f, false),
            ("RNG", 163f, 165.5f, 161.5f, 164.2f, false),
            ("FTV", 164f, 166.5f, 162.5f, 165.2f, false),
            ("PAYC", 165f, 167.5f, 163.5f, 166.2f, false),
            ("SWKS", 166f, 168.5f, 164.5f, 167.2f, false),
            ("COUP", 167f, 169.5f, 165.5f, 168.2f, false),
            ("VRSN", 168f, 170.5f, 166.5f, 169.2f, false),
            ("STNE", 169f, 171.5f, 167.5f, 170.2f, false),
            ("GRMN", 170f, 172.5f, 168.5f, 171.2f, false),
            ("MXIM", 171f, 173.5f, 169.5f, 172.2f, false),
            ("KEYS", 172f, 174.5f, 170.5f, 173.2f, false),
            ("FLT", 173f, 175.5f, 171.5f, 174.2f, false),
            ("NET", 174f, 176.5f, 172.5f, 175.2f, false),
            ("ANET", 175f, 177.5f, 173.5f, 176.2f, false),
            ("CAJ", 176f, 178.5f, 174.5f, 177.2f, false),
            ("ZBRA", 177f, 179.5f, 175.5f, 178.2f, false),
            ("ZS", 178f, 180.5f, 176.5f, 179.2f, false),
            ("FTNT", 179f, 181.5f, 177.5f, 180.2f, false),
            ("EPAM", 180f, 182.5f, 178.5f, 181.2f, false),
            ("CDW", 181f, 183.5f, 179.5f, 182.2f, false),
            ("GIB", 182f, 184.5f, 180.5f, 183.2f, false),
            ("TER", 183f, 185.5f, 181.5f, 184.2f, false),
            ("SSNC", 184f, 186.5f, 182.5f, 185.2f, false),
            ("ZI", 185f, 187.5f, 183.5f, 186.2f, false),
            ("UMC", 186f, 188.5f, 184.5f, 187.2f, false),
            ("WORK", 187f, 189.5f, 185.5f, 188.2f, false),
            ("BR", 188f, 190.5f, 186.5f, 189.2f, false),
            ("HUBS", 189f, 191.5f, 187.5f, 190.2f, false),
            ("QRVO", 190f, 192.5f, 188.5f, 191.2f, false),
            ("CHKP", 191f, 193.5f, 189.5f, 192.2f, false),
            ("AKAM", 192f, 194.5f, 190.5f, 193.2f, false),
            ("TYL", 193f, 195.5f, 191.5f, 194.2f, false),
            ("CCC", 194f, 196.5f, 192.5f, 195.2f, false),
            ("UI", 195f, 197.5f, 193.5f, 196.2f, false),
            ("ZEN", 196f, 198.5f, 194.5f, 197.2f, false),
            ("CTXS", 197f, 199.5f, 195.5f, 198.2f, false),
            ("TRMB", 198f, 200.5f, 196.5f, 199.2f, false),
            ("AVLR", 199f, 201.5f, 197.5f, 200.2f, false),
            ("STX", 200f, 202.5f, 198.5f, 201.2f, false)
        ];

        public static IEnumerable<Symbol> Seed()
        {
            return SeedData.Select(data =>
            {
                var symbol = new Symbol(data.Name)
                    .SetOpen(data.Open)
                    .SetHigh(data.High)
                    .SetLow(data.Low)
                    .SetCurrentPrice(data.CurrentPrice);

                if (data.IsTopNine)
                {
                    symbol.SetTopNine();
                }

                return symbol;
            }).ToList();
        }
    }
}
