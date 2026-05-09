using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 热区数据结构
/// </summary>
[System.Serializable]
public class Hotspot
{
    public string name = "新热区";

    [Header("UV区域 (0~1)")]
    [Tooltip("热区的左下角UV坐标")]
    public Vector2 min = new Vector2(0.1f, 0.1f);
    [Tooltip("热区的右上角UV坐标")]
    public Vector2 max = new Vector2(0.2f, 0.2f);

    [Space]
    public UnityEvent onClick;
}


/// <summary>
/// 功能描述：提供一个在UI图形（Image/RawImage）上定义可视化热区并响应点击事件的系统。
/// 用户可以通过UV坐标定义多个矩形区域，当点击发生在这些区域内时，触发相应的UnityEvent。
/// </summary>
/// <remarks>
/// 注意：
/// 1. 此组件需要挂载在拥有Canvas的UI对象层级下。
/// 2. Canvas上必须有 GraphicRaycaster 组件才能接收到点击事件。
/// 3. 必须在 "Target Graphic" 字段中指定一个 Image 或 RawImage 组件。
/// 4. 组件自身的 aaciveAndEnabled 状态会控制点击检测和Gizmo的显示。
/// 5. 热区列表的响应顺序是从后往前（列表中越靠下的热区，层级越高，优先被检测）。
/// 6. 能够正确处理Sprite Atlas中Sprite的UV偏移问题。
/// </remarks>
/// <dependencies>
/// 依赖：UnityEngine.UI, UnityEngine.EventSystems
/// 需要 Image 或 RawImage 组件作为点击目标。
/// 实现 IPointerClickHandler 接口。
/// </dependencies>

[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Active)]
[AddComponentMenu("UI/Visual Hotspot System")]
[HelpURL("https://github.com/XiaoHer001/Visual-Hotspot-System")]

public class VisualHotspotSystem : MonoBehaviour, IPointerClickHandler
{
    [Header("目标UI图形")]
    [Tooltip("指定要检测点击的Image或RawImage组件")]
    public MaskableGraphic targetGraphic;

    [Header("热区列表")]
    public List<Hotspot> hotspots = new List<Hotspot>();

    // 内部缓存
    private RectTransform _rectTransform;

    private void Awake()
    {
        if (targetGraphic != null)
        {
            _rectTransform = targetGraphic.rectTransform;
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
    }

    /// <summary>
    /// IPointerClickHandler接口实现
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        if (!IsComponentValid()) return;

        Vector2 uv = ScreenToUV(eventData);
        HandleHotspotClick(uv);
    }

    /// <summary>
    /// 将屏幕坐标转换为目标图形的UV坐标(0-1范围)
    /// 完美兼容Image和RawImage
    /// </summary>
    private Vector2 ScreenToUV(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        Vector2 size = _rectTransform.rect.size;
        Vector2 pivot = _rectTransform.pivot;
        float localU = (localPoint.x + pivot.x * size.x) / size.x;
        float localV = (localPoint.y + pivot.y * size.y) / size.y;
        var localUV = new Vector2(localU, localV);

        Image image = targetGraphic as Image;
        if (image != null && image.sprite != null)
        {
            var spriteUVs = image.sprite.uv;
            if (spriteUVs.Length == 4)
            {
                Vector2 minSpriteUV = new Vector2(Mathf.Min(spriteUVs[0].x, spriteUVs[1].x, spriteUVs[2].x, spriteUVs[3].x),
                                                  Mathf.Min(spriteUVs[0].y, spriteUVs[1].y, spriteUVs[2].y, spriteUVs[3].y));
                Vector2 maxSpriteUV = new Vector2(Mathf.Max(spriteUVs[0].x, spriteUVs[1].x, spriteUVs[2].x, spriteUVs[3].x),
                                                  Mathf.Max(spriteUVs[0].y, spriteUVs[1].y, spriteUVs[2].y, spriteUVs[3].y));

                float finalU = Mathf.Lerp(minSpriteUV.x, maxSpriteUV.x, localUV.x);
                float finalV = Mathf.Lerp(minSpriteUV.y, maxSpriteUV.y, localUV.y);
                return new Vector2(finalU, finalV);
            }
        }

        return localUV;
    }

