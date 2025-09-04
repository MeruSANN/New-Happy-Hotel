using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class EffectUniversalSwitch : MonoBehaviour

{

    private ParticleSystem particle;



    void Start()

    {

        particle = GetComponent<ParticleSystem>();

    }



    // Update is called once per frame

    void Update()

    {



        if (!particle.isPlaying)

        {

            gameObject.SetActive(false); // 特效播放完毕后禁用该对象

        }

    }

}

