

using System.Reflection;


using UnityEngine;

/// <summary>
/// 将数据的存储能力做的更加解耦合
/// </summary>

public interface IDataLoadable 
{
    void LoadData(string json);

    void SaveData();

    /// <summary>
    /// 反射，方便为class覆盖数据
    /// </summary>
    /// <param name="target"></param>
    public static void CopyNewData<T>(T origan, T target) where T : ScriptableObject
    {
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            field.SetValue(origan, field.GetValue(target));
        }
    }

}
