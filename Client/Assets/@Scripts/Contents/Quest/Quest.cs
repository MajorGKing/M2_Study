using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using GameServer.Game;
using Google.Protobuf.Protocol;

public class Quest
{
    public QuestInfo Info { get; private set; } = new QuestInfo();
    public QuestData QuestData { get; private set; }
    public List<QuestTask> QuestTasks { get; } = new List<QuestTask>();

    public event Action<Quest> OnQuestCompleted;

    public QuestTask CurrentTask { get; set; }

    public Quest(QuestInfo questInfo)
    {
		UpdateQuest(questInfo);
    }

	private void SetCurrentProcessingTask()
	{
		foreach (QuestTask task in QuestTasks)
		{
			// 진행중인 마지막 태스크 발견.
			if (task.IsCompleted == false)
			{
				CurrentTask = task;
				return;
			}
		}

		// 모든 태스크 종료.
		CurrentTask = null;
	}

	public void UpdateQuest(QuestInfo questInfo)
    {
		if (Managers.Data.QuestDict.TryGetValue(questInfo.TemplateId, out QuestData questData) == false)
			return;

		EQuestState prevState = State;

		Info = questInfo;
		QuestData = questData;
		QuestTasks.Clear();

		for (int i = 0; i < questInfo.TaskInfos.Count; i++)
		{
			QuestTaskInfo taskInfo = questInfo.TaskInfos[i];
			int taskTemplateId = QuestData.TaskIds[i];
			QuestTasks.Add(new QuestTask(taskTemplateId, taskInfo));
		}

		SetCurrentProcessingTask();

        if (prevState == EQuestState.Processing && State == EQuestState.Rewarded)
            ShowCompletePopup();
    }

    private void ShowCompletePopup()
    {
        // 메시지를 담을 리스트 생성
        List<string> msgs = new List<string>
        {
            $"{QuestData.NameTextId} 완료 !",                            
            $"경험치 : {QuestData.RewardTableData.RewardExp} 획득",        
            $"골드 : {QuestData.RewardTableData.RewardGold} 획득"  
        };

        // 보상 아이템 이름 추가
        msgs.AddRange(QuestData.RewardTableData.Rewards.Select(reward => $"{reward.Item.NameTextId} 획득"));

        // 메시지들을 하나의 문자열로 합치기
        string res = string.Join("\n", msgs);

        // 팝업 메시지 출력
        Managers.UI.ShowMessage(Define.MsgPopupType.Yes, res);
    }

	#region Helpers
	public static Quest MakeQuest(QuestInfo questInfo)
	{
		if (Managers.Data.QuestDict.TryGetValue(questInfo.TemplateId, out QuestData questData) == false)
			return null;

		Quest quest = new Quest(questInfo);
		return quest;
	}

	public int TemplateId
	{
		get { return Info.TemplateId; }
		set { Info.TemplateId = value; }
	}

	public EQuestState State
	{
		get { return Info.QuestState; }
		set { Info.QuestState = value; }
	}
	#endregion
}