using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Xml;
using static OptionStateListSO;

[CreateAssetMenu(fileName = "New OptionStateListSO", menuName = "Setting/OptionStateListSO")]
[System.Serializable]
public class OptionStateListSO : ScriptableObject,IDataLoadable
{


    public List<OptionState> optionStateList;


    /// <summary>
    /// 从此so文件中，找到对应key的选项
    /// </summary>
    /// <param name="settingopt"></param>
    /// <returns></returns>
    public  OptionState GetStateByOption(OptionFuction settingopt)
    {
        for (int i = 0; i < optionStateList.Count; i++)
        {
            if (settingopt.fuctionName == optionStateList[i].key)
            {
                return optionStateList[i];
            }
            else
            {
                if (i == optionStateList.Count - 1)
                {
                    Debug.Log($"没有找到{settingopt}的选项");
                    SaveData();
                }
                
            }
        }
        return null;
    }




    public void LoadData(string json)
    {
        var data = JsonConvert.DeserializeObject<OptionStateListSO>(json);
        IDataLoadable.CopyNewData(this,data);

    }

    /// <summary>
    /// 存储数据
    /// </summary>
    public void SaveData()
    {
        //
        optionStateList = new List<OptionState>();
        var options = UIM_SettingManager.Instance.optionFuctionListSO.OptionFuctionList;
        foreach (var item in options)
        {
            var state = item.optionState;
            optionStateList.Add(new OptionState(state.key, state.value));
        }

        UIM_SaveLoad.SaveData(this, "SettingData");
    }
}


[Serializable]
public class OptionState
{
    
    public string key;
    public int value;

    public OptionState(string _key, int _value)
    {
        key = _key;
        value = _value;

    }


}