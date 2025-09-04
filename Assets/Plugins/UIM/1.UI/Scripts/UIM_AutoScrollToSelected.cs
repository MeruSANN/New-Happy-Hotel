using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/*using UnityEngine.Localization.SmartFormat.Core.Parsing;*/
using Unity.Mathematics;
using UnityEngine.Serialization;
using UIM.Input;

[AddComponentMenu("UIMaster/Scroll/AutoScroll")]
public class UIM_AutoScrollToSelected : MonoBehaviour
{

    private ScrollRect _scrollRect;
    private Scrollbar _scrollbar;
    private RectTransform _content;

    [FormerlySerializedAs("_speed")] 
    public float speed = 100f;
    [FormerlySerializedAs("_blood")] [Range(0f,1f)]
    public float blood = 0.3f;


    //嫌时间麻烦就没用事件委托了
    /*private bool isCanMove;*/

    private UIM_InputSystem _inputSystem=> UIM_InputSystem.Instance;
    public void Start()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _scrollbar = _scrollRect.verticalScrollbar;
        _content = _scrollRect.content;

        /*_inputSystem.OnSwitchControlDevice += delegate { SetisCanMove(_inputSystem.currentControlDevice);};*/


    }

    public void OnDisable()
    {
        //_inputSystem.OnSwitchControlDevice -= delegate { SetisCanMove(_inputSystem.currentControlDevice);};
    }
    

    private bool CheackCanMove()
    {
        var device = _inputSystem.currentControlDevice;
        return (device==ControlDevice.Gamepad||device==ControlDevice.Keyboard);
    }


    private void Update()
    {
        //模式也得考虑
        
        
        if (EventSystem.current.currentSelectedGameObject != null &&
        EventSystem.current.currentSelectedGameObject.transform.IsChildOf(_scrollRect.content))
        {
            OnMoveContent();
        }
    }

    public void OnMoveContent()
    {
        if(!CheackCanMove())return;
        
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            RectTransform rt = selected.GetComponent<RectTransform>();
            if (rt != null)
            {

                Vector2 screenPos = GetUIScreenPosition(rt);


                float screenHeight = Screen.height;

                //Debug.Log("选中对象屏幕坐标: " + screenPos + "当前屏幕高度: " + screenHeight);

                var hight = screenPos.y;
                //弹性系数，偏差越大，速度系数越大



                //暴力判断
                if (hight >= screenHeight * (1 - blood))
                {
                    // 向下滚动 Content
                    var d = (hight - screenHeight * (1 - blood)) / (blood * hight);
                    _content.anchoredPosition += Vector2.down * speed * 100f *GetAcceleration(d) * Time.deltaTime;
                }
                else if (hight <= screenHeight * blood)
                {
                    var d = (screenHeight * blood - hight) / (blood * hight);
                    _content.anchoredPosition += Vector2.up * speed * 100f * GetAcceleration(d)* Time.deltaTime;
                }
                else
                {
                    //Debug.Log("处于未出血区域");
                }


            }
        }


    }


    public float GetAcceleration(float deviation)
    {
        return math.lerp(1f, 2f,deviation);
       

    }



    public static Vector2 GetUIScreenPosition(RectTransform target)
    {
        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, target.position);
        return screenPos;
    }

}
