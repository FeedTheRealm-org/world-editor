using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class Dialog : Creatable
    {
        public DialogData data { get; private set; }

        public Dialog(DialogData data)
        {
            this.data = data;
        }

        public override string Id => data.id;

        public override void Save(ref CreatablesData data)
        {
            data.dialogs.Add(this.data);
        }
    }
}
