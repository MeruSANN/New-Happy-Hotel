using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace UIM{

[AddComponentMenu("UIMaster/UIManager")]
public class UIM_UIManager : UIM_UniversalManager<UIM_UIManager>
{

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

    }

    public void Start()
    {

        InitializeInputSystem();

    }

    private void OnDestroy()
    {

        StopAllCoroutines();
    }

    /// <summary>
    /// 初始化控制
    /// </summary>
    private void InitializeInputSystem()
    {

        RefreshLayoutsRecursively(); //初始化刷新
    }

    /// <summary>
    /// 将UI刷新
    /// </summary>
    public void RefreshLayoutsRecursively(Transform parent = null)
    {
        if (this == null) return;
        RefreshLayoutsAsync(parent).Forget(); // 使用 Forget() 表示不关心 Task 是否完成
    }

    private async UniTask RefreshLayoutsAsync(Transform parent = null)
    {
        parent = parent ?? transform;

        if (IsTransformValid(parent))
            RefreshLayoutsImmediate(parent);

        await UniTask.Yield(); // 等同于 yield return null（等待一帧）

        if (IsTransformValid(parent))
        {
            await UniTask.Delay(1, DelayType.UnscaledDeltaTime); // 等待 0.2 秒（200ms）
            RefreshLayoutsImmediate(parent);
        }
    }


    private void RefreshLayoutsImmediate(Transform parent)
    {
        var layouts = parent.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true);
        var fitters = parent.GetComponentsInChildren<ContentSizeFitter>(true);

        foreach (var item in layouts)
        {
            if (item != null && item.transform != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        }

        foreach (var item in fitters)
        {
            if (item != null && item.transform != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        }
    }

   
    private bool IsTransformValid(Transform target)
    {
        return target != null && target.gameObject != null && target.gameObject.activeInHierarchy;
    }

   
    public void StartQuit()
    {
        StartCoroutine(DelayQuit(1f));
        UIM_MainMenu.Instance?.frameOptionsAC.SetTrigger("ISSWITCH");
    }

    private IEnumerator DelayQuit(float delay)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }

  
    public static void SwitchPanel(GameObject panel)
    {
        if (panel == null) return;

        var animator = panel.GetComponent<Animator>();
        if (!panel.activeInHierarchy)
        {
            panel.SetActive(true);
        }
        else if (animator != null)
        {
            animator.SetBool("ISSWITCH", true);
        }
        else
        {
            panel.SetActive(false);
        }
    }


    private void Update()
    {
       //如果当前选择对象不存在，如果当前活动面板存在
        if (EventSystem.current.currentSelectedGameObject == null&& _currentActivePanel!=null)
        {
           
            TrySelectDefaultButton(_currentActivePanel);
        }
    }


    #region panel控制

  
    [SerializeField]
    private GameObject _currentActivePanel;

 
    public void OnPanelOpened(GameObject panel)
    {
        _currentActivePanel = panel;
        TrySelectDefaultButton(panel);
    }

  
    /// <summary>
    /// 尝试选择默认按钮
    /// </summary>
    /// <param name="panel"></param>
    private void TrySelectDefaultButton(GameObject panel)
    {
        if (EventSystem.current == null) return;

        var panelManager = panel.GetComponent<UIM_PanelManager>();
        if (panelManager != null && panelManager.DefaultButton != null)
        {
            EventSystem.current.firstSelectedGameObject = panelManager.DefaultButton;
            EventSystem.current.SetSelectedGameObject(panelManager.DefaultButton);
        }
    }



    #endregion


 
    public static void RebuildVerticalNavigation(Transform parent)
    {
        List<Selectable> buttons = new List<Selectable>();

        foreach (Transform child in parent)
        {
            Selectable s = child.GetComponent<Selectable>();
            if (s != null)
                buttons.Add(s);
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            Navigation nav = buttons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = i > 0 ? buttons[i - 1] : null;
            nav.selectOnDown = i < buttons.Count - 1 ? buttons[i + 1] : null;

            buttons[i].navigation = nav;
        }
    }


}

}