using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Sirenix.OdinInspector;

public class GameUIManager : Singleton<GameUIManager>
{

    public TMP_Text scoreText;

    [LabelText("死亡面板")]
    public GamePanel overPanel;

    [LabelText("通关面板")]
    public GamePanel passPanel;

    private GameDatas GD;

    private void Awake()
    {
    }

    private void Start()
    {
        GD = GameDatas.Instance;

    }

    private void Update()
    {
    }

    public  void OnUpdateFrame()
    {

    }



    private void OnEnable()
    {
        GameDatas.OnGameOver += ShowDeadPanel;
        GameDatas.OnGamePass += ShowPassPanel;
    }
    private void OnDisable()
    {
        GameDatas.OnGameOver -= ShowDeadPanel;
        GameDatas.OnGamePass -= ShowPassPanel;
    }


    void ShowDeadPanel()
    {
        overPanel.gameObject.SetActive(true);
    }

    void ShowPassPanel()
    {
        passPanel.gameObject.SetActive(true);
    }


    public void ReStartGame()
    {
        // 获取当前场景的名称
        string currentSceneName = SceneManager.GetActiveScene().name;
        Time.timeScale = 1f;
        // 重新加载当前场景
        SceneManager.LoadScene(currentSceneName);
    }

    void OnTimeChange(float f)
    {
        Time.timeScale = Time.timeScale * f;
    }


}
