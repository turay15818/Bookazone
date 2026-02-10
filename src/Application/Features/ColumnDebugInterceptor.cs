namespace Bookazone.Application.Features;

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

public sealed class ColumnDebugInterceptor : DbCommandInterceptor
{
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        Console.WriteLine("==== DB READER EXECUTED ====");
        Console.WriteLine(command.CommandText);

        for (int i = 0; i < result.FieldCount; i++)
        {
            Console.WriteLine(
                $"Column[{i}] Name={result.GetName(i)} " +
                $"DbType={result.GetDataTypeName(i)}"
            );
        }

        Console.WriteLine("============================");
        return result;
    }
}
