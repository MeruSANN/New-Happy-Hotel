using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.Utils
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ForceLayoutRebuildOnAwake : MonoBehaviour
    {
        [SerializeField] private int extraRebuildFrames = 2; // 额外等待的帧数
        [SerializeField] private bool rebuildOnEnable = true; // 物体启用时重建
        [SerializeField] private bool rebuildOnStart; // Start 再重建一次
        [SerializeField] private bool rebuildOnResize = true; // 尺寸变化时重建
        [SerializeField] private RectTransform explicitRoot; // 指定重建起点（可选）
        private bool queued;

        private RectTransform selfRect;

        private void Awake()
        {
            selfRect = transform as RectTransform;
        }

        private void Start()
        {
            if (rebuildOnStart) QueueRebuild();
        }

        private void OnEnable()
        {
            if (rebuildOnEnable) QueueRebuild();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled) return;
            if (rebuildOnResize) QueueRebuild();
        }

        public void QueueRebuild()
        {
            if (queued) return;
            queued = true;
            StartCoroutine(RebuildRoutine());
        }

        private IEnumerator RebuildRoutine()
        {
            queued = false;

            var root = GetRebuildRoot();
            if (root == null) yield break;

            Canvas.ForceUpdateCanvases();
            RebuildFromTopLayout(root);

            yield return null;
            Canvas.ForceUpdateCanvases();
            RebuildFromTopLayout(root);

            for (var i = 0; i < extraRebuildFrames; i++)
            {
                yield return null;
                Canvas.ForceUpdateCanvases();
                RebuildFromTopLayout(root);
            }

            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            RebuildFromTopLayout(root);
        }

        private RectTransform GetRebuildRoot()
        {
            if (explicitRoot) return explicitRoot;
            if (selfRect == null) selfRect = transform as RectTransform;

            var t = selfRect != null ? selfRect.transform : transform;
            RectTransform lastRect = null;
            RectTransform lastWithLayout = null;

            while (t != null)
            {
                var rt = t as RectTransform;
                if (rt != null) lastRect = rt;
                if (t.GetComponent<LayoutGroup>() != null) lastWithLayout = rt;
                if (t.GetComponent<Canvas>() != null) break;
                t = t.parent;
            }

            return lastWithLayout != null ? lastWithLayout : lastRect;
        }

        private static void RebuildFromTopLayout(RectTransform target)
        {
            if (target == null) return;

            Transform t = target;
            var top = target;

            while (t != null)
            {
                var rt = t as RectTransform;
                if (rt != null && t.GetComponent<LayoutGroup>() != null) top = rt;
                if (t.GetComponent<Canvas>() != null) break;
                t = t.parent;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(top);
        }
    }
}

// 在启用/尺寸变化时更稳健地强制重建布局