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
    
    [SerializeField] private List<RectTransform> elements = new List<RectTransform>();

    
    public void AddElement(RectTransform element)
    {
        elements.Add(element);
        
        if (element.gameObject.TryGetComponent(out Button button))
        {
            button.onClick.AddListener(()=>SetToSnapPoint(element));
        }
    }

    public void RemoveElement(RectTransform element)
    {
        elements.Remove(element);
        
        if (element.gameObject.TryGetComponent(out Button button))
        {
            button.onClick.RemoveListener(()=>SetToSnapPoint(element));
        }
    }

    public void SetToSnapPoint(RectTransform element)
    {
        // RectTransform rect = elements.Find(x => x == element);
        //
        // if (rect == null)
        // {
        //     Debug.LogError("Element not found");
        //     return;
        // }
        
        Vector2 targetPos = GetTargetPositionForContent(element);
        Debug.Log("Click: " + targetPos);

        content.anchoredPosition = targetPos;
        //content.DOAnchorPos(targetPos, snapDuration).SetEase(Ease.OutBack);
    }
    
    private void SnapToNearest()
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
        if (nearest.gameObject.TryGetComponent(out IScrollingSelectable selectable))
        {
            selectable.Select();
        }
        
        Vector2 targetPos = GetTargetPositionForContent(nearest);

        Debug.Log("Drag: " + targetPos);
        content.DOAnchorPos(targetPos, snapDuration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                content.anchoredPosition = targetPos; // 위치 보정
            });;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SnapToNearest();
    }

    private Vector2 GetTargetPositionForContent(RectTransform element)
    {
        Vector3 centerPos = snapPoint.position;
        centerPos.y = content.position.y;
        
        Vector3 deltaWorld = centerPos - element.position; 
        Vector3 targetWorld = content.position + deltaWorld;
        Vector2 targetPos = content.InverseTransformPoint(targetWorld);
        
        return targetPos;
    }
    
}
