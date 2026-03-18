using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;

namespace FeedTheRealm.Core.Library
{
    public interface ILibrary
    {
        List<string> ListAvailableItems();
        UniTask<GameObject> GetItem(string itemName);
    }
}
