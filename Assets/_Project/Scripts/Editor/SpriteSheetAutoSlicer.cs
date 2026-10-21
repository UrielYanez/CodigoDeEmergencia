#if UNITY_EDITOR
#pragma warning disable 0618
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodigoDeEmergencia.EditorTools
{
    /// <summary>
    /// Herramienta de Editor para:
    /// 1. Eliminar automáticamente fondo de color croma (verde #00FF00 o magenta) de imágenes generadas por IA.
    /// 2. Configurar modo Multiple, filtro Bilinear (estilo Hollow Knight limpio) y recortar la cuadrícula de sprites.
    /// No requiere Python ni Photoshop: todo se realiza dentro de Unity con un solo clic.
    /// </summary>
    public class SpriteSheetAutoSlicer : EditorWindow
    {
        private Texture2D targetTexture;
        private int columns = 6;
        private int rows = 1;
        private int pixelsPerUnit = 100;
        private FilterMode filterMode = FilterMode.Bilinear;
        private Vector2 spritePivot = new Vector2(0.5f, 0.0f); // Base de los pies

        [Header("Eliminación de Fondo Croma")]
        private bool removeChroma = true;
        private Color chromaKeyColor = new Color(0f, 1f, 0f, 1f); // Verde puro
        private float chromaTolerance = 0.28f;

        [MenuItem("Tools/Código de Emergencia/Auto-Slice HD SpriteSheet")]
        public static void ShowWindow()
        {
            SpriteSheetAutoSlicer window = GetWindow<SpriteSheetAutoSlicer>("Auto-Slice HD");
            window.minSize = new Vector2(400, 420);

            if (Selection.activeObject is Texture2D selectedTex)
            {
                window.targetTexture = selectedTex;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("🎨 Recortador Automático de Spritesheets HD", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Diseñado para spritesheets generados por Gemini/Argón. Puede remover el fondo croma y recortar los cuadros automáticamente.",
                MessageType.Info
            );

            EditorGUILayout.Space(8);

            targetTexture = (Texture2D)EditorGUILayout.ObjectField("Textura / Spritesheet", targetTexture, typeof(Texture2D), false);
            columns = EditorGUILayout.IntSlider("Columnas (Frames X)", columns, 1, 24);
            rows = EditorGUILayout.IntSlider("Filas (Frames Y)", rows, 1, 12);
            pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit (PPU)", pixelsPerUnit);
            filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filtro de Textura", filterMode);
            spritePivot = EditorGUILayout.Vector2Field("Pivote (0.5, 0.0 = Pies)", spritePivot);

            EditorGUILayout.Space(8);
            GUILayout.Label("🛠️ Transparencia de Fondo (Chroma Key)", EditorStyles.boldLabel);
            removeChroma = EditorGUILayout.Toggle("Remover Fondo Croma", removeChroma);
            if (removeChroma)
            {
                chromaKeyColor = EditorGUILayout.ColorField("Color a Remover", chromaKeyColor);
                chromaTolerance = EditorGUILayout.Slider("Tolerancia de Color", chromaTolerance, 0.05f, 0.6f);
            }

            EditorGUILayout.Space(12);

            EditorGUI.BeginDisabledGroup(targetTexture == null);
            if (GUILayout.Button("✂️ Procesar Transparencia y Recortar", GUILayout.Height(40)))
            {
                ProcessAndSliceTexture();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ProcessAndSliceTexture()
        {
            if (targetTexture == null) return;

            string assetPath = AssetDatabase.GetAssetPath(targetTexture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer == null)
            {
                EditorUtility.DisplayDialog("Error", "No se pudo acceder al TextureImporter.", "OK");
                return;
            }

            // Paso 1: Eliminar color croma si está activado
            if (removeChroma)
            {
                bool wasReadable = importer.isReadable;
                importer.isReadable = true;
                importer.SaveAndReimport();

                Color32[] pixels = targetTexture.GetPixels32();
                Color32 key = chromaKeyColor;

                for (int i = 0; i < pixels.Length; i++)
                {
                    Color32 p = pixels[i];
                    float diff = Mathf.Sqrt(
                        Mathf.Pow((p.r - key.r) / 255f, 2) +
                        Mathf.Pow((p.g - key.g) / 255f, 2) +
                        Mathf.Pow((p.b - key.b) / 255f, 2)
                    );

                    if (diff < chromaTolerance)
                    {
                        pixels[i] = new Color32(0, 0, 0, 0); // Transparente total
                    }
                }

                Texture2D transparentTex = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);
                transparentTex.SetPixels32(pixels);
                transparentTex.Apply();

                byte[] pngBytes = transparentTex.EncodeToPNG();
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                File.WriteAllBytes(absolutePath, pngBytes);

                DestroyImmediate(transparentTex);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            // Paso 2: Configuración de importación para 2D HD
            importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            importer.filterMode = filterMode;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;

            // Paso 3: Generación de cuadrícula de sprites
            int textureWidth = targetTexture.width;
            int textureHeight = targetTexture.height;
            float frameWidth = (float)textureWidth / columns;
            float frameHeight = (float)textureHeight / rows;

            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            string baseName = targetTexture.name;

            int index = 0;
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < columns; c++)
                {
                    SpriteMetaData meta = new SpriteMetaData
                    {
                        rect = new Rect(c * frameWidth, r * frameHeight, frameWidth, frameHeight),
                        name = $"{baseName}_{index:D2}",
                        alignment = (int)SpriteAlignment.Custom,
                        pivot = spritePivot
                    };
                    metas.Add(meta);
                    index++;
                }
            }

            importer.spritesheet = metas.ToArray();

            EditorUtility.SetDirty(importer);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            EditorUtility.DisplayDialog(
                "¡Proceso Completado!",
                $"Spritesheet '{targetTexture.name}' configurado con éxito.\n• Transparencia aplicada.\n• {metas.Count} sprites individuales generados.\n• Filtro Bilinear aplicado.",
                "Excelente"
            );
        }
    }
}
#endif
