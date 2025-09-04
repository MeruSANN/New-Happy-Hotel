using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UIM;

public class UIM_PausePanel : UIM_PanelManager
{
    public static UIM_PausePanel Instance;
    
    
    public Button continuekBut, restartBut, quitBut;


    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        
    }

    private void Start()
    {
        CheckBlur();
        if (continuekBut)
        {
            continuekBut.onClick.AddListener( UIM_PauseManager.Instance.OnReback);
        }

        if (restartBut)
        {
            restartBut.onClick.AddListener(UIM_PauseManager.Instance.OnRestart);
        }

        if (quitBut)
        {
            quitBut.onClick.AddListener(delegate { UIM_UIManager.Instance.StartQuit(); });
        }
    }

    public async void CheckBlur()
    {
        await Task.Yield();
        
        if (GetComponent<Image>()){

            if (!BlurRenderer.Instance)
            {
                GetComponent<Image>().material = null;
            }
            else
            {
                GetComponent<Image>().material = BlurRenderer.Instance.CurrentMaterial;

            }

        }

    }


    public void  OnBlurFadeIn()
    {
        if (BlurRenderer.Instance)
        {
            StartCoroutine(BlurRenderer.Instance.AnimateColor(true));
        }


    }

    public void OnBlurFadeOut()
    {
        if (BlurRenderer.Instance)
        {
            StartCoroutine(BlurRenderer.Instance.AnimateColor(false));
        }


    }

    public void OnBlurReset()
    {
        if (BlurRenderer.Instance)
        {
            BlurRenderer.Instance.ResetBlur();
        }



    }


}