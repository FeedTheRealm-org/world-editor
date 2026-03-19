using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;

namespace FeedTheRealm.Core.Library
{
    public interface ILibrary
    {
        // Returns a dictionary where the key is the item name and the value is a user-friendly display name
        Dictionary<string, string> ListAvailableItems();
        UniTask<GameObject> GetItem(string itemName);
    }
}
