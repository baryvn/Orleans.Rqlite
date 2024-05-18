namespace Orleans.Persistence.Rqlite.Storage;

internal class StateModel
{
    public string GrainId { get; set; } = string.Empty;

    public string ETag { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;
}