using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Persistence.Rqlite.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using RqliteNet;
using System.Text.Json;

namespace Orleans.Persistence.Rqlite.Storage;

public class RqliteGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _storageName;
    private readonly RqliteGrainStorageOptions _options;
    private readonly ClusterOptions _clusterOptions;

    public RqliteGrainStorage(string storageName, RqliteGrainStorageOptions options, IOptions<ClusterOptions> clusterOptions)
    {
        _options = options;
        _clusterOptions = clusterOptions.Value;
        _storageName = _clusterOptions.ServiceId + "_" + storageName;
    }

    private string GetKeyString(string stateName, GrainId grainId)
    {
        return $"{grainId}.{stateName}";
    }

    private Task<List<StateModel>> GetCustomState(string id)
    {
        using (var _client = new RqliteNetClient(_options.Uri))
        {
            return _client.Query<StateModel>("SELECT * FROM " + _storageName + " WHERE GrainId = ?", id);
        }
    }

    public void Participate(ISiloLifecycle observer)
    {
        observer.Subscribe(
        observerName: OptionFormattingUtilities.Name<RqliteGrainStorageOptions>(_storageName),
        stage: ServiceLifecycleStage.ApplicationServices,
        onStart: async (ct) =>
        {
            using (var _client = new RqliteNetClient(_options.Uri))
            {
                var queryResults = await _client.Execute("CREATE TABLE IF NOT EXISTS " + _storageName + " (GrainId TEXT PRIMARY KEY, ETag TEXT, State TEXT)");

            }
        });
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {

        var id = GetKeyString(stateName, grainId);
        var result = await GetCustomState(id);
        if (result.Any())
        {
            if (result.Single().ETag != grainState.ETag)
            {
                throw new InconsistentStateException("ETag mismatch.");
            }
            using (var _client = new RqliteNetClient(_options.Uri))
            {
                await _client.Execute($"DELETE FROM " + _storageName + " WHERE GrainId=?", id);
                grainState.ETag = null;
                grainState.State = Activator.CreateInstance<T>()!;
                grainState.RecordExists = false;
            }
        }
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var id = GetKeyString(stateName, grainId);
        var result = await GetCustomState(id);
        if (!result.Any())
        {
            grainState.State = Activator.CreateInstance<T>()!;
            return;
        }

        var state = result.Single();
        var saveState = JsonSerializer.Deserialize<T>(state.State);
        grainState.State = saveState != null ? saveState : Activator.CreateInstance<T>();
        grainState.ETag = state.ETag;
        grainState.RecordExists = true;
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var dataToSave = grainState.State != null ? JsonSerializer.Serialize(grainState.State) : "{}";

        var id = GetKeyString(stateName, grainId);
        var result = await GetCustomState(id);
        if (result.Any())
        {
            if (result.Single().ETag != grainState.ETag)
            {
                throw new InconsistentStateException("ETag mismatch.");
            }
            using (var _client = new RqliteNetClient(_options.Uri))
            {
                await _client.Execute($"DELETE FROM " + _storageName + " WHERE GrainId=?", id);
            }
        }

        grainState.ETag = Guid.NewGuid().ToString();
        using (var _client = new RqliteNetClient(_options.Uri))
        {
            var insertResult = await _client.Execute($"INSERT INTO " + _storageName + " (GrainId, ETag, State) VALUES (?, ?, ?)", id, grainState.ETag, dataToSave);
            if (insertResult.Results!.Single().RowsAffected != 1)
            {
                throw new InconsistentStateException("Error during row insert.");
            }
        }
        grainState.RecordExists = true;
    }

}