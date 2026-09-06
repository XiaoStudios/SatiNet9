using Microsoft.AspNetCore.SignalR;
using Sati_Net_Last.API.Repositories.Interfaces;

namespace Sati_Net_Last.API.Hubs;

public class MetaTraderHub : Hub
{
    private readonly IAuthRepository _authRepository;

    public MetaTraderHub(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task JoinSymbol(int userId, string symbol)
    {
        if (userId <= 0)
            throw new HubException("Usuario inválido.");

        if (string.IsNullOrWhiteSpace(symbol))
            throw new HubException("El símbolo es obligatorio.");

        var normalizedSymbol = NormalizeSymbol(symbol);
        var allowedSymbols = await _authRepository.GetActiveSymbolsByUserIdAsync(userId);

        var hasAccess = allowedSymbols.Any(x =>
            string.Equals(NormalizeSymbol(x), normalizedSymbol, StringComparison.Ordinal));

        if (!hasAccess)
            throw new HubException("No tienes permiso para este símbolo.");

        await Groups.AddToGroupAsync(Context.ConnectionId, BuildSymbolGroup(normalizedSymbol));
    }

    public async Task LeaveSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildSymbolGroup(NormalizeSymbol(symbol)));
    }

    public static string BuildSymbolGroup(string symbol) => $"symbol:{NormalizeSymbol(symbol)}";

    private static string NormalizeSymbol(string symbol) => symbol.Trim().ToUpperInvariant();
}
