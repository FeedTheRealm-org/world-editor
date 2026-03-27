using System.Runtime.InteropServices;
using FeedTheRealm.Core.DataPersistence;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using VContainer;

namespace FeedTheRealm.Core.WorldObjects
{
    public abstract class Creatable : IPersistent<CreatablesData>
    {
        /// <summary>
        ///  When a creatable is added to the CreatablesManager,
        ///  it gets injected through the container, so you can add any dependencies you need
        ///  The dependencies are not injected via constructor, but directly into the fields,
        /// so you can have a parameterless constructor and still have access to any service registered in the container.
        /// </summary>
        [Inject]
        protected Config config;

        public abstract string Id { get; }

        public bool IsDeleted = false;

        public void Delete()
        {
            IsDeleted = true;
        }

        /// <summary>
        /// Called when a creatable is marked as deleted.
        /// Use this to perform any cleanup necessary, like removing sprites that
        /// might still be persisted in the world sprites folder or any other data
        /// that might be referenced from other objects.
        /// </summary>
        public virtual void OnDelete() { }

        public void SaveData(ref CreatablesData data)
        {
            if (IsDeleted)
            {
                OnDelete();
                return;
            }
            Save(ref data);
        }

        public abstract void Save(ref CreatablesData data);
    }
}
