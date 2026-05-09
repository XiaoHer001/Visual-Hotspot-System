using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 功能描述：为 VisualHotspotSystem 组件提供一个强大的、可视化的编辑器界面。
/// 允许用户在Scene视图中直接创建、移动、缩放热区，并提供复制和删除功能，极大地提升了编辑效率。
/// </summary>
/// <remarks>
/// 注意：
/// 1. 此脚本必须放置在项目中的 "Editor" 文件夹下。
/// 2. 提供了比手动输入UV坐标更直观、高效的编辑工作流。
/// 3. 所有通过此编辑器进行的操作都完整支持Undo/Redo。
/// 4. 场景中的可视化编辑手柄仅在 VisualHotspotSystem 组件处于激活状态且正确设置了 Target Graphic 后才会显示。
/// </remarks>
/// <dependencies>
/// 依赖：UnityEditor, VisualHotspotSystem.cs, Hotspot.cs
/// </dependencies>

[CustomEditor(typeof(VisualHotspotSystem))]
public class VisualHotspotSystemEditor : Editor
{
    private VisualHotspotSystem _targetScript;
    private RectTransform _rectTransform;
    private readonly Vector3[] _worldCorners = new Vector3[4];

    private SerializedProperty _hotspotsProp;
    private SerializedProperty _targetGraphicProp;

    private void OnEnable()
    {
        _targetScript = (VisualHotspotSystem)target;

        _hotspotsProp = serializedObject.FindProperty("hotspots");
        _targetGraphicProp = serializedObject.FindProperty("targetGraphic");

        UpdateRectTransform();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_targetGraphicProp);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            UpdateRectTransform();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("热区列表", EditorStyles.boldLabel);

