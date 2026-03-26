using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class Dialog : ICreatable
    {
        public DialogData data { get; private set; }

        public Dialog(DialogData data)
        {
            this.data = data;
        }

        public string Id => data.id;

        public void SaveData(ref CreatablesData data)
        {
            data.dialogs.Add(this.data);
        }
    }
}
