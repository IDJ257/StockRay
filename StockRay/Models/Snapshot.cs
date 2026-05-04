namespace StockRay.Models
{
    public class Snapshot
    {

        public int Id { get; init; }

        //RELATIONSHIP
        public Symbol Symbol { get; init; }

        public int SymbolId { get; init; }

        public DateTime SnapDate { get; init; }

        public float Open { get; init; }

        public float Close { get; init; }

        public float High { get; init; }

        public float Low { get; init; }

        public float Volume { get; init; }

        //SymbolID bi trqbvaloa avtomatichno da se uceli
        public Snapshot(DateTime snapDate,
            float open, float close, float high, float low, float volume)
        {
           
            SnapDate = snapDate;
            Open = open;
            Close = close;
            High = high;
            Low = low;
            Volume = volume;

        }

        public Snapshot()
        {

        }



    }

}
