using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects.Dialogs
{
    public class Dialog : CreatorObject
    {
        public Dialog(DialogData dialogData)
            : base(dialogData.name, dialogData.id, "") { }

        public override void DeleteObject(ref WorldDataOld worldData)
        {
            worldData.dialogs.RemoveAll(d => d.id == ObjectId);
        }

        public override void SaveObject(ref WorldDataOld worldData)
        {
            DialogData dialogData = new(ObjectId, name);
            worldData.dialogs.Add(dialogData);
        }
    }
}