    /// <summary>
    /// 热区点击检测
    /// 从后往前遍历，上层热区优先响应
    /// </summary>
    private void HandleHotspotClick(Vector2 uv)
    {
        for (int i = hotspots.Count - 1; i >= 0; i--)
        {
            var spot = hotspots[i];

            Vector2 min = new Vector2(Mathf.Min(spot.min.x, spot.max.x), Mathf.Min(spot.min.y, spot.max.y));
            Vector2 max = new Vector2(Mathf.Max(spot.min.x, spot.max.x), Mathf.Max(spot.min.y, spot.max.y));

            if (uv.x >= min.x && uv.x <= max.x &&
                uv.y >= min.y && uv.y <= max.y)
            {
                Debug.Log($"[VisualHotspotSystem] 点击命中热区: {spot.name}");
                spot.onClick?.Invoke();
                return;
            }
        }
        Debug.Log("[VisualHotspotSystem] 未命中任何热区。");
    }

    private bool IsComponentValid()
    {
        if (targetGraphic == null)
        {
            Debug.LogError("[VisualHotspotSystem] 错误: Target Graphic 未设置!", this);
            return false;
        }
        if (_rectTransform == null)
        {
            _rectTransform = targetGraphic.rectTransform;
        }
        return true;
    }

    // ======================
    // 编辑器专用功能
    // ======================
#if UNITY_EDITOR
    private readonly Vector3[] _worldCorners = new Vector3[4];

    private void OnValidate()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Image>() ?? (MaskableGraphic)GetComponent<RawImage>();
        }

        if (hotspots == null) return;
        foreach (var spot in hotspots)
        {
            spot.min.x = Mathf.Clamp01(spot.min.x);
            spot.min.y = Mathf.Clamp01(spot.min.y);
            spot.max.x = Mathf.Clamp01(spot.max.x);
            spot.max.y = Mathf.Clamp01(spot.max.y);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isActiveAndEnabled) return;
        if (!IsComponentValid()) return;

        Gizmos.color = new Color(0.3f, 0.5f, 0.5f, 0.15f);

        for (int i = 0; i < hotspots.Count; i++)
        {
            var spot = hotspots[i];
            Vector2 min = new Vector2(Mathf.Min(spot.min.x, spot.max.x), Mathf.Min(spot.min.y, spot.max.y));
            Vector2 max = new Vector2(Mathf.Max(spot.min.x, spot.max.x), Mathf.Max(spot.min.y, spot.max.y));

            Vector3 a = UVToWorld(min);
            Vector3 b = UVToWorld(new Vector2(max.x, min.y));
            Vector3 c = UVToWorld(max);
            Vector3 d = UVToWorld(new Vector2(min.x, max.y));

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!isActiveAndEnabled) return;
        if (!IsComponentValid()) return;

        Gizmos.color = new Color(0, 1, 1, 0.7f);

        for (int i = 0; i < hotspots.Count; i++)
        {
            var spot = hotspots[i];
            Vector2 min = new Vector2(Mathf.Min(spot.min.x, spot.max.x), Mathf.Min(spot.min.y, spot.max.y));
            Vector2 max = new Vector2(Mathf.Max(spot.min.x, spot.max.x), Mathf.Max(spot.min.y, spot.max.y));

            Vector3 a = UVToWorld(min);
            Vector3 b = UVToWorld(new Vector2(max.x, min.y));
            Vector3 c = UVToWorld(max);
            Vector3 d = UVToWorld(new Vector2(min.x, max.y));

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }

    private Vector3 UVToWorld(Vector2 uv)
    {
        if (_rectTransform == null) return Vector3.zero;

        _rectTransform.GetWorldCorners(_worldCorners);
        Vector3 bl = _worldCorners[0];
        Vector3 tl = _worldCorners[1];
        Vector3 tr = _worldCorners[2];
        Vector3 br = _worldCorners[3];

        return Vector3.Lerp(Vector3.Lerp(bl, br, uv.x), Vector3.Lerp(tl, tr, uv.x), uv.y);
    }
#endif
}