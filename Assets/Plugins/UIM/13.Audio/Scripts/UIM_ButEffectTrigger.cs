using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

[AddComponentMenu("UIMaster/Button/ButEffectTrigger")]
[DisallowMultipleComponent]
public class UIM_ButEffectTrigger : MonoBehaviour, ISelectHandler,IPointerEnterHandler
{

    //包装一下按钮，不只是发出效果
    public string presskey="id_click_01";
    public string selectkey = "id_click_02";
    public string pointerEnterkey = "id_pointerEnter_03";

    public bool isShake;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioFunclib.PlaySFX(pointerEnterkey);
    }


    public void OnSelect(BaseEventData eventData)
    {
        // 打印出被选中按钮的名字
        //UIM_DebugManager.Log($"按钮{gameObject.name}被选中", "成功","UI");
        AudioFunclib.PlaySFX(selectkey);

     
    }

    public void OnPressEff()
    {
        AudioFunclib.PlaySFX(presskey);
        // 打印出被选中按钮的名字
        UIM_DebugManager.Log($"玩家按下了按钮{gameObject.name}", "成功","UI");
        //加入镜头抖动效果
  
        if (UIM_PanelsManager.Instance&& isShake)
        {
            UIM_PanelsManager.Instance.TriggerCanvasAnimation();
        }
    }


    private void Awake()
    {
        if (GetComponent<Button>())
        {
            GetComponent<Button>().onClick.AddListener(OnPressEff);
        }
        if (GetComponent<Toggle>())
        {
            GetComponent<Toggle>().onValueChanged.AddListener(delegate { OnPressEff(); });
        }
       

        //SFPlayer = GetComponent<AudioSource>() ?
        //    GetComponent<AudioSource>() : gameObject.AddComponent<AudioSource>();
    }


    [Button("设置默认key")]
    void RestKeyDefault()
    {
        presskey = "id_click_01";
        selectkey = "id_click_02";
        pointerEnterkey = "id_pointerEnter_03";
    }



}
