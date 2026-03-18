using System;

namespace FeedTheRealm.Core.WorldSetup
{
    public interface ISetup : IDisposable
    {
        void Setup();
    }
}
