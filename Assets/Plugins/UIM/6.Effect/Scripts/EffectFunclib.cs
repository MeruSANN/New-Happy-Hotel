// 更简单的工具类
using Cysharp.Threading.Tasks;
using UnityEngine;


/// <summary>
/// 全局工具，接口的入口
/// </summary>
public static class EffectFunclib
{
    private static EffectManager _effectSystem;

    // 注入入口（简化版，无需接口）
    public static void SetEffectSystem(EffectManager effectSystem)
    {
        _effectSystem = effectSystem;
    }

    // 快捷方法
    public static void PlayEffect(string key,Vector3 pos) => _effectSystem?.PlayEffect(key,pos);
    

}