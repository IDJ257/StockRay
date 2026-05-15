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
            _symbols = new HashSet<Symbol>(MAX_STOCKSYMBOLS);
        }

        public int Id { get; init; }


        private string _email;

        public string Email
        {
            get { return _email; }
            private set
            {
                if (IsEmailValid(value))
                {
                    _email = value;
                }
            }
        }


        private string _userName;

        public string UserName
        {
            get { return _userName; }
            private set
            {
                //nqq da e tru false
                if (IsNameValid(value))
                {
                    _userName = value;
                }



            }
        }


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
            this.Password = password;
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



        //eventualno drug return tip
        private static bool IsNameValid(string nameToBeTested)
        {

            //poneje e edno i sushto i za trite moje da se izkara na 1 
            if (string.IsNullOrWhiteSpace(nameToBeTested)) throw new FormatException("Name is empty");

            if (nameToBeTested.Any(x => !char.IsLetter(x))) throw new FormatException("Name contains something that is not a letter");

            return true;
        }

        private static bool IsEmailValid(string emailToBeTested)
        {
            if (string.IsNullOrWhiteSpace(emailToBeTested)) throw new FormatException("Name is empty");

            if (!emailToBeTested.Contains('@')) throw new FormatException("Invalid email");

            //tehcnicheski e bezmiselno ama fun
            if (emailToBeTested.Split('@')[0].Any(x => !char.IsLetter(x))) throw new FormatException("Invalid email chars");



            return true;
        }

    }

}
