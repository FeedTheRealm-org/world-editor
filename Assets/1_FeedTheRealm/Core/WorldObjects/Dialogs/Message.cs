using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects.Dialogs
{
    public class Message : CreatorObject
    {
        public string Sender;
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                name = value;
            }
        }
        private string _content;
        public string dialogId;

        public Message(MessageData data, string dialogId)
            : base(data.Content, data.id, "")
        {
            Sender = data.Sender;
            Content = data.Content;
            this.dialogId = dialogId;
        }

        public Message(string id, string Sender, string Content, string dialogId)
            : base(Content, id, "")
        {
            this.Sender = Sender;
            this.Content = Content;
            this.dialogId = dialogId;
        }

        public override void DeleteObject(ref WorldDataOld worldData)
        {
            var dialog = worldData.dialogs.Find(d => d.id == dialogId);
            if (dialog != null && dialog.messages != null)
            {
                dialog.messages.RemoveAll(m => m.id == ObjectId);
            }
        }

        public override void SaveObject(ref WorldDataOld worldData)
        {
            MessageData messageData = new(ObjectId, Sender, Content);
            var dialog = worldData.dialogs.Find(d => d.id == dialogId);
            if (dialog != null)
            {
                if (dialog.messages == null)
                    dialog.messages = new List<MessageData>();
                dialog.messages.Add(messageData);
            }
        }
    }
}
