using FeedTheRealm.Core.DataPersistence;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects
{
    public interface ICreatable : IPersistent<CreatablesData>
    {
        string Id { get; }
    }
}
