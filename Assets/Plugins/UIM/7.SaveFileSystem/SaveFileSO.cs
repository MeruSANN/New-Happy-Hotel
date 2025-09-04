using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "SaveDataSO", menuName = "GameData/SaveFile")]
public class SaveFileSO : ScriptableObject,IDataLoadable
{


    [TabGroup("Basic Info"), LabelText("运行时间（秒）")]
    public float playTime;

    [TabGroup("Basic Info"), LabelText("完成度（百分比）")]
    public float completionRate;

    [TabGroup("Scores"), LabelText("历史最高分列表")]
    public List<float> highScores;

    [TabGroup("Puzzle Tracking"), LabelText("已解开的谜题数")]
    public int puzzlesSolved;

    [TabGroup("Puzzle Tracking"), LabelText("使用的提示次数")]
    public int hintsUsed;

    [TabGroup("Puzzle Tracking"), LabelText("失败尝试次数")]
    public int failedAttempts;

    [TabGroup("Puzzle Tracking"), LabelText("找到的隐藏要素数")]
    public int secretsFound;

    [TabGroup("Special Flags"), LabelText("特殊结局是否解锁")]
    public bool isSpecialEndingUnlocked;

    public void LoadData(string json)
    {
        var data = JsonConvert.DeserializeObject<SaveFileSO>(json);
        IDataLoadable.CopyNewData(this, data);
    }

    public void SaveData()
    {
      
    }
}
