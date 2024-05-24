using Orleans.Storage;

namespace Orleans.Persistence.Rqlite.Providers;

public class RqliteGrainStorageOptions : IStorageProviderSerializerOptions
{
    public required string Uri { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public required IGrainStorageSerializer GrainStorageSerializer { get; set; }
}