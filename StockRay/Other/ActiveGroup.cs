using System.Collections.Concurrent;

namespace StockRay.Other
{

    public interface IActiveGroup
    {

        void AddToGroup(string connectionString, List<string> groups);

        void RemoveWhenLostConnection(string connectionString);

        void LeaveGroup(string connectionString, string group);

        public IReadOnlyDictionary<string, List<string>> ConnectionGroups { get; }



    }

    public class ActiveGroup : IActiveGroup
    {
        //MOJE I HASHSET<T>
        //MINAVA SE NA LIST SHOTO BAGA E NAPRAVEN DA SE DISPOSEVA. AKO IMA HANG 
        //PROSTO SHTE NAPRAVIM LOCK{}
   
        //CONNECTION ID -> LSIT OT GROUPI
        private readonly ConcurrentDictionary<string, List<string>> _connectionGroups;

        public IReadOnlyDictionary<string, List<string>> ConnectionGroups { get => _connectionGroups; }


        public ActiveGroup()
        {

            _connectionGroups = new ConcurrentDictionary<string, List<string>>();
             

        }

        public void AddToGroup(string connectionId, List<string> groups)
        {
            //Shte vidim dali shte dade error ako doide edno i sushto connectionId
            //za da doide edno i sushto neshto ne se e terminiralo
            //kato dade error togava sh mislim

            if (_connectionGroups.ContainsKey(connectionId))
            {
                //za sega taka sh raboti
                if(groups.Count == 1)
                {
                    _connectionGroups[connectionId].Add(groups[0]);
                }
                else
                {
                    foreach (var item in groups)
                    {
                        _connectionGroups[connectionId].Add(item);
                    }
                }
              
            }
            else
            {
                _connectionGroups.TryAdd(connectionId, new List<string>(groups));
            }

               
            

        }

        public void LeaveGroup(string connectionId, string group)
        {
            if (_connectionGroups.ContainsKey(connectionId))
            {
                _connectionGroups[connectionId].Remove(group);
            }
            else
            {
                throw new ArgumentException("Connection error");
            }
        }

        public void RemoveWhenLostConnection(string connectionId)
        {
            _connectionGroups.TryRemove(connectionId, out var _);
        }

    }

}
