using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.Controls;

using UIM.Input;

public class InputViewsMenu : MonoBehaviour
{
    private PlayerInput playerInput;
    public TMP_Text inputText;

    public GameObject ps4Controller;
    public GameObject xboxController;
    
    
    private UIM_InputSystem inputSystem;
    


    private void Start()
    {
        inputSystem = UIM_InputSystem.Instance;
        playerInput = inputSystem.GetComponent<PlayerInput>();
        playerInput.onControlsChanged += switchGameUI;

        //设备改变后监听

    }

    void switchGameUI(PlayerInput playerInput)
    {

        xboxController.SetActive(false);
        ps4Controller.SetActive(false);
        
        //查找
        var mod = inputSystem.currentGamepad;
        switch (mod)
        {
            case GamepadMod.PS4 :
                ps4Controller.SetActive(true);
                break;
            case GamepadMod.Xbox:
                xboxController.SetActive(true);
                break;
        }


    }
    private void Update()
    {
      
      
        var str = "";
        str += "\n\n<color=yellow>ActionMap</color>\n" + playerInput.currentActionMap;
        str += "\n\n<color=yellow>ControlScheme</color>\n" + playerInput.currentControlScheme;
        str += "\n\n<color=yellow>GamepadCount</color>\n" + Gamepad.all.Count;

        inputText.text = str;
    }


    
    private Gamepad gamepad;

    private void OnEnable()
    {

        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {

        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {

        if (change == InputDeviceChange.Added && device is Gamepad)
        {
            gamepad = (Gamepad)device;


        }

        else if (change == InputDeviceChange.Removed && device == gamepad)
        {

            gamepad = null;
        }
    }

    private void OnButtonPressed(ButtonControl button)
    {

        Debug.Log("Pressed Button: " + button.displayName);
    }



}
