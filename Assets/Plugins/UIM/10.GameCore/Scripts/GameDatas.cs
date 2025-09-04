using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;



public class GameDatas : Singleton<GameDatas>
{

    //[LabelText("存活天数")]
    public int turnTime;
    public bool isGameOver;

    public float scoreTotal;
    public float scoreCurrent;
    public float scoreTarget;
    //[LabelText("最高分")]
    public float highScore;


    private GameUIManager GUI;


    public bool gameTest;


    private void Awake()
    {

    }

    private void Start()
    {
        GUI = GameUIManager.Instance;

    }


    private void Update()
    {
        if (gameTest)
        {
            OnGameTest();
        }


    }


    // 定义一个事件，用于监听游戏结束
    public static event Action OnGameOver;
    public static event Action OnGamePass;
    public void GameOver(bool ispass=false)
    {

        isGameOver = true;
        Time.timeScale = 0;
        if (!ispass)
        {
            OnGameOver?.Invoke();
        }
        else
        {
            OnGamePass?.Invoke();
        }
   
        // 检查当前分数是否是最高分
        //var score = scoreCurrent + scoreTotal;
        var score = turnTime;
        if (score > highScore)
            // 如果是最高分，更新最高分并保存到 PlayerPrefs
            highScore = score;
        PlayerPrefs.SetFloat("HighScore", highScore);
        PlayerPrefs.Save();

        // 保存修改到磁盘
        //Debug.Log("New High Score: " + highScore);
    }

    void OnGameTest()
    {
        if (Input.GetKey(KeyCode.F7))
        {
            GameOver(false);
        }
        if (Input.GetKey(KeyCode.F8))
        {
            GameOver(true);
        }
    }



}
