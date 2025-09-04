using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{

    public Button butRestart;
    //public TMP_Text textDeadDay;
    //public TMP_Text textHighDay;

    private GameUIManager GUM;

    private void Start()
    {
        gameObject.SetActive(false);
        GUM = GameUIManager.Instance;
        butRestart.onClick.AddListener(GUM.ReStartGame);
    }

    private void OnEnable()
    {
        //textDeadDay.text = string.Format("在位天数:{0} \n", GameDatas.instance.turnTime);
        //textHighDay.text = "最长在位天数：" + GameDatas.instance.highScore;

    }

}
