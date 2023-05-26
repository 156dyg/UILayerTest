using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

[System.Serializable]
public class LayerItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public GameObject selectedBackground; // 选中背景
    public Text text;
    public bool isSelected; // 是否选中
    public Vector2 pos;
    public RectTransform mouseP;

    public bool Group;//组
    public RectTransform group_rect;//组对象
    public List<LayerItem> group_item = new List<LayerItem>();//组内成员

    public bool isGrouping;//是否在组内
    public LayerItem group_obj;//在组内的话组内对象是啥

    public void OnPointerClick(PointerEventData eventData)
    {

        //每次选一部分
        if (Input.GetKey(KeyCode.LeftShift))
        {
            LayerManager.Instance.ismultiple_selection = true;
            LayerManager.Instance.isSelection = false;
        }
        //每次选一个
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            LayerManager.Instance.isSelection = true;
            LayerManager.Instance.ismultiple_selection = false;
            if (isSelected)
            {
                SetSelected(false);
                LayerManager.Instance.select_items.Remove(this);
                return;
            }

        }
        else
        {
            LayerManager.Instance.ismultiple_selection = false;
            LayerManager.Instance.isSelection = false;
        }

        LayerManager.Instance.SelectItem(this);

    }

    // 选中状态改变
    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        selectedBackground.SetActive(isSelected);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GameObject obj = new GameObject("mousP");
        obj.transform.SetParent(LayerManager.Instance.content);
        obj.AddComponent<Image>().color = Color.blue;
        mouseP = obj.transform as RectTransform;
        mouseP.anchorMax = new Vector2(0.5f, 1);
        mouseP.anchorMin = new Vector2(0.5f, 1);
        mouseP.localScale = Vector3.one;
        mouseP.sizeDelta = new Vector2(transform.GetComponent<RectTransform>().sizeDelta.x, 5);
    }
    bool inGroup = false;
    public void OnDrag(PointerEventData eventData)
    {
        var ver = LayerManager.Instance.GetUIPoint(Input.mousePosition, LayerManager.Instance.content);
        mouseP.anchoredPosition = new Vector2(0, ver.y);
        //判断当前坐标是否在Group上的某个范围
        inGroup = LayerManager.Instance.WithinTheLimitsOfGroup(ver);
        if (inGroup)
        {
            mouseP.GetComponent<Image>().color = Color.red;
        }
        else
        {
            mouseP.GetComponent<Image>().color = Color.blue;
            LayerManager.Instance.selected_group = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inGroup)
        {
            LayerManager.Instance.LayerDragInGroup(LayerManager.Instance.selected_group);
        }
        else
        {
            pos = mouseP.anchoredPosition;
            //当前逻辑肯能要在manager中实现才好
            LayerManager.Instance.LayerDragOutGroup(pos, mouseP);
        }

        Destroy(mouseP.gameObject);

        LayerManager.Instance.SetSelectItemPos(pos);
        LayerManager.Instance.Rearrangement();
    }
}
