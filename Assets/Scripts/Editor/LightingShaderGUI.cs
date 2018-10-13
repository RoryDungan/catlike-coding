using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class LightingShaderGUI : ShaderGUI
{
    enum SmoothnessSource
    {
        Uniform, Albedo, Metallic
    }

    enum RenderingMode
    {
        Opaque, Cutout
    }

    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;

    bool shouldShowAlphaCutoff;

    static GUIContent staticLabel = new GUIContent();

    static GUIContent MakeLabel(string text, string tooltip = null)
    {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
    {
        return MakeLabel(property.displayName, tooltip);
    }

    void RecordAction(string label)
    {
        editor.RegisterPropertyChangeUndo(label);
    }

    bool IsKeywordEnabled(string keyword) 
    {
        return target.IsKeywordEnabled(keyword);
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        target = (Material)editor.target;
        this.editor = editor;
        this.properties = properties;
        DoRenderingMode();
        DoMain();
        DoSecondary();
    }

    MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, properties);
    }

    void SetKeyword(string keyword, bool state)
    {
        if (state)
        {
            foreach (Material m in editor.targets) 
            {
                m.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material m in editor.targets)
            {
                m.DisableKeyword(keyword);
            }
        }
    }

    void DoMain()
    {
        GUILayout.Label("Main Maps", EditorStyles.boldLabel);

        var mainTex = FindProperty("_MainTex");
        var tint = FindProperty("_Tint");
        editor.TexturePropertySingleLine(
            MakeLabel(mainTex, "Albedo (RGB)"), 
            mainTex, 
            tint
        );
        if (shouldShowAlphaCutoff)
        {
            DoAlphaCutoff();
        }
        DoMetallic();
        DoSmoothness();
        DoNormals();
        DoOcclusion();
        DoEmission();
        DoDetailMask();
        editor.TextureScaleOffsetProperty(mainTex);
    }

    void DoRenderingMode() 
    {
        var mode = RenderingMode.Opaque;
        shouldShowAlphaCutoff = false;
        if (IsKeywordEnabled("_RENDERING_CUTOUT"))
        {
            mode = RenderingMode.Cutout;
            shouldShowAlphaCutoff = true;
        }

        EditorGUI.BeginChangeCheck();
        mode = (RenderingMode)EditorGUILayout.EnumPopup(
            MakeLabel("Rendering Mode"), 
            mode
        );
        if (EditorGUI.EndChangeCheck())
        {
            RecordAction("Rendering Mode");
            SetKeyword("_RENDERING_CUTOUT", mode == RenderingMode.Cutout);

            var queue = mode == RenderingMode.Opaque
                ? RenderQueue.Geometry
                : RenderQueue.AlphaTest;
            foreach (Material m in editor.targets) 
            {
                m.renderQueue = (int)queue;
            }
        }
    }

    void DoAlphaCutoff() 
    {
        var slider = FindProperty("_AlphaCutoff");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider));
        EditorGUI.indentLevel -= 2;
    }

    void DoMetallic()
    {
        var map = FindProperty("_MetallicMap");
        var tex = map.textureValue;
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(map, "Metallic (R)"),
            map,
            tex ? null : FindProperty("_Metallic")
        );
        if (EditorGUI.EndChangeCheck() && tex != map.textureValue)
        {
            SetKeyword("_METALLIC_MAP", map.textureValue);
        }
    }

    void DoSmoothness()
    {
        var source = SmoothnessSource.Uniform;
        if (IsKeywordEnabled("_SMOOTHNESS_ALBEDO"))
        {
            source = SmoothnessSource.Albedo;
        }
        else if (IsKeywordEnabled("_SMOOTHNESS_METALLIC"))
        {
            source = SmoothnessSource.Metallic;
        }
        var slider = FindProperty("_Smoothness");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider));
        EditorGUI.indentLevel += 1;
        EditorGUI.BeginChangeCheck();
        source = (SmoothnessSource)EditorGUILayout.EnumPopup(
            MakeLabel("Source"), 
            source
        );
        if (EditorGUI.EndChangeCheck()) 
        {
            RecordAction("Smoothness Source");
            SetKeyword("_SMOOTHNESS_ALBEDO", source == SmoothnessSource.Albedo);
            SetKeyword("_SMOOTHNESS_METALLIC", source == SmoothnessSource.Metallic);
        }
        EditorGUI.indentLevel -= 1;
        EditorGUI.indentLevel -= 2;
    }

    void DoNormals()
    {
        var map = FindProperty("_NormalMap");
        var tex = map.textureValue;
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(map), 
            map, 
            tex ? FindProperty("_BumpScale") : null
        );
        if (EditorGUI.EndChangeCheck() && map.textureValue != tex)
        {
            SetKeyword("_NORMAL_MAP", map.textureValue);
        }
    }

    void DoSecondary()
    {
        GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);

        var detailTex = FindProperty("_DetailTex");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(detailTex, "Albedo (RGB) multiplied by 2"),
            detailTex
        );
        if (EditorGUI.EndChangeCheck()) 
        {
            SetKeyword("_DETAIL_ALBEDO_MAP", detailTex.textureValue);
        }
        DoSecondaryNormals();
        editor.TextureScaleOffsetProperty(detailTex);
    }

    void DoSecondaryNormals()
    {
        var map = FindProperty("_DetailNormalMap");
        var tex = map.textureValue;
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(map),
            map,
            tex ? FindProperty("_DetailBumpScale") : null
        );
        if (EditorGUI.EndChangeCheck() && map.textureValue != tex)
        {
            SetKeyword("_DETAIL_NORMAL_MAP", map.textureValue);
        }
    }

    void DoOcclusion()
    {
        var map = FindProperty("_OcclusionMap");
        var tex = map.textureValue;
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(map, "Occlusion (G)"),
            map,
            tex ? FindProperty("_OcclusionStrength") : null
        );
        if (EditorGUI.EndChangeCheck() && map.textureValue != tex)
        {
            SetKeyword("_OCCLUSION_MAP", map.textureValue);
        }
    }

    void DoEmission()
    {
        var map = FindProperty("_EmissionMap");
        var tex = map.textureValue;
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertyWithHDRColor(
            MakeLabel(map, "Emission (RGB)"), 
            map, 
            FindProperty("_Emission"),
            false
        );
        if (EditorGUI.EndChangeCheck() && map.textureValue != tex) 
        {
            SetKeyword("_EMISSION_MAP", map.textureValue);
        }
    }

    void DoDetailMask()
    {
        var mask = FindProperty("_DetailMask");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(mask, "Detail Mask (A)"),
            mask
        );
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_DETAIL_MASK", mask.textureValue);
        }
    }
}
