using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
using System.Drawing;
using UnityEngine.SceneManagement;

public class UIM_ButTransSence : MonoBehaviour
{

    public string sencesName;

    private void Start()
    {

        if (UIM_TransCurtatin.Instance)
        {
            GetComponent<Button>().onClick.AddListener(delegate { UIM_TransCurtatin.Instance.OnTransSence(sencesName); });
        }
        else
        {
            GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene(sencesName);});
        }
  


    }





}
