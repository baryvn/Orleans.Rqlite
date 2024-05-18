using Orleans.Runtime;

namespace Orleans.Clustering.Rqlite
{
    public class RqliteMembershipTable : IMembershipTable
    {
        public Task CleanupDefunctSiloEntries(DateTimeOffset beforeDate)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMembershipTableEntries(string clusterId)
        {
            throw new NotImplementedException();
        }

        public Task InitializeMembershipTable(bool tryInitTableVersion)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            throw new NotImplementedException();
        }

        public Task<MembershipTableData> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Task<MembershipTableData> ReadRow(SiloAddress key)
        {
            throw new NotImplementedException();
        }

        public Task UpdateIAmAlive(MembershipEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            throw new NotImplementedException();
        }
    }
}