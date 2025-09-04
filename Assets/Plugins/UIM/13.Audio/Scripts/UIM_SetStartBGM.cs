using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIM_SetStartBGM : MonoBehaviour
{

    [LabelText("背景音乐key")]
    public string BGMkey;
    
    void Start()
    {
        AudioFunclib.PlayBGM(BGMkey);
        
    }


}
