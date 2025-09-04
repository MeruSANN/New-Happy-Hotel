using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UIM.Input;


[CreateAssetMenu(fileName = "ControllerIcons", menuName = "UI/Controller Icons")]
public class SO_ControllerIcons : ScriptableObject
{
    [System.Serializable]
    public class IconMapping
    {
        public string controlPath; // 如 "<Gamepad>/buttonSouth"
        [FormerlySerializedAs("icon")] [PreviewField]
        public Sprite defaultIcon;
        
        [PreviewField]
        public Sprite xboxIcon;
        [PreviewField]
        public Sprite ps4Icon;
        
        
        
    }

    [SerializeField]
    public IconMapping[] icons;

    public Sprite GetIconForControl(string controlPath)
    {


        Sprite icon = null;
        
        //遍历所有图标
        foreach (var mapping in icons)
        {
            //如果有路径相符合的
            if (mapping.controlPath == controlPath)
            {
                //更加现在的设备来看，先区分手柄还是键盘，在区分哪个手柄
                
                var gamepadmod = UIM_InputSystem.Instance.currentGamepad;
                var gamedevice = UIM_InputSystem.Instance.currentControlDevice;


                if (gamedevice==ControlDevice.Keyboard)
                {
                    icon = mapping.defaultIcon;
                    
                }else if (gamedevice == ControlDevice.Gamepad)
                {
                    
                    switch (gamepadmod)
                    {
                        case GamepadMod.NullGamepad:
                            icon = mapping.defaultIcon;
                            break;
                        case GamepadMod.Other:
                            icon = mapping.defaultIcon;
                            break;
                        case GamepadMod.PS4:
                            icon=mapping.ps4Icon;
                            break;
                        case GamepadMod.Xbox:
                            icon=mapping.xboxIcon;
                            break;

                    }
                    
                    
                }
                
                return icon;
            }
               
        }


        
        
        
        return icon;
    }
}