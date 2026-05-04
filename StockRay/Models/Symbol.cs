namespace StockRay.Models
{
    public class Symbol
    {
        public int Id { get; init; }


        //UNIQUENESS ama 
        public string Name { get; init; }

        //bez back field tova e samo za relationshipa.
        //Nqma da ima sluchai v koito da polzvamwe Symbols.Users.AddUser()

        //da se vidi dali shte dade problem ako ne e instancenat0
        public IReadOnlyCollection<User> Users { get; private set; }

        //1:M Snapshots

        //NE BI TRQBVALO DA STANE GRESHKA DA SE DOBAVQT DVA SNAPSHOTA ZA 
        //EDIN SYMBOL V EDIN DEN ZATVA LSIT A NE HASHSET
        private readonly List<Snapshot> _snapShots;

        public IReadOnlyCollection<Snapshot> SnapShots { get { return _snapShots; } }

        public Symbol()
        {

        }


        public Symbol(string name)
        {
            Name = name;
            _snapShots = new List<Snapshot>();
        }


        public void AddSnapshot(Snapshot snapshot)
        {
            _snapShots.Add(snapshot);
        }


        public override bool Equals(object? obj)
        {

            Symbol? castedSym = obj as Symbol;

            if (castedSym == null) return false;

            return this.Id == castedSym.Id;

            //castvame zashtoto Equals shte se callne samo pri .add na hashseta.

        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }



    }

}
