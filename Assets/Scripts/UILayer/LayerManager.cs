using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI;

public class LayerManager : MonoBehaviour
{
    private static LayerManager instance;
    public static LayerManager Instance { get { return instance; } }
    private void Awake()
    {
        instance = this;
        select_items = new List<LayerItem>();
    }

    public GameObject layerItemPrefab; // ͼ����Ԥ����
    public GameObject groupItemPrefab; // ����Ԥ����
    public RectTransform content; // �б�����
    /// <summary>
    ///���б���δ���ص�����ͼ������������
    /// </summary>
    public LayerItem[] items;
    /// <summary>
    /// ��ǰѡ�е�����ͼ�㣬������
    /// </summary>
    public List<LayerItem> select_items; //��ѡ

    public int selectedIndex = -1; // ѡ��������
    public int layer_index = 0; // ͼ��������
    public RectTransform selectedObject; // ѡ������

    public bool isSelection;//�Ƿ��ѡ
    public bool ismultiple_selection;//�Ƿ�һ����ѡ
    public bool isGrouping;//�Ƿ���飨�Ƿ������ڣ�
    /// <summary>
    /// ��ǰ�϶�����ʱ����������
    /// </summary>
    public LayerItem selected_group;
    /// <summary>
    /// �½�ͼ��
    /// </summary>
    public void CreateLayer()
    {
        GameObject newItem = Instantiate(layerItemPrefab, content);
        var li = newItem.GetComponent<LayerItem>();

        items = content.GetComponentsInChildren<LayerItem>();
        layer_index += 1;
        selectedIndex = layer_index;
        li.text.text = selectedIndex.ToString();
        selectedObject = newItem.transform as RectTransform;
        SelectItem(li);
        newItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -layer_index * selectedObject.sizeDelta.y);
        Rearrangement();
        ChangeContentHeight(selectedObject.sizeDelta.y);
    }

    public void CG()
    {
        CreateGroup(select_items);
    }

    /// <summary>
    /// ��������ͼ��
    /// </summary>
    /// <param name="select_items"></param>
    /// <param name="ver"></param>
    public void CreateGroup(List<LayerItem> select_items, Vector2 ver = new Vector2())
    {
        // ���������
        GameObject groupItem = Instantiate(groupItemPrefab, content);
        groupItem.name = "Group";
        var rect = groupItem.GetComponent<RectTransform>();
        var li = groupItem.GetComponent<LayerItem>();
        li.Group = true;
        li.text.text = groupItem.name;
        li.SetSelected(true);
        rect.anchoredPosition = ver;

        GameObject group = new GameObject("grop_list");
        group.transform.SetParent(content, false);
        var g_rect = group.GetComponent<RectTransform>();
        if (g_rect == null) g_rect = group.AddComponent<RectTransform>();
        g_rect.anchorMax = new Vector2(0.5f, 1);
        g_rect.anchorMin = new Vector2(0.5f, 1);
        g_rect.sizeDelta = new Vector2(groupItem.GetComponent<RectTransform>().sizeDelta.x, 0);
        li.group_rect = g_rect;
        float height = 0;
        g_rect.anchoredPosition = new Vector2(0, -(ver.y + rect.sizeDelta.y / 2));
        select_items.Sort((x, y) => -x.pos.y.CompareTo(y.pos.y));
        for (int i = 0; i < select_items.Count; i++)
        {
            if (select_items[i].isGrouping) continue; //�����ڵľ��Ȳ�����
            select_items[i].transform.SetParent(g_rect, false);
            var s_rect = select_items[i].GetComponent<RectTransform>();

            height -= s_rect.sizeDelta.y;
            s_rect.anchoredPosition = new Vector2(0, -i * s_rect.sizeDelta.y);
            li.group_item.Add(select_items[i]);
            select_items[i].pos = s_rect.anchoredPosition;

            select_items[i].isGrouping = true;
            select_items[i].group_obj = li;
            select_items[i].SetSelected(false);
        }
        g_rect.sizeDelta = new Vector2(g_rect.sizeDelta.x, -height);

        g_rect.anchoredPosition = new Vector2(0, height / 2);
        groupItem.transform.GetChild(1).gameObject.AddComponent<Toggle>().onValueChanged.AddListener((e) =>
        {
            group.SetActive(e);
            items = content.GetComponentsInChildren<LayerItem>();
            Rearrangement();

        });
        group.SetActive(false);
        items = content.GetComponentsInChildren<LayerItem>();
        Rearrangement();
        ChangeContentHeight(rect.sizeDelta.y);
    }

    /// <summary>
    /// content�߶�����������ɾ����������Ӻͼ���
    /// </summary>
    void ChangeContentHeight(float variation)
    {
        content.sizeDelta += new Vector2(0, variation);
    }

    private void Update()
    {
      
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
        {
            CreateGroup(select_items, selectedObject.anchoredPosition);
        }
    }

    /// <summary>
    /// ѡ��ͼ����
    /// </summary>
    /// <param name="item"></param>
    public void SelectItem(LayerItem item)
    {
        if (!isSelection && !ismultiple_selection) select_items.Clear();
        foreach (var i in items)
        {
            if (i == null || isSelection || ismultiple_selection) continue;
            i.SetSelected(false);
        }
        item.SetSelected(true);
        if (!item.Group)
        {
            if (!select_items.Contains(item))
                select_items.Add(item);
        }
      
        if (ismultiple_selection)
        {
            foreach (var i in items)
            {
                if (i == null) continue;
                i.SetSelected(false);
            }
            int start = Array.IndexOf(items, selectedObject.GetComponent<LayerItem>());
            int end = Array.IndexOf(items, item);
            if (start != -1 && end != -1)
            {
                select_items.Clear();
                items = content.GetComponentsInChildren<LayerItem>();
                for (int i = Mathf.Min(start, end); i <= Mathf.Max(start, end); i++)
                {
                    if (items[i].Group)
                    {

                        foreach (var li in items[i].group_item)
                        {
                            if (!select_items.Contains(li))
                                select_items.Add(li);
                        }
                    }
                    else
                    {
                        if (!select_items.Contains(items[i]))
                            select_items.Add(items[i]);
                    }
                    items[i].SetSelected(true);
                }
            }
        }

        selectedIndex = item.transform.GetSiblingIndex();
        selectedObject = item.transform as RectTransform;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void GroupPermutation(LayerItem group)
    {
        if (group.group_item == null) return;
        var list = group.group_item;
        float height = 0;
        list.Sort((x, y) => -x.pos.y.CompareTo(y.pos.y));
        group.group_rect.sizeDelta = new Vector2(group.group_rect.sizeDelta.x, height);
        for (int i = 0; i < list.Count; i++)
        {
            var s_rect = list[i].GetComponent<RectTransform>();
            height -= s_rect.sizeDelta.y;
            group.group_rect.sizeDelta = new Vector2(group.group_rect.sizeDelta.x, -height);
            s_rect.anchoredPosition = new Vector2(0, -i * s_rect.sizeDelta.y);
            list[i].pos = s_rect.anchoredPosition;
            list[i].transform.SetSiblingIndex(i);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void Rearrangement()
    {
        List<LayerItem> item_list = new List<LayerItem>();

        foreach (var item in items)
        {
            item_list.Add(item);
        }

        item_list.Sort((x, y) => -x.pos.y.CompareTo(y.pos.y));
        float height = 0;
        for (int i = 0; i < item_list.Count; i++)
        {
            if (item_list[i].isGrouping) continue;
            var rect = item_list[i].GetComponent<RectTransform>();
            height -= rect.sizeDelta.y;
            rect.anchoredPosition = new Vector2(0, height+ rect.sizeDelta.y/2);
            if (item_list[i].group_rect != null && item_list[i].group_rect.gameObject.activeSelf)
            {
                item_list[i].group_rect.anchoredPosition = new Vector2(0, (height - rect.sizeDelta.y - item_list[i].group_rect.sizeDelta.y / 2)+ rect.sizeDelta.y/2);
                height -= item_list[i].group_rect.sizeDelta.y;
            }
            item_list[i].transform.SetSiblingIndex(i);
            item_list[i].pos = rect.anchoredPosition;
        }

        //Ϊ�˱�֤�����±���ӽڵ�˳��һһ��Ӧ
        items = content.GetComponentsInChildren<LayerItem>();
        foreach (LayerItem item in items)
        {
            item.gameObject.name = item.transform.GetSiblingIndex().ToString();
        }
    }

    /// <summary>
    /// ��������ѡ�е�ͼ�������
    /// </summary>
    /// <param name="ver"></param>
    public void SetSelectItemPos(Vector2 ver)
    {
        for (int i = 0; i < select_items.Count; i++)
        {
            if (select_items[i].isGrouping) continue;
            select_items[i].GetComponent<RectTransform>().anchoredPosition = ver;
            select_items[i].pos = ver;
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    public void LayerDragInGroup(LayerItem group)
    {
        foreach (var item in select_items)
        {
            if (item == group || item.group_obj == group) continue;
            if (item.isGrouping)
            {
                item.isGrouping = false;
                item.group_obj.group_item.Remove(item);
                GroupPermutation(item.group_obj);
                item.group_obj = null;
            }

            item.transform.SetParent(group.group_rect, false);
            if (!group.group_item.Contains(item)) group.group_item.Add(item);
            item.isGrouping = true;
            item.group_obj = group;
            item.SetSelected(false);
        }
        GroupPermutation(group);
        Rearrangement();
    }

    /// <summary>
    /// �ϳ�
    /// </summary>
    public void LayerDragOutGroup(Vector2 pos, RectTransform mouseP)
    {
        //�ϳ�������պ�������뵽��һ��Ļ���������Ӧ�û�Ҫ���ж�
        foreach (var item in select_items)
        {
            if (item == null || !item.isGrouping) continue;
            //��������λ���ж�
            var li = item.group_obj.GetComponent<RectTransform>();
            var max = li.anchoredPosition.y + li.sizeDelta.y / 2;
            var min = item.group_obj.group_rect.anchoredPosition.y - item.group_obj.group_rect.sizeDelta.y / 2;
            if (!item.isGrouping) continue;
            if (pos.y > max || pos.y < min)
            {
                item.isGrouping = false;
                item.transform.SetParent(item.group_obj.transform.parent);
                item.group_obj.group_item.Remove(item);
                GroupPermutation(item.group_obj);
                item.group_obj = null;
            }
            else
            {
                var ver = GetUIPoint(mouseP.transform.position, item.group_obj.transform as RectTransform);
                //����ת����ʱ������һ���̶�ֵ������߶ȣ��Ĳ�ֵ
                ver.y += (int)item.transform.GetComponent<RectTransform>().sizeDelta.y;
                item.pos = ver;
                GroupPermutation(item.group_obj);
            }
        }
    }

    /// <summary>
    /// �Ƿ������������
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    internal bool WithinTheLimitsOfGroup(Vector2Int ver)
    {
        float min, max;
        foreach (LayerItem item in items)
        {
            if (!item.Group) continue;
            var pos = GetUIPoint(item.transform.position, content as RectTransform);
            float height = item.GetComponent<RectTransform>().sizeDelta.y;
            max = pos.y + height / 3;
            min = pos.y - height / 3;

            if (ver.y < max && ver.y > min)
            {
                selected_group = item;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ����ת��
    /// </summary>
    /// <param name="vector">��Ļ����</param>
    /// <param name="rect">��ui���ڵ�</param>
    /// <returns></returns>
    public Vector2Int GetUIPoint(Vector2 vector, RectTransform rect = null)
    {
        if (rect == null) rect = GameObject.Find("UIMainCanvas").GetComponent<RectTransform>();
        Vector2 screenPos = Camera.main.WorldToScreenPoint(vector);//��������ת��Ļ����
        Vector2 localPos;
        bool p = RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, Camera.main, out localPos);//��Ļ����תUI����
        return new Vector2Int((int)localPos.x, (int)localPos.y);
    }

}