        for (int i = 0; i < _hotspotsProp.arraySize; i++)
        {
            EditorGUILayout.BeginVertical("box");
            SerializedProperty spotProp = _hotspotsProp.GetArrayElementAtIndex(i);

            spotProp.isExpanded = EditorGUILayout.Foldout(spotProp.isExpanded, spotProp.FindPropertyRelative("name").stringValue, true);
            if (spotProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spotProp.FindPropertyRelative("name"));
                EditorGUILayout.PropertyField(spotProp.FindPropertyRelative("min"));
                EditorGUILayout.PropertyField(spotProp.FindPropertyRelative("max"));
                EditorGUILayout.PropertyField(spotProp.FindPropertyRelative("onClick"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("复制", GUILayout.Width(60)))
            {
                _hotspotsProp.InsertArrayElementAtIndex(i);
            }
            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                _hotspotsProp.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        if (GUILayout.Button("添加新热区"))
        {
            _hotspotsProp.arraySize++;
            var newSpot = _hotspotsProp.GetArrayElementAtIndex(_hotspotsProp.arraySize - 1);
            newSpot.FindPropertyRelative("name").stringValue = "新热区 " + _hotspotsProp.arraySize;
            newSpot.FindPropertyRelative("min").vector2Value = new Vector2(0.1f, 0.1f);
            newSpot.FindPropertyRelative("max").vector2Value = new Vector2(0.2f, 0.2f);
            newSpot.FindPropertyRelative("onClick").FindPropertyRelative("m_PersistentCalls.m_Calls").ClearArray();
            newSpot.isExpanded = true;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        // 最重要的修复：只有组件启用的时候才显示编辑手柄
        if (!_targetScript.enabled) return;

        if (_targetScript.targetGraphic == null || _rectTransform == null)
        {
            Handles.Label(_targetScript.transform.position, "请在Inspector中指定Target Graphic", new GUIStyle { normal = { textColor = Color.yellow } });
            return;
        }

        serializedObject.Update();

        for (int i = 0; i < _hotspotsProp.arraySize; i++)
        {
            DrawHotspotHandles(_hotspotsProp.GetArrayElementAtIndex(i));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHotspotHandles(SerializedProperty spotProp)
    {
        SerializedProperty minProp = spotProp.FindPropertyRelative("min");
        SerializedProperty maxProp = spotProp.FindPropertyRelative("max");

        Vector2 minUV = minProp.vector2Value;
        Vector2 maxUV = maxProp.vector2Value;

        Vector2 correctedMin = new Vector2(Mathf.Min(minUV.x, maxUV.x), Mathf.Min(minUV.y, maxUV.y));
        Vector2 correctedMax = new Vector2(Mathf.Max(minUV.x, maxUV.x), Mathf.Max(minUV.y, maxUV.y));

        Vector3 world_bl = UVToWorld(new Vector2(correctedMin.x, correctedMin.y));
        Vector3 world_br = UVToWorld(new Vector2(correctedMax.x, correctedMin.y));
        Vector3 world_tl = UVToWorld(new Vector2(correctedMin.x, correctedMax.y));
        Vector3 world_tr = UVToWorld(correctedMax);

        Handles.color = Color.cyan;
        Handles.DrawPolyLine(world_bl, world_br, world_tr, world_tl, world_bl);

        float handleSize = HandleUtility.GetHandleSize(world_bl) * 0.05f;

        EditorGUI.BeginChangeCheck();
        Vector3 new_bl = DrawHandle(world_bl, handleSize);
        Vector3 new_br = DrawHandle(world_br, handleSize);
        Vector3 new_tl = DrawHandle(world_tl, handleSize);
        Vector3 new_tr = DrawHandle(world_tr, handleSize);

        Vector3 centerWorld = (world_bl + world_tr) / 2f;
        Handles.color = Color.yellow;
        Vector3 newCenterWorld = Handles.FreeMoveHandle(centerWorld, Quaternion.identity, handleSize * 2, Vector3.zero, Handles.RectangleHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_targetScript, "Modify Hotspot");

            if (new_bl != world_bl) minProp.vector2Value = WorldToUV(new_bl);
            if (new_tr != world_tr) maxProp.vector2Value = WorldToUV(new_tr);
            if (new_br != world_br)
            {
                var uv = WorldToUV(new_br);
                minProp.vector2Value = new Vector2(minProp.vector2Value.x, uv.y);
                maxProp.vector2Value = new Vector2(uv.x, maxProp.vector2Value.y);
            }
            if (new_tl != world_tl)
            {
                var uv = WorldToUV(new_tl);
                minProp.vector2Value = new Vector2(uv.x, minProp.vector2Value.y);
                maxProp.vector2Value = new Vector2(maxProp.vector2Value.x, uv.y);
            }
            if (newCenterWorld != centerWorld)
            {
                Vector3 delta = newCenterWorld - centerWorld;
                minProp.vector2Value = WorldToUV(world_bl + delta);
                maxProp.vector2Value = WorldToUV(world_tr + delta);
            }
        }
    }

    private Vector3 DrawHandle(Vector3 position, float size)
    {
        Handles.color = Color.white;
        return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.DotHandleCap);
    }

    private void UpdateRectTransform()
    {
        if (_targetGraphicProp.objectReferenceValue != null)
        {
            _rectTransform = ((MaskableGraphic)_targetGraphicProp.objectReferenceValue).rectTransform;
        }
        else
        {
            _rectTransform = null;
        }
    }

    private Vector3 UVToWorld(Vector2 uv)
    {
        if (_rectTransform == null) return Vector3.zero;
        _rectTransform.GetWorldCorners(_worldCorners);
        // [0] = Bottom-Left, [1] = Top-Left, [2] = Top-Right, [3] = Bottom-Right
        return Vector3.Lerp(
            Vector3.Lerp(_worldCorners[0], _worldCorners[3], uv.x),
            Vector3.Lerp(_worldCorners[1], _worldCorners[2], uv.x),
            uv.y);
    }

    private Vector2 WorldToUV(Vector3 worldPos)
    {
        if (_rectTransform == null) return Vector2.zero;
        _rectTransform.GetWorldCorners(_worldCorners);
        Vector3 bl = _worldCorners[0];
        Vector3 xAxis = _worldCorners[3] - bl;
        Vector3 yAxis = _worldCorners[1] - bl;
        Vector3 offset = worldPos - bl;
        float u = Vector3.Dot(offset, xAxis.normalized) / xAxis.magnitude;
        float v = Vector3.Dot(offset, yAxis.normalized) / yAxis.magnitude;
        return new Vector2(Mathf.Clamp01(u), Mathf.Clamp01(v));
    }
}