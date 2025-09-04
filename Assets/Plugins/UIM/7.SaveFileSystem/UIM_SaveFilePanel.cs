using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UIM;

public class UIM_SaveFilePanel : UIM_UnvisersalListPanel
{

    private UIM_SaveFileManger SFM;
    [LabelText("存档跳转场景")]
    public string startSceneName="LoadingScene";


    private void Start()
    {
  
        SFM = UIM_SaveFileManger.Instance;
    }

    public override void OnInitElements()
    {
        base.OnInitElements();
        SFM = UIM_SaveFileManger.Instance;
        for (int i = 0; i < SFM.saveFileDatas.Count; i++)
        {
            int num = i;
            UIM_SaveFileItem item = OnAddElements().GetComponent<UIM_SaveFileItem>();
            item.SetDisplay(num);
    
            item.emptyObj.GetComponent<Button>().onClick.AddListener(delegate { OnItemCreateData(item, num); });
  
            item.createdObj.GetComponent<Button>().onClick.AddListener(delegate { OnItemStartGame(item, num); });
  
            item.deleteBut.onClick.AddListener(delegate { OnItemOpenDeleteBar(item); });
            item._cancelDeleteBut.onClick.AddListener(delegate { OnItemCloseDeleteBar(item); });

            item._confimDeleteBut.onClick.AddListener(delegate { OnItemDeleteData(item, num); });
        }

    }


    public void OnItemStartGame(UIM_SaveFileItem item, int i)
    {
        SFM.currentSaveFileData = SFM.saveFileDatas[i];
        UIM_TransCurtatin.Instance.OnTransSence(startSceneName);

    }



    public void OnItemCreateData(UIM_SaveFileItem item,int i)
    {
        SFM.CreateData(i);
   
        item.SetDisplay(i);
    }

    public void OnItemDeleteData(UIM_SaveFileItem item, int i)
    {
        SFM.DeleteData(i);
        item.SetDisplay(i);
    }

    /// <summary>
    /// 打开删除面板
    /// </summary>
    /// <param name="item"></param>
    public void OnItemOpenDeleteBar(UIM_SaveFileItem item)
    {
        item._confimObj.gameObject.SetActive(true);
        item.createdObj.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(item._cancelDeleteBut.gameObject);

        UIM_UIManager.Instance.RefreshLayoutsRecursively();
    }

    /// <summary>
    /// 关闭删除面板
    /// </summary>
    /// <param name="item"></param>
    public void OnItemCloseDeleteBar(UIM_SaveFileItem item)
    {
        item._confimObj.gameObject.SetActive(false);
        item.createdObj.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(item.deleteBut.gameObject);

        UIM_UIManager.Instance.RefreshLayoutsRecursively();

    }




}

