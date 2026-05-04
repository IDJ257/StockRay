using RabbitMQ.Client.Logging;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace StockRay.Models
{
    public class User
    {

        //realno si e konstanta za usera toest
        //Vseki User znae che ima maximum ot 100 uniquen simvola da pokazva
        [NotMapped]
        private const int MAX_STOCKSYMBOLS = 101;


        //EFCORE
        public User()
        {

        }

        public int Id { get; init; }

        public string Email { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }


        private readonly HashSet<Subscription> _subscriptions;

        public IReadOnlyCollection<Subscription> Subscriptions { get => _subscriptions; }


        private readonly HashSet<Symbol> _symbols;

        public IReadOnlyCollection<Symbol> Symbols { get => _symbols; }


        public User(string userName, string password, string email)
        {
            this.UserName = userName;
            this.Password = password;
            this.Email = email;
            _symbols = new HashSet<Symbol>(MAX_STOCKSYMBOLS);
            _subscriptions = new HashSet<Subscription>();

        }




        //Pri persistance-a shte hvashtame problemi koito sa svurzani s persistance problemi
        //Tuk shte proverqvme za C# problemi
        public void ChangeName(string name)
        {
            //za da ne cluttervame modela shte imame drug STRING VALIDATION CLASS
            this.UserName = name;
        }


        public void ChangeEmail(string email)
        {
            //za da ne cluttervame modela shte imame drug EMAIL VALIDATION CLASS
            this.Email = email;
        }

        //tuk shte doide heshiranata parola
        public void ChangePassword(string password)
        {
            this.Password = Password;
        }

        public bool TryAddSymbolToWatch(Symbol symbol)
        {
            //Symvola e random reference no bi trqbvalo
            //tuk da se sledi po ID equals
            if (_symbols.Add(symbol))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //
        public bool RemoveSymbolFromWatch(Symbol symbol)
        {
            if (_symbols.Remove(symbol))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //eventualen batch delete

        public bool TrySubscribeToSymbol(Subscription subscription)
        {
            if (_subscriptions.Add(subscription))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Tova stava realno ako imame
        //10/5/2026-17/5/2026 subscription za THIS user nie prespokoin na 17/5/2026 shte
        //mojem da go iztriem no ne da iztriem SNAPSHOTITE
        public bool TryRemoveSubscription(Subscription subscription)
        {
            if (_subscriptions.Remove(subscription))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}
