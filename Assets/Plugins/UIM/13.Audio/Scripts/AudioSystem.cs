using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 音效系统核心逻辑层，只处理播放逻辑
/// </summary>
public sealed class AudioSystem : IAudioDataProvider
{
    private readonly AudioSource _sfxSource;
    private readonly AudioSource _bgmSource;
    private readonly SO_AudioData _audioData;
    
    
    // 淡入淡出相关参数
    private float _fadeDuration = 1.0f; // 淡入淡出持续时间(秒)
    private bool _isFading = false;
    private float _originalBGMVolume;

    public AudioSystem(AudioSource sfxSource, AudioSource bgmSource, SO_AudioData audioData)
    {
        _sfxSource = sfxSource;
        _bgmSource = bgmSource;
        _audioData = audioData;
        
        // 保存原始音量以便淡出后恢复
        if (_bgmSource != null)
        {
            _originalBGMVolume = _bgmSource.volume;
        }
        
    }

    // 实现数据接口
    /// <summary>
    /// 根据key查找到对应的audioclip返回
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public AudioClip GetAudioClip(string key)
    {
        foreach (var item in _audioData.audioDataList)
        {
            if (item.key == key) return item.audioClip;
        }
        return null;
    }

    // 播放音效
    public void PlaySFX(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        var clip = GetAudioClip(key);
        if (clip == null) {

            Debug.LogWarning("没有找到对应音效的片段");
        }
        else
        {
            if (_sfxSource != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("播放器为空");
            }
        

        }
       
    }


    #region 播放背景音乐

    
    // 播放背景音乐(带淡入淡出效果)
    public async UniTask PlayBGM(string key, bool withFade = true)
    {
        if (string.IsNullOrEmpty(key) || _bgmSource == null || _isFading) return;
        
        var clip = GetAudioClip(key);
        if (clip == null)
        {
            Debug.LogWarning("没有找到对应音乐的片段");
            return;
        }

        _isFading = true;
        
        try
        {
            // 如果正在播放其他BGM，先淡出
            if (_bgmSource.isPlaying && withFade)
            {
                await FadeOutBGM();
            }
            else
            {
                _bgmSource.Stop();
            }

            // 设置新clip并播放
            _bgmSource.clip = clip;
            _bgmSource.loop = true; // 通常BGM需要循环
            _bgmSource.Play();

            // 淡入新BGM
            if (withFade)
            {
                await FadeInBGM();
            }
        }
        finally
        {
            _isFading = false;
        }
    }

    // BGM淡出
    private async UniTask FadeOutBGM()
    {
        float startVolume = _bgmSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / _fadeDuration);
            await UniTask.Yield();
        }

        _bgmSource.Stop();
        _bgmSource.volume = _originalBGMVolume; // 恢复原始音量
    }

    // BGM淡入
    private async UniTask FadeInBGM()
    {
        _bgmSource.volume = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(0f, _originalBGMVolume, elapsedTime / _fadeDuration);
            await UniTask.Yield();
        }

        _bgmSource.volume = _originalBGMVolume;
    }

    // 设置淡入淡出持续时间
    public void SetFadeDuration(float duration)
    {
        _fadeDuration = Mathf.Max(0.1f, duration);
    }

    #endregion
    
    
    /*// 播放背景音乐
    public void PlayBGM(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        var clip = GetAudioClip(key);
        if (clip == null) {

            Debug.LogWarning("没有找到对应音乐的片段");
        }
        else
        {
            if (_bgmSource != null)
            {
                _bgmSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("播放器为空");
            }
        

        }
       
    }*/
    
    
    
    

    // 异步播放（适用于需要等待的场景）
    public async UniTask PlaySFXAsync(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        var clip = GetAudioClip(key);
        if (clip == null) return;

        _sfxSource.PlayOneShot(clip);
        await UniTask.Delay((int)(clip.length * 1000));
    }
}