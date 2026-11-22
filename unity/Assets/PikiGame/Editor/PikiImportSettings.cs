// =====================================================================
//  PIKI GAME · Ajustes de importación de la marca (solo Editor)
//  · Logos (Resources/PikiMarca): se importan como Sprite con
//    transparencia, para teñirlos con los colores de la marca.
//  · Poppins (Resources/PikiFuentes): fuente dinámica con la línea base
//    medida desde el "ascender" de la fuente (alineación exacta).
//  · Ícono de la app (Marca/piki-icono-app-1024.png).
// =====================================================================
using UnityEditor;
using UnityEngine;

namespace Piki.EditorTools
{
    public class PikiImportSettings : AssetPostprocessor
    {
        const string Logos = "/PikiGame/Resources/PikiMarca/", Fonts = "/PikiGame/Resources/PikiFuentes/", Icon = "/PikiGame/Marca/";

        void OnPreprocessTexture()
        {
            var ti = assetImporter as TextureImporter; if (ti == null) return;
            if (assetPath.Contains(Logos)) ConfigureLogo(ti);
            else if (assetPath.Contains(Icon)) ConfigureIcon(ti);
        }
        void OnPreprocessAsset()
        {
            var fi = assetImporter as TrueTypeFontImporter;
            if (fi != null && assetPath.Contains(Fonts)) ConfigureFont(fi);
        }

        public static bool ConfigureLogo(TextureImporter ti)
        {
            bool changed = ti.textureType != TextureImporterType.Sprite || !ti.alphaIsTransparency || ti.maxTextureSize != 1024 || !ti.mipmapEnabled;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single; ti.spritePixelsPerUnit = 100;
            ti.alphaIsTransparency = true; ti.mipmapEnabled = true; ti.maxTextureSize = 1024;
            ti.wrapMode = TextureWrapMode.Clamp; ti.filterMode = FilterMode.Trilinear;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            return changed;
        }
        public static bool ConfigureIcon(TextureImporter ti)
        {
            bool changed = !ti.alphaIsTransparency || ti.npotScale != TextureImporterNPOTScale.None;
            ti.textureType = TextureImporterType.Default; ti.alphaIsTransparency = true; ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None; ti.textureCompression = TextureImporterCompression.Uncompressed;
            return changed;
        }
        public static bool ConfigureFont(TrueTypeFontImporter fi)
        {
            bool changed = fi.fontTextureCase != FontTextureCase.Dynamic || fi.ascentCalculationMode != AscentCalculationMode.FaceAscender;
            fi.fontTextureCase = FontTextureCase.Dynamic; fi.includeFontData = true;
            fi.ascentCalculationMode = AscentCalculationMode.FaceAscender;
            return changed;
        }

        // Revisa (y corrige si hace falta) los ajustes de todos los archivos de la marca
        public static void EnsureAll(string root)
        {
            foreach (var guid in AssetDatabase.FindAssets("", new[] { root + "/Resources/PikiMarca", root + "/Marca" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                var ti = AssetImporter.GetAtPath(p) as TextureImporter; if (ti == null) continue;
                bool ch = p.Contains(Logos) ? ConfigureLogo(ti) : ConfigureIcon(ti);
                if (ch) ti.SaveAndReimport();
            }
            foreach (var guid in AssetDatabase.FindAssets("", new[] { root + "/Resources/PikiFuentes" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                var fi = AssetImporter.GetAtPath(p) as TrueTypeFontImporter; if (fi == null) continue;
                if (ConfigureFont(fi)) fi.SaveAndReimport();
            }
        }
    }
}
