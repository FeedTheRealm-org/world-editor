using FeedTheRealm.Core.WorldObjects.Provider;
using FTRShared.Runtime.Models;
using VContainer;

namespace FTR.Core.Common.Loaders
{
    public interface IInitializeable<T>
    {
        void Initialize(T data);
    }
}
