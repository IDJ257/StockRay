using System.Diagnostics.Contracts;

namespace StockRay.Models
{
    public class Symbol
    {


        public int Id { get; init; }


        //UNIQUENESS ama 
        public string Name { get; init; }


        public bool IsTopNine { get; private set; }


        

        public float Open { get; private set; }
        

        public float High { get; private set; }

        public float Low { get; private set; }

        public float Close { get; private set; }    


       


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
            Open = 0f;
            High = 0f;
            Low = 0f;
            Close = 0f;
            _snapShots = new List<Snapshot>();
        }


        public void SetOpen(float open)
        {
            if (CheckForNormalFloat(open))
            {
                Open = open;
            }
        }

        public void SetHigh(float high)
        {
            if (CheckForNormalFloat(high))
            {
                High = high;
            }
        }


        public void SetLow(float low)
        {
            if (CheckForNormalFloat(low))
            {
                Low = low;
            }
        }

        public void SetClose(float close)
        {
            if (CheckForNormalFloat(close))
            {
                Close = close;
            }
        }



        public void SetTopNine()
        {
            IsTopNine = true;
        }

        public void DeSetTopNine()
        {
            IsTopNine = false;
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


        //Moje da se ostavi STATIC
        private static bool CheckForNormalFloat(float value)
        {
            return float.IsNormal(value);
        }


    }

}
