namespace StockRay.Models
{
    /*
    public class UsersSymbols
    {

        public int Id { get; init; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int SymbolId { get; set; }

        public Symbol Symbol { get; set; }


        //Compositen Index UserID/SymbolId



    }*/



    //Moje i da e record
    public class Subscription
    {

        //Tova DB shte go dopusne ZA SEGA KATO NQMAME COMPOSITE INDEX
        //NO HASHSETA NI PAZI IN MEMORY PREDI DB CALL-A

        //id:1 UserID:1 SymbolID:1 StartDate 10/5/2026 endDate 17/5/2026
        //id:2 UserID:1 SymbolID:1 StartDate 10/5/2026 endDate 17/5/2026

        //MOJE DA VURNEM NAV PROPERTITATA.

        //v user tova e hashset<Sub> zashtoto iskame User da ne moje da dobavi dva subscriptiona
        //za edin i susht symbol dokato ne izteche symbola za koito vechee  vzel

        //Tazi tablica taka ili nache shte se proverqva vseki den zatova e hubavo da ostane entity class.

        public int Id { get; init; }

        //1:M 
        //
        //public User User { get; init; }

        //1:M

        //Vrushtame navPropertitata zaradi OnModelCreating
        public User User { get; init; }

        public int UserId { get; init; }

        //TVA SAMO SE REFERNCEVA I PO NEGO GLEDAME DALI MOJE USER DA DOBAVI

        public int SymbolId { get; init; }

        public DateTime StartDate { get; init; }

        public DateTime EndDate { get; init; }


        public Subscription()
        {

        }

        public Subscription(int symbolId, DateTime startDate, DateTime endDate)
        {

            SymbolId = symbolId;
            StartDate = startDate;
            EndDate = endDate;

        }

        public override bool Equals(object? obj)
        {
            Subscription? castSubscription = obj as Subscription;

            if (castSubscription == null) return false;

            return this.SymbolId == castSubscription.SymbolId;

        }

        public override int GetHashCode()
        {
            return this.SymbolId.GetHashCode();
        }

    }

}
