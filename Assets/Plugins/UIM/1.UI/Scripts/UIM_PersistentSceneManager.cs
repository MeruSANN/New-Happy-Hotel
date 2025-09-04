using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIM_PersistentSceneManager : MonoBehaviour
{
    public static UIM_PersistentSceneManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保切换场景时管理器不会被销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }








}
