// 音效数据提供接口（可替换数据源）
using UnityEngine;

public interface IAudioDataProvider
{
    AudioClip GetAudioClip(string key);
}

