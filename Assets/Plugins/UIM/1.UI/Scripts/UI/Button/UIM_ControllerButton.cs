using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UIM;
using UIM.Input;

[RequireComponent(typeof(Button))]
public class UIM_ControllerButton : MonoBehaviour
{
    [Header("手柄按键绑定")] 
    public InputActionReference confirmAction;
    public Image buttonIconDisplay; // 用于显示图标的UI Image组件

    [Header("图标设置")]
    public SO_ControllerIcons controllerIcons; // 手柄图标资源
    public bool showIcon = true;

    private Button button;
    private Sprite defaultIcon;
    
    private UIM_InputSystem inputSystem;
    

    private void Awake()
    {
        button = GetComponent<Button>();
        
        if (buttonIconDisplay != null)
        {
            defaultIcon = buttonIconDisplay.sprite;
            
        }
    }

    private void Start()
    {
        inputSystem=UIM_InputSystem.Instance;
        /*if (confirmAction != null)
        {
            //初始化刷新
            OnValidate();
        }*/
    }


    private void OnEnable()
    {
        inputSystem=UIM_InputSystem.Instance;
        if (confirmAction != null)
        {
            confirmAction.action.performed += OnConfirmPerformed;
            /*inputSystem.OnSwitchGamepad.AddListener(delegate { OnValidate();});
            inputSystem.OnSwitchControlMode.AddListener(delegate { OnValidate();});*/
            
            inputSystem.OnSwitchControlDevice.AddListener(delegate { UIM_UIManager.Instance.RefreshLayoutsRecursively();});
            
            
            /*//初始化刷新
            OnValidate();*/
        }
    }

    private void OnDisable()
    {
        if (confirmAction != null)
        {
            confirmAction.action.performed -= OnConfirmPerformed;
            /*inputSystem.OnSwitchGamepad.RemoveListener(delegate { OnValidate();});
            inputSystem.OnSwitchControlMode.RemoveListener(delegate { OnValidate();});*/
            
            inputSystem.OnSwitchControlDevice.RemoveListener(delegate { UIM_UIManager.Instance.RefreshLayoutsRecursively();});
        }
    }

    private void OnConfirmPerformed(InputAction.CallbackContext context)
    {
        if (button == null) return;
        button.onClick.Invoke();
    }

    private void UpdateButtonIcon()
    {
 
        var gamepadMod = inputSystem.currentGamepad;
        var device = inputSystem.currentControlDevice;
        
        /*if (Application.isPlaying) return;*/
        
        if (!showIcon || buttonIconDisplay == null || controllerIcons == null||confirmAction==null) 
            return;
        
        //根据当前模式决定开啊管
        
        if (device==ControlDevice.Mouse)
        {
            buttonIconDisplay.gameObject.SetActive(false);
 
        }
        else
        {
            buttonIconDisplay.gameObject.SetActive(true);
        }
        
        
        
        
        var currentBindings= UIM_InputSystem.Instance.CheckCurrentBindings(confirmAction);

        string controlblandingPath = currentBindings.path;
        
        
        
        
        /*Debug.Log($"<color=blue>当前按键路径为</color> controlblandingPath: {controlblandingPath}");*/
        
        
        
        // 获取对应的图标
        Sprite icon = controllerIcons.GetIconForControl(controlblandingPath);
        
        // 更新显示
        buttonIconDisplay.sprite = icon != null ? icon : defaultIcon;
        

    }


    private void Update()
    {
        UpdateButtonIcon();
    }



}