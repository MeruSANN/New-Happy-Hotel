using UnityEngine;

public class DestroyParticleSystem : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // 如果粒子系统不在播放，则销毁该对象
        if (!particleSystem.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}