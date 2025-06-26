using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class SnapScroller : MonoBehaviour, IEndDragHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform snapPoint;
    
    [SerializeField] private float snapDuration = 0.25f;
    
    public List<RectTransform> elements = new List<RectTransform>();

    public void AddElement(RectTransform element)
    {
        elements.Add(element);
        
        if (element.gameObject.TryGetComponent(out Button button))
        {
            button.onClick.AddListener(SnapToNearest);
        }
    }

    public void RemoveElement(RectTransform element)
    {
        elements.Remove(element);
        
        if (element.gameObject.TryGetComponent(out Button button))
        {
            button.onClick.RemoveListener(SnapToNearest);
        }
    }
    
    public void SnapToNearest()
    {
        Vector3 centerPos = snapPoint.TransformPoint(snapPoint.rect.center);

        float minDistance = float.MaxValue;
        RectTransform nearest = null;
        
        foreach (var element in elements)
        {
            float dist = Vector3.Distance(element.position, centerPos);
            
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = element;
            }
        }
        
        if (nearest == null) return;
        
        float targetX = -nearest.anchoredPosition.x; 
        Vector2 targetPos = new Vector2(targetX, content.anchoredPosition.y);
        
        content.DOAnchorPos(targetPos, snapDuration).SetEase(Ease.OutBack);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SnapToNearest();
    }
}
