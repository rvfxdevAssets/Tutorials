// Assets/RVFX/Tools/CurveToTexture.cs
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RVFX.Tools
{
#if UNITY_EDITOR
    public sealed class CurveToTexture : EditorWindow
    {
        private const int Width = 32;
        private const int Height = 1;

        [SerializeField] private AnimationCurve _x = ConstantOne();
        [SerializeField] private AnimationCurve _y = ConstantOne();
        [SerializeField] private AnimationCurve _z = ConstantOne();
        [SerializeField] private AnimationCurve _w = ConstantOne();

        [SerializeField] private string _savePath = "Assets/RVFX/Tools/CurveTexture_32x1.png";

        private Texture2D _previewTex;
        private bool _previewDirty = true;

        private static readonly GUIContent GC_Title = new GUIContent("Curve To Texture");
        private static readonly GUIContent GC_R = new GUIContent("R", "X curve");
        private static readonly GUIContent GC_G = new GUIContent("G", "Y curve");
        private static readonly GUIContent GC_B = new GUIContent("B", "Z curve");
        private static readonly GUIContent GC_A = new GUIContent("A", "W curve");

        [MenuItem("RVFX/Tools/Curve To Texture (32x1)")]
        public static void Open()
        {
            var win = GetWindow<CurveToTexture>("Curve To Texture (32x1)");
            win.minSize = new Vector2(520, 360);
            win.Show();
        }

        private static AnimationCurve ConstantOne()
        {
            return new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        }

        private void OnEnable()
        {
            _previewDirty = true;
        }

        private void OnDisable()
        {
            if (_previewTex != null) DestroyImmediate(_previewTex);
            _previewTex = null;
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.62f)))
                {
                    DrawCurvesCard();
                    EditorGUILayout.Space(8);
                    DrawSaveCard();
                }

                EditorGUILayout.Space(10);

                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    DrawPreviewCard();
                    GUILayout.FlexibleSpace();
                    DrawActionsCard();
                }
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.KeyDown)
                Repaint();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(GC_Title, EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(!_previewDirty))
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), GUILayout.Width(30), GUILayout.Height(20)))
                            _previewDirty = true;
                    }
                }

                EditorGUILayout.LabelField($"{Width}x{Height} PNG  •  Linear (sRGB off)  •  Point/Clamp  •  No mipmaps", EditorStyles.miniLabel);
            }
        }

        private void DrawCurvesCard()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Curves", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Reset", GUILayout.Width(70), GUILayout.Height(20)))
                    {
                        _x = ConstantOne();
                        _y = ConstantOne();
                        _z = ConstantOne();
                        _w = ConstantOne();
                        _previewDirty = true;
                    }
                }

                EditorGUILayout.Space(6);

                using (var cc = new EditorGUI.ChangeCheckScope())
                {
                    _x = CurveRow(GC_R, _x);
                    _y = CurveRow(GC_G, _y);
                    _z = CurveRow(GC_B, _z);
                    _w = CurveRow(GC_A, _w);

                    if (cc.changed) _previewDirty = true;
                }
            }
        }

        private AnimationCurve CurveRow(GUIContent label, AnimationCurve curve)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var tag = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18), GUILayout.Height(18));
                DrawChannelDot(tag, label.text);
                EditorGUILayout.LabelField(label, GUILayout.Width(22));
                return EditorGUILayout.CurveField(curve, GUILayout.ExpandWidth(true));
            }
        }

        private void DrawSaveCard()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Save", EditorStyles.boldLabel);
                EditorGUILayout.Space(6);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _savePath = EditorGUILayout.TextField(_savePath);

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(32), GUILayout.Height(20)))
                    {
                        var file = EditorUtility.SaveFilePanel(
                            "Save 32x1 PNG",
                            Application.dataPath,
                            "CurveTexture_32x1",
                            "png"
                        );

                        if (!string.IsNullOrEmpty(file))
                        {
                            var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                            var full = Path.GetFullPath(file);

                            if (full.StartsWith(projectPath))
                            {
                                var rel = full.Substring(projectPath.Length + 1).Replace('\\', '/');
                                _savePath = rel;
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Invalid Path", "Save inside this project's Assets folder.", "OK");
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(_savePath) || !_savePath.StartsWith("Assets/"))
                {
                    EditorGUILayout.Space(6);
                    EditorGUILayout.HelpBox("Path must start with 'Assets/'.", MessageType.Warning);
                }
            }
        }

        private void DrawPreviewCard()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                EditorGUILayout.Space(6);

                EnsurePreviewUpToDate();

                var r = GUILayoutUtility.GetRect(10, 70, GUILayout.ExpandWidth(true));
                r.height = Mathf.Max(64f, r.height);

                if (_previewTex != null)
                {
                    var texRect = new Rect(r.x, r.y, r.width, r.height);
                    EditorGUI.DrawPreviewTexture(texRect, _previewTex, null, ScaleMode.StretchToFill);
                    EditorGUI.LabelField(new Rect(r.x, r.yMax + 4, r.width, 16), "Left → Right : t = 0 → 1", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUI.HelpBox(r, "Preview unavailable.", MessageType.Info);
                }
            }
        }

        private void DrawActionsCard()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
                EditorGUILayout.Space(6);

                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_savePath) || !_savePath.StartsWith("Assets/")))
                {
                    var big = GUILayout.Height(34);
                    if (GUILayout.Button("Generate & Save", big))
                        GenerateAndSave();
                }
            }
        }

        private void EnsurePreviewUpToDate()
        {
            if (!_previewDirty) return;

            if (_previewTex == null)
            {
                _previewTex = new Texture2D(Width, Height, TextureFormat.RGBA32, false, true);
                _previewTex.wrapMode = TextureWrapMode.Clamp;
                _previewTex.filterMode = FilterMode.Point;
                _previewTex.hideFlags = HideFlags.HideAndDontSave;
                _previewTex.name = "CurveToTexture_Preview";
            }

            var pixels = new Color32[Width];
            for (int x = 0; x < Width; x++)
            {
                float t = x / (float)(Width - 1);

                float r = Mathf.Clamp01(_x != null ? _x.Evaluate(t) : 1f);
                float g = Mathf.Clamp01(_y != null ? _y.Evaluate(t) : 1f);
                float b = Mathf.Clamp01(_z != null ? _z.Evaluate(t) : 1f);
                float a = Mathf.Clamp01(_w != null ? _w.Evaluate(t) : 1f);

                pixels[x] = new Color(r, g, b, a);
            }

            _previewTex.SetPixels32(pixels);
            _previewTex.Apply(false, false);

            _previewDirty = false;
        }

        private void GenerateAndSave()
        {
            if (string.IsNullOrWhiteSpace(_savePath) || !_savePath.StartsWith("Assets/"))
            {
                EditorUtility.DisplayDialog("Invalid Path", "The save path must start with 'Assets/'.", "OK");
                return;
            }

            EnsureFolderExists(_savePath);

            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false, true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            var pixels = new Color32[Width];

            for (int x = 0; x < Width; x++)
            {
                float t = x / (float)(Width - 1);

                float r = Mathf.Clamp01(_x != null ? _x.Evaluate(t) : 1f);
                float g = Mathf.Clamp01(_y != null ? _y.Evaluate(t) : 1f);
                float b = Mathf.Clamp01(_z != null ? _z.Evaluate(t) : 1f);
                float a = Mathf.Clamp01(_w != null ? _w.Evaluate(t) : 1f);

                pixels[x] = new Color(r, g, b, a);
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);

            File.WriteAllBytes(_savePath, tex.EncodeToPNG());
            DestroyImmediate(tex);


            AssetDatabase.ImportAsset(_savePath, ImportAssetOptions.ForceUpdate);
            ApplyImporterSettings(_savePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Done", $"Saved:\n{_savePath}", "OK");
        }

        private void EnsureFolderExists(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(dir)) return;
            if (AssetDatabase.IsValidFolder(dir)) return;

            var parts = dir.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private void ApplyImporterSettings(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;
            importer.sRGBTexture = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            importer.SaveAndReimport();
        }

        private void DrawChannelDot(Rect rect, string channel)
        {
            var c = GUI.color;
            GUI.color = channel == "R" ? Color.red :
                        channel == "G" ? Color.green :
                        channel == "B" ? Color.blue :
                        Color.white;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = c;

            var border = rect;
            border.xMin -= 1;
            border.yMin -= 1;
            border.xMax += 1;
            border.yMax += 1;
            Handles.BeginGUI();
            Handles.color = new Color(0, 0, 0, 0.35f);
            Handles.DrawAAPolyLine(2f, new Vector3(border.xMin, border.yMin), new Vector3(border.xMax, border.yMin),
                new Vector3(border.xMax, border.yMax), new Vector3(border.xMin, border.yMax), new Vector3(border.xMin, border.yMin));
            Handles.EndGUI();
        }
    }
#endif
}
