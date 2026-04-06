using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.WorldEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Core.Library
{
    public abstract class PrefabLibrary : ILibrary
    {
        private readonly Dictionary<string, GameObject> prefabs = new();
        private readonly IObjectResolver resolver;

        protected PrefabLibrary(IObjectResolver resolver)
        {
            this.resolver = resolver;
        }

        protected void Register(string id, GameObject prefab)
        {
            prefabs[id] = prefab;
        }

        public UniTask<GameObject> GetItem(string id)
        {
            if (!prefabs.TryGetValue(id, out var prefab))
                return UniTask.FromResult<GameObject>(null);

            return UniTask.FromResult(resolver.Instantiate(prefab));
        }

        public abstract List<PlaceableOption> ListAvailableItems();
    }
}
