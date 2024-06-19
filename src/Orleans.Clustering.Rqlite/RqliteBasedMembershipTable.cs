
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using RqliteNet;
using Orleans.Clustering.Rqlite;
using System.Text.Json;
using System.Data;

namespace Orleans.Runtime.Membership
{

    public class RqliteBasedMembershipTable : IMembershipTable
    {
        private readonly ILogger logger;

        private readonly string Uri;
        private readonly string ClusterId;
        private static readonly TableVersion DefaultTableVersion = new TableVersion(0, "0");

        public bool IsInitialized { get; private set; }
        public RqliteBasedMembershipTable(
            ILogger<RqliteBasedMembershipTable> logger,
            IOptions<RqliteClusteringSiloOptions> membershipTableOptions,
            IOptions<ClusterOptions> clusterOptions)
        {
            this.logger = logger;
            var options = membershipTableOptions.Value;
            ClusterId = clusterOptions.Value.ClusterId;
            Uri = options.Uri;

        }
        /// <summary>
        /// Initialize Membership Table
        /// </summary>
        /// <param name="tryInitTableVersion"></param>
        /// <returns></returns>
        public async Task InitializeMembershipTable(bool tryInitTableVersion)
        {
            if (tryInitTableVersion)
            {
                try
                {
                    using (var _rqlite = new RqliteNetClient(this.Uri))
                    {
                        await _rqlite.Execute("CREATE TABLE IF NOT EXISTS " + ClusterId + "_Members (SiloAddress TEXT PRIMARY KEY, Data TEXT)");
                        await _rqlite.Execute("CREATE TABLE IF NOT EXISTS TableVersion (ClusterId TEXT PRIMARY KEY, Data TEXT)");
                        var select = _rqlite.Query<TableVersionModel>(string.Format("SELECT * FROM TableVersion WHERE ClusterId = '{0}'", this.ClusterId));
                        select.Wait();
                        if (!select.Result.Any())
                        {
                            var create = _rqlite.Execute(string.Format("INSERT INTO TableVersion(ClusterId,Data) VALUES('{0}','{1}')", this.ClusterId, JsonSerializer.Serialize(DefaultTableVersion)));
                            create.Wait();
                        }
                    }
                    IsInitialized = true;

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Atomically reads the Membership Table information about a given silo.
        /// The returned MembershipTableData includes one MembershipEntry entry for a given silo and the 
        /// TableVersion for this table. The MembershipEntry and the TableVersion have to be read atomically.
        /// </summary>
        /// <param name="siloAddress">The address of the silo whose membership information needs to be read.</param>
        /// <returns>The membership information for a given silo: MembershipTableData consisting one MembershipEntry entry and
        /// TableVersion, read atomically.</returns>
        public async Task<MembershipTableData> ReadRow(SiloAddress siloAddress)
        {
            try
            {
                TableVersion tableVersion = DefaultTableVersion;
                using (var _rqlite = new RqliteNetClient(this.Uri))
                {
                    var select = await _rqlite.Query<TableVersionModel>(string.Format("SELECT * FROM TableVersion WHERE ClusterId = '{0}'", ClusterId));
                    if (select.Any())
                    {
                        var state = select.Single();
                        var table = JsonSerializer.Deserialize<TableVersionData>(state.Data);
                        if (table != null)
                        {
                            tableVersion = new TableVersion(table.Version, table.VersionEtag);
                        }
                    }

                    var result = await _rqlite.Query<MemberModel>(string.Format("SELECT * FROM " + ClusterId + "_Members WHERE SiloAddress = '{0}'", siloAddress.ToString()));
                    var member = result.Select(p => Tuple.Create(JsonSerializer.Deserialize<MembershipEntry>(p.Data), tableVersion.VersionEtag)).FirstOrDefault();
                    if (member != null)
                    {
                        return new MembershipTableData(member, tableVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            return new MembershipTableData(DefaultTableVersion);
        }

        /// <summary>
        /// Atomically reads the full content of the Membership Table.
        /// The returned MembershipTableData includes all MembershipEntry entry for all silos in the table and the 
        /// TableVersion for this table. The MembershipEntries and the TableVersion have to be read atomically.
        /// </summary>
        /// <returns>The membership information for a given table: MembershipTableData consisting multiple MembershipEntry entries and
        /// TableVersion, all read atomically.</returns>
        public async Task<MembershipTableData> ReadAll()
        {
            try
            {
                TableVersion tableVersion = DefaultTableVersion;
                using (var _rqlite = new RqliteNetClient(this.Uri))
                {
                    var select = await _rqlite.Query<TableVersionModel>(string.Format("SELECT * FROM TableVersion WHERE ClusterId = '{0}'", this.ClusterId));
                    if (select.Any())
                    {
                        var state = select.Single();
                        var table = JsonSerializer.Deserialize<TableVersionData>(state.Data);
                        if (table != null)
                        {
                            tableVersion = new TableVersion(table.Version, table.VersionEtag);
                        }
                    }
                    var result = await _rqlite.Query<MemberModel>("SELECT * FROM " + ClusterId + "_Members");
                    var members = result.Select(p => Tuple.Create(JsonSerializer.Deserialize<MembershipEntry>(p.Data), tableVersion.VersionEtag)).ToList();
                    return new MembershipTableData(members, tableVersion);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            return new MembershipTableData(DefaultTableVersion);
        }


        /// <summary>
        /// Atomically tries to insert (add) a new MembershipEntry for one silo and also update the TableVersion.
        /// If operation succeeds, the following changes would be made to the table:
        /// 1) New MembershipEntry will be added to the table.
        /// 2) The newly added MembershipEntry will also be added with the new unique automatically generated eTag.
        /// 3) TableVersion.Version in the table will be updated to the new TableVersion.Version.
        /// 4) TableVersion etag in the table will be updated to the new unique automatically generated eTag.
        /// All those changes to the table, insert of a new row and update of the table version and the associated etags, should happen atomically, or fail atomically with no side effects.
        /// The operation should fail in each of the following conditions:
        /// 1) A MembershipEntry for a given silo already exist in the table
        /// 2) Update of the TableVersion failed since the given TableVersion etag (as specified by the TableVersion.VersionEtag property) did not match the TableVersion etag in the table.
        /// </summary>
        /// <param name="entry">MembershipEntry to be inserted.</param>
        /// <param name="tableVersion">The new TableVersion for this table, along with its etag.</param>
        /// <returns>True if the insert operation succeeded and false otherwise.</returns>
        public async Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            return await InsertOrUpdateMember(entry, tableVersion, true);
        }

        /// <summary>
        /// Atomically tries to update the MembershipEntry for one silo and also update the TableVersion.
        /// If operation succeeds, the following changes would be made to the table:
        /// 1) The MembershipEntry for this silo will be updated to the new MembershipEntry (the old entry will be fully substituted by the new entry) 
        /// 2) The eTag for the updated MembershipEntry will also be eTag with the new unique automatically generated eTag.
        /// 3) TableVersion.Version in the table will be updated to the new TableVersion.Version.
        /// 4) TableVersion etag in the table will be updated to the new unique automatically generated eTag.
        /// All those changes to the table, update of a new row and update of the table version and the associated etags, should happen atomically, or fail atomically with no side effects.
        /// The operation should fail in each of the following conditions:
        /// 1) A MembershipEntry for a given silo does not exist in the table
        /// 2) A MembershipEntry for a given silo exist in the table but its etag in the table does not match the provided etag.
        /// 3) Update of the TableVersion failed since the given TableVersion etag (as specified by the TableVersion.VersionEtag property) did not match the TableVersion etag in the table.
        /// </summary>
        /// <param name="entry">MembershipEntry to be updated.</param>
        /// <param name="etag">The etag  for the given MembershipEntry.</param>
        /// <param name="tableVersion">The new TableVersion for this table, along with its etag.</param>
        /// <returns>True if the update operation succeeded and false otherwise.</returns>
        public async Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            return await InsertOrUpdateMember(entry, tableVersion, true);
        }
        private async Task<bool> InsertOrUpdateMember(MembershipEntry entry, TableVersion tableVersion, bool updateTableVersion)
        {
            try
            {
                using (var _rqlite = new RqliteNetClient(this.Uri))
                {
                    if (updateTableVersion)
                    {
                        if (tableVersion.Version == 0 && "0".Equals(tableVersion.VersionEtag, StringComparison.Ordinal))
                        {
                            var select = await _rqlite.Query<TableVersionModel>(string.Format("SELECT * FROM TableVersion WHERE ClusterId = '{0}'", this.ClusterId));
                            if (!select.Any())
                            {
                                await _rqlite.Execute(string.Format("INSERT INTO TableVersion(ClusterId,Data) VALUES('{0}','{1}')", this.ClusterId, JsonSerializer.Serialize(DefaultTableVersion)));
                            }
                        }
                        else
                        {
                            await _rqlite.Execute(string.Format("UPDATE TableVersion SET Data = '{0}' WHERE ClusterId = '{1}'", JsonSerializer.Serialize(tableVersion), ClusterId));
                        }
                    }
                    var member = await _rqlite.Query<MemberModel>(string.Format("SELECT * FROM " + ClusterId + "_Members WHERE SiloAddress = '{0}'", entry.SiloAddress.ToString()));
                    if (!member.Any())
                    {
                        await _rqlite.Execute("INSERT INTO " + ClusterId + string.Format("_Members(SiloAddress,Data) VALUES('{0}','{1}')", entry.SiloAddress.ToString(), JsonSerializer.Serialize(entry)));
                    }
                    else
                    {
                        await _rqlite.Execute("UPDATE " + ClusterId + string.Format("_Members SET Data = '{0}' WHERE SiloAddress = '{1}'", JsonSerializer.Serialize(entry), entry.SiloAddress.ToString()));
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, ex.Message);
            }
            return false;
        }
        /// <summary>
        /// Updates the IAmAlive part (column) of the MembershipEntry for this silo.
        /// This operation should only update the IAmAlive column and not change other columns.
        /// This operation is a "dirty write" or "in place update" and is performed without etag validation. 
        /// With regards to eTags update:
        /// This operation may automatically update the eTag associated with the given silo row, but it does not have to. It can also leave the etag not changed ("dirty write").
        /// With regards to TableVersion:
        /// this operation should not change the TableVersion of the table. It should leave it untouched.
        /// There is no scenario where this operation could fail due to table semantical reasons. It can only fail due to network problems or table unavailability.
        /// </summary>
        /// <param name="entry">The target MembershipEntry tp update</param>
        /// <returns>Task representing the successful execution of this operation. </returns>
        public async Task UpdateIAmAlive(MembershipEntry entry)
        {
            try
            {
                TableVersion tableVersion = DefaultTableVersion;
                using (var _rqlite = new RqliteNetClient(this.Uri))
                {
                    var select = await _rqlite.Query<TableVersionModel>(string.Format("SELECT * FROM TableVersion WHERE ClusterId = '{0}'", this.ClusterId));
                    if (select.Any())
                    {
                        var state = select.Single();
                        var table = JsonSerializer.Deserialize<TableVersionData>(state.Data);
                        if (table != null)
                        {
                            tableVersion = new TableVersion(table.Version, table.VersionEtag).Next();
                        }
                    }
                    var result = await _rqlite.Query<MemberModel>(string.Format("SELECT * FROM " + ClusterId + "_Members WHERE SiloAddress = '{0}'", entry.SiloAddress.ToString()));
                    var member = result.Select(p => JsonSerializer.Deserialize<MembershipEntry>(p.Data)).FirstOrDefault();
                    if (member != null)
                    {
                        member.IAmAliveTime = entry.IAmAliveTime;
                        await InsertOrUpdateMember(member, tableVersion, updateTableVersion: false);
                    }
                    else
                    {
                        throw new Exception("Member not found!");
                    }

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Deletes all table entries of the given clusterId
        /// </summary>
        public async Task DeleteMembershipTableEntries(string clusterId)
        {
            using (var _rqlite = new RqliteNetClient(this.Uri))
            {
                await _rqlite.Execute("DELETE FROM  " + ClusterId + "_Members");
            }
        }

        public async Task CleanupDefunctSiloEntries(DateTimeOffset beforeDate)
        {
            using (var _rqlite = new RqliteNetClient(this.Uri))
            {
                await _rqlite.Execute(string.Format("DELETE FROM  " + ClusterId + "_Members WHERE json_extract(Data,'$.Status') = {0} AND json_extract(Data,'$.IAmAliveTime') < '{1}'", SiloStatus.Dead, beforeDate.ToString("o")));
            }
        }
    }


}
