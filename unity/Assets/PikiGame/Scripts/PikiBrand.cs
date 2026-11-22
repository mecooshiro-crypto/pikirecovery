// =====================================================================
//  PIKI GAME · Marca (según el Manual de marca de Piki).
//  · Nombre: Piki (antes PikiRecovery). El videojuego es "Piki Game".
//  · Claim: "Recuperar también es entrenar."
//  · Logos: isologo, isotipo y logotipo en blanco (se tiñen por código)
//    en Resources/PikiMarca.
//  · Tipografía: Poppins (licencia OFL) en Resources/PikiFuentes.
//    Titulares y números grandes en ExtraBold cursiva, como pide el manual.
// =====================================================================
using UnityEngine;

namespace Piki
{
    public static class Brand
    {
        public const string Name = "Piki", Game = "Piki Game", Claim = "Recuperar también es entrenar.",
            Concept = "El trabajo invisible", Tagline = "Recuperación para deportistas";

        /* ------------------------------ Tipografía ------------------------------ */
        // Tamaño (en unidades de panel) desde el que un texto en negrita se considera titular
        public const float DisplayMin = 44;
        // Poppins es un poco más ancha que la fuente de sistema: se compensa para mantener las medidas
        public const float PoppinsScale = .94f;
        static Font fMed, fMedIt, fBold, fBoldIt, fDisplay; static bool loaded;
        static void Load()
        {
            if (loaded) return; loaded = true;
            fMed = Resources.Load<Font>("PikiFuentes/Poppins-Medium");
            fMedIt = Resources.Load<Font>("PikiFuentes/Poppins-MediumItalic");
            fBold = Resources.Load<Font>("PikiFuentes/Poppins-Bold");
            fBoldIt = Resources.Load<Font>("PikiFuentes/Poppins-BoldItalic");
            fDisplay = Resources.Load<Font>("PikiFuentes/Poppins-ExtraBoldItalic");
        }
        public static void Reload() { loaded = false; isologo = isotipo = logotipo = null; }
        public static bool HasPoppins { get { Load(); return fMed != null && fBold != null; } }
        public static Font FontFor(bool bold, bool italic, float size)
        {
            Load();
            Font f;
            if (bold && size >= DisplayMin) f = fDisplay != null ? fDisplay : fBold;  // titulares: ExtraBold cursiva
            else if (bold) f = italic ? (fBoldIt != null ? fBoldIt : fBold) : fBold;
            else f = italic ? (fMedIt != null ? fMedIt : fMed) : fMed;
            return f != null ? f : UI.LegacyFont;
        }
        public static bool IsPoppins(Font f) { return f != null && f.name.StartsWith("Poppins"); }
        // Proporciones verticales de la fuente (en "em"), leídas de la fuente importada
        public static float Ascent(Font f)
        {
            if (!IsPoppins(f)) return .905f;
            if (f.fontSize > 0 && f.ascent > 0) { float a = f.ascent / f.fontSize; if (a > .7f && a < 1.4f) return a; }
            return 1.05f;
        }
        public static float LineHeight(Font f)
        {
            if (!IsPoppins(f)) return 1.15f;
            if (f.fontSize > 0 && f.lineHeight > 0) { float l = (float)f.lineHeight / f.fontSize; if (l > 1f && l < 2f) return l; }
            return 1.5f;
        }

        /* ------------------------------ Logos ------------------------------ */
        static Sprite isologo, isotipo, logotipo;
        public static Sprite Isologo { get { return Logo(ref isologo, "piki-isologo-blanco"); } }   // P + IKI + bajada
        public static Sprite Isotipo { get { return Logo(ref isotipo, "piki-isotipo-blanco"); } }   // la P con la figura
        public static Sprite Logotipo { get { return Logo(ref logotipo, "piki-logotipo-blanco"); } } // PIKI + línea + bajada
        static Sprite Logo(ref Sprite cache, string name)
        {
            if (cache != null) return cache;
            var s = Resources.Load<Sprite>("PikiMarca/" + name);
            if (s == null)
            {
                var t = Resources.Load<Texture2D>("PikiMarca/" + name);
                if (t != null)
                {
                    s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), 100);
                    s.name = name; Persist.Keep(s, "Sprite_" + name);
                }
            }
            cache = s; return s;
        }
        // Proporción ancho/alto de un logo (para ubicarlo sin deformarlo)
        public static float Aspect(Sprite s) { if (s == null) return 1; var b = s.rect; return b.height > 0 ? b.width / b.height : 1; }

        // Dibuja un logo en un panel a una altura dada; devuelve el ancho que ocupa
        public static float Draw(UIPanel p, Sprite logo, float x, float y, float h, Color c, UI.Al al = UI.Al.L)
        {
            if (logo == null) return 0;
            float w = h * Aspect(logo); float x0 = al == UI.Al.L ? x : al == UI.Al.C ? x - w / 2 : x - w;
            UI.Img(p, x0, y, w, h, logo, c).name = "Logo " + logo.name;
            return w;
        }
    }
}
