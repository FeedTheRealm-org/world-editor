using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Library;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects
{
    public interface ICreatable : IPersistent<CreatablesData>
    {
        string Id { get; }
        CreatableObjectCategories Category { get; }
    }
}
