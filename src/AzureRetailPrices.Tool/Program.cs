using Knapcode.AzureRetailPrices.ComputePrices;
using Knapcode.AzureRetailPrices.LoadDatabase;
using Knapcode.AzureRetailPrices.NaturalKeys;
using Knapcode.AzureRetailPrices.OptionalProperties;
using Knapcode.AzureRetailPrices.PropertyRelationships;
using Knapcode.AzureRetailPrices.Snapshot;

var commands = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
{
    { "load-database", () => Task.Run(LoadDatabaseCommand.Run) },
    { "natural-keys", () => NaturalKeysCommand.RunAsync() },
    { "optional-properties", () => Task.Run(OptionalPropertiesCommand.Run) },
    { "property-relationships", () => PropertyRelationshipsCommand.RunAsync() },
    { "compute-prices", () => Task.Run(ComputePricesCommand.Run) },
    { "snapshot", () => SnapshotCommand.RunAsync() },
};

if (args.Length == 0)
{
    Console.WriteLine("A command name must be provided as the first argument.");
    ShowCommands(commands);
    return 1;
}
else if (!commands.TryGetValue(args[0], out var command))
{
    Console.WriteLine("The provided command name is not valid.");
    ShowCommands(commands);
    return 1;
}
else
{
    await command();
    return 0;
}

void ShowCommands(Dictionary<string, Func<Task>> commands)
{
    Console.WriteLine("Valid commands: " + string.Join(" ", commands.Keys.OrderBy(x => x)));
}