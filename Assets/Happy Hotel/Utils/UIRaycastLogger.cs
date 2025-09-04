using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HappyHotel.Utils
{
    // 实时输出鼠标位置下的UI对象名称
    public class UIRaycastLogger : MonoBehaviour
    {
        [Header("日志设置")] [SerializeField] private bool logOnlyWhenChanged = true; // 仅在目标变化时输出

        [SerializeField] private bool logAllRaycastResults = true; // 输出命中的所有对象

        private string lastLogSignature = "";
        private bool warnedNoEventSystem;

        private void Update()
        {
            if (EventSystem.current == null)
            {
                if (!warnedNoEventSystem)
                {
                    Debug.LogWarning("UIRaycastLogger: 未找到EventSystem，无法进行UI射线检测");
                    warnedNoEventSystem = true;
                }

                return;
            }

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            var signature = BuildSignature(raycastResults);

            if (!logOnlyWhenChanged || signature != lastLogSignature)
            {
                Debug.Log(signature);
                lastLogSignature = signature;
            }
        }

        private string BuildSignature(List<RaycastResult> results)
        {
            if (results == null || results.Count == 0) return "UI Hover: <none>";

            if (!logAllRaycastResults)
            {
                var top = results[0].gameObject;
                return $"UI Hover: {top.name}";
            }

            var builder = new StringBuilder();
            builder.Append("UI Hover: [");
            for (var i = 0; i < results.Count; i++)
            {
                builder.Append(results[i].gameObject != null ? results[i].gameObject.name : "null");
                if (i < results.Count - 1) builder.Append(", ");
            }

            builder.Append("]");
            return builder.ToString();
        }
    }
}