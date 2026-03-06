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
            : base(questData.Title, questData.Id)
        {
            content = questData.Content;
            questType = questData.Type;
            targetId = questData.TargetId;
            targetAmount = questData.TargetAmount;
        }

        public override void DeleteObject(ref WorldData worldData)
        {
            worldData.quests.RemoveAll(quest => quest.Id == ObjectId);
        }

        public override void SaveObject(ref WorldData worldData)
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
