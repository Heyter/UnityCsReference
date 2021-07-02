// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor
{
    // Modules/UIServiceEditor/EditorToolbar/ToolbarElements/BuiltinTools.cs
    [Overlay(typeof(SceneView), k_Id, "Tools", true)]
    [Icon("Icons/Overlays/StandardTools.png")]
    class TransformToolsOverlayToolBar : ToolbarOverlay
    {
        const string k_Id = "unity-transform-toolbar";
        public TransformToolsOverlayToolBar() : base("Tools/Builtin Tools") {}
    }

    [Overlay(typeof(SceneView), k_Id, "View Options", true)]
    [Icon("Icons/Overlays/ViewOptions.png")]
    class SceneViewToolBar : ToolbarOverlay
    {
        const string k_Id = "unity-scene-view-toolbar";
        public SceneViewToolBar() : base(
            "SceneView/Camera Mode",
            "SceneView/2D",
            "SceneView/Lighting",
            "SceneView/Audio",
            "SceneView/Fx",
            "SceneView/Scene Visibility",
            "SceneView/Render Doc",
            "SceneView/Metal Capture",
            "SceneView/Scene Camera",
            "SceneView/Gizmos") {}
    }

    [Overlay(typeof(SceneView), k_Id, "Search", true)]
    [Icon("Icons/Overlays/SearchOverlay.png")]
    class SearchToolBar : Overlay, ICreateHorizontalToolbar
    {
        const string k_Id = "unity-search-toolbar";

        public override VisualElement CreatePanelContent()
        {
            return new IMGUIContainer { onGUIHandler = OnGUI };
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (containerWindow is SceneView sceneView)
                sceneView.ToolbarSearchFieldGUI();
            EditorGUILayout.EndHorizontal();
        }

        public VisualElement CreateHorizontalToolbarContent() => CreatePanelContent();
    }

    [Overlay(typeof(SceneView), k_Id, "Grid and Snap", true)]
    [Icon("Icons/Overlays/GridAndSnap.png")]
    class GridAndSnapToolBar : ToolbarOverlay
    {
        const string k_Id = "unity-grid-and-snap-toolbar";

        public GridAndSnapToolBar() : base(
            "SceneView/Grids",
            "Tools/Snap Settings",
            "SceneView/Snap Increment") {}
    }
}
// namespace
