namespace Bookazone.Application.Common.Shared.Helpers;

public static class Money
{
    public static decimal Round(decimal value, int decimals = 2)
        => Math.Round(value, decimals, MidpointRounding.AwayFromZero);
}
