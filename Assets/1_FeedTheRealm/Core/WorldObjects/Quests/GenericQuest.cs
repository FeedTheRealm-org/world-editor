using Enums;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects.Quests
{
    public class GenericQuest : CreatorObject
    {
        public string content;
        public QuestType questType;
        public string targetId;
        public int targetAmount;

        public GenericQuest(QuestData questData)
            : base(questData.title, questData.id)
        {
            content = questData.content;
            questType = questData.type;
            targetId = questData.targetId;
            targetAmount = questData.targetAmount;
        }

        public override void DeleteObject(ref WorldDataOld worldData)
        {
            worldData.quests.RemoveAll(quest => quest.id == ObjectId);
        }

        public override void SaveObject(ref WorldDataOld worldData)
        {
            QuestData questData = new(
                ObjectId,
                DisplayName,
                content,
                questType,
                targetId,
                targetAmount,
                null
            );
            worldData.quests.Add(questData);
        }
    }
}
