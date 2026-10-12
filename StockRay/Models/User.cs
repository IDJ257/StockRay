using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockRay.Models;

public class User : IdentityUser<int>  //Identity now manages email, password validations
{
    [NotMapped]
    private const int MaxStockSymbols = 101;

    private readonly HashSet<Subscription> _subscriptions;
    private readonly HashSet<Symbol> _symbols;

    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions;

    public IReadOnlyCollection<Symbol> Symbols => _symbols;

    public User()
    {
        _symbols = new HashSet<Symbol>(MaxStockSymbols);
        _subscriptions = new HashSet<Subscription>();
    }

    public User(string userName, string email) : this()
    {
        UserName = userName;
        Email = email;
    }

    public void ChangeEmail(string email) => Email = email;


    public bool TryAddSymbolToWatch(Symbol symbol) => _symbols.Add(symbol);

    public bool RemoveSymbolFromWatch(Symbol symbol) => _symbols.Remove(symbol);

    public bool TrySubscribeToSymbol(Subscription subscription) => _subscriptions.Add(subscription);

    public bool TryRemoveSubscription(Subscription subscription) => _subscriptions.Remove(subscription);
}
