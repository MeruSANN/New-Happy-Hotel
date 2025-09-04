using System;

namespace HappyHotel.Core.Singleton
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ManagedSingletonAttribute : Attribute
    {
        public ManagedSingletonAttribute(bool dontDestroyOnLoad = false)
        {
            DontDestroyOnLoad = dontDestroyOnLoad;
            IncludeScenes = null;
            ExcludeScenes = null;
            LoadMode = SceneLoadMode.All;
        }

        // 指定只在特定场景加载
        public ManagedSingletonAttribute(SceneLoadMode loadMode, params string[] scenes)
        {
            DontDestroyOnLoad = false;
            LoadMode = loadMode;

            if (loadMode == SceneLoadMode.Include)
            {
                IncludeScenes = scenes;
                ExcludeScenes = null;
            }
            else if (loadMode == SceneLoadMode.Exclude)
            {
                ExcludeScenes = scenes;
                IncludeScenes = null;
            }
        }

        // 指定场景加载模式和DontDestroyOnLoad设置
        public ManagedSingletonAttribute(SceneLoadMode loadMode, bool dontDestroyOnLoad, params string[] scenes)
        {
            DontDestroyOnLoad = dontDestroyOnLoad;
            LoadMode = loadMode;

            if (loadMode == SceneLoadMode.Include)
            {
                IncludeScenes = scenes;
                ExcludeScenes = null;
            }
            else if (loadMode == SceneLoadMode.Exclude)
            {
                ExcludeScenes = scenes;
                IncludeScenes = null;
            }
        }

        public bool DontDestroyOnLoad { get; }
        public string[] IncludeScenes { get; }
        public string[] ExcludeScenes { get; }
        public SceneLoadMode LoadMode { get; }
    }

    public enum SceneLoadMode
    {
        All, // 在所有场景加载（默认行为）
        Include, // 只在指定场景加载
        Exclude // 在除指定场景外的所有场景加载
    }
}