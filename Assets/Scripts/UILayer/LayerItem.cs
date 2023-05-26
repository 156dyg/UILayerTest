using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

[System.Serializable]
public class LayerItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public GameObject selectedBackground; // ѡ�б���
    public Text text;
    public bool isSelected; // �Ƿ�ѡ��
    public Vector2 pos;
    public RectTransform mouseP;

    public bool Group;//��
    public RectTransform group_rect;//�����
    public List<LayerItem> group_item = new List<LayerItem>();//���ڳ�Ա

    public bool isGrouping;//�Ƿ�������
    public LayerItem group_obj;//�����ڵĻ����ڶ�����ɶ

    public void OnPointerClick(PointerEventData eventData)
    {

        //ÿ��ѡһ����
        if (Input.GetKey(KeyCode.LeftShift))
        {
            LayerManager.Instance.ismultiple_selection = true;
            LayerManager.Instance.isSelection = false;
        }
        //ÿ��ѡһ��
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

    // ѡ��״̬�ı�
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
        //�жϵ�ǰ�����Ƿ���Group�ϵ�ĳ����Χ
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
            //��ǰ�߼�����Ҫ��manager��ʵ�ֲź�
            LayerManager.Instance.LayerDragOutGroup(pos, mouseP);
        }

        Destroy(mouseP.gameObject);

        LayerManager.Instance.SetSelectItemPos(pos);
        LayerManager.Instance.Rearrangement();
    }
}
