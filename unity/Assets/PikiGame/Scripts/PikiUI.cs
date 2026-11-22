// =====================================================================
//  PIKI GAME · Interfaz 3D sin paquetes extra.
//  Usa solo piezas que vienen siempre en Unity: SpriteRenderer (fondos,
//  íconos, barras) y TextMesh (textos). No necesita el paquete Unity UI.
//
//  Cada panel es un UIPanel. Dentro de un panel las coordenadas van en
//  "unidades de panel" (1 unidad = 2,5 mm), con origen arriba a la
//  izquierda y la Y hacia abajo. Los textos se ubican por su línea base.
// =====================================================================
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{




    /* ------------------------------ Fábrica de interfaz ------------------------------ */
    public static class UI
    {
        public const float S = 0.0025f;
        static Font font;
        // Fuente de sistema (respaldo si no se encuentra Poppins)
        public static Font LegacyFont
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return font;
            }
        }
        // Fuente principal: Poppins (Manual de marca)
        public static Font Font { get { return Brand.FontFor(false, false, 20); } }
        // Fondo de los paneles: petróleo profundo (Manual de marca · paleta de Piki Game)
        public static Color PanelA { get { return Access.HighContrast ? new Color(0, .09f, .11f, 1) : new Color(0, .2f, .243f, .95f); } }

        static readonly Dictionary<int, Material> spriteMats = new Dictionary<int, Material>();
        static readonly Dictionary<string, Material> textMats = new Dictionary<string, Material>();
        public static void ClearCache() { spriteMats.Clear(); textMats.Clear(); }
        public static Material SpriteMat(int queue)
        {
            Material m; if (spriteMats.TryGetValue(queue, out m) && m != null) return m;
            m = Mat.Unlit(Color.white, null, queue > 0 ? queue : 3000); spriteMats[queue] = m; return m;
        }
        public static Material TextMat(Font f, int queue)
        {
            if (f == null) f = LegacyFont;
            string key = f.name + "_" + queue;
            Material m; if (textMats.TryGetValue(key, out m) && m != null) return m;
            m = f.material != null ? new Material(f.material) : Mat.Unlit(Color.white, null, 3000); if (queue > 0) m.renderQueue = queue;
            m.name = "Texto_" + f.name;
            textMats[key] = Persist.Keep(m, "Texto_" + f.name); return m;
        }

        public static UIPanel Canvas(Transform parent, Vector3 pos, float wM, float hM, Vector3 euler = default(Vector3), string name = "Panel", float unit = S, float dppu = 3f, int queue = 0, int order = 0)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var p = go.AddComponent<UIPanel>(); p.W = wM / unit; p.H = hM / unit; p.queue = queue; p.baseOrder = order;
            go.transform.localPosition = pos; go.transform.localEulerAngles = euler; go.transform.localScale = Vector3.one * unit;
            return p;
        }
        public static float Wd(UIPanel c) { return c.W; }
        public static float Ht(UIPanel c) { return c.H; }

        public static UIPanel Rect(UIPanel p, float x, float y, float w, float h, string n = "R")
        {
            var go = new GameObject(n); go.transform.SetParent(p.transform, false);
            var r = go.AddComponent<UIPanel>(); r.W = w; r.H = h; r.root = p.Root;
            go.transform.localPosition = p.Center(x, y, w, h); return r;
        }
        public static UIImage Img(UIPanel p, float x, float y, float w, float h, Sprite s, Color c, float radius = 0)
        {
            var go = new GameObject("img"); go.transform.SetParent(p.transform, false);
            var im = go.AddComponent<UIImage>(); im.Init(p, x, y, w, h, s, c, radius); return im;
        }
        public static UIImage Icon(UIPanel p, Sprite s, float cx, float cy, float r, Color c) { return Img(p, cx - r, cy - r, r * 2, r * 2, s, c); }
        public static UIImage Round(UIPanel p, float x, float y, float w, float h, Color c, float radius) { return Img(p, x, y, w, h, Spr.Rounded, c, Mathf.Max(1, Mathf.Min(radius, Mathf.Min(w, h) / 2))); }
        public static void Framed(UIPanel p, float x, float y, float w, float h, Color fill, Color border, float radius, float t = 2.5f)
        {
            Round(p, x - t, y - t, w + 2 * t, h + 2 * t, border, radius + t);
            Round(p, x, y, w, h, fill, radius);
        }

        public enum Al { L, C, R }
        public static UIText T(UIPanel p, string s, float x, float baseline, float size, Color c, bool bold = false, Al al = Al.L, float w = 1400, bool italic = false)
        {
            var go = new GameObject("txt"); go.transform.SetParent(p.transform, false);
            var t = go.AddComponent<UIText>(); t.Init(p, s, x, baseline, size, c, bold, italic, al); return t;
        }
        // Párrafo con ajuste de línea; devuelve la línea base siguiente
        public static float Wrap(UIPanel p, string s, float x, float baseline, float maxW, float lineH, float size, Color c, bool bold = false, bool italic = false, Al al = Al.L)
        {
            var lines = new List<string>(); var cur = new StringBuilder();
            foreach (var word in s.Split(' '))
            {
                string test = cur.Length == 0 ? word : cur + " " + word;
                if (cur.Length > 0 && TextWidth(test, size, bold, italic) > maxW) { lines.Add(cur.ToString()); cur.Length = 0; cur.Append(word); }
                else { cur.Length = 0; cur.Append(test); }
            }
            if (cur.Length > 0) lines.Add(cur.ToString());
            var t = T(p, string.Join("\n", lines.ToArray()), al == Al.C ? x + maxW / 2 : x, baseline, size, c, bold, al, maxW, italic);
            t.lineSpacing = lineH / (size * 1.15f); // relativo al interlineado base (UIText lo adapta a la fuente)
            return baseline + lines.Count * lineH;
        }
        // Corta un texto en líneas que entren en maxW (devuelve el texto con saltos de línea)
        public static string WrapText(string s, float size, float maxW, bool bold, int maxLines = 99)
        {
            var lines = new List<string>(); var cur = new StringBuilder();
            foreach (var word in (s ?? "").Split(' '))
            {
                string test = cur.Length == 0 ? word : cur + " " + word;
                if (cur.Length > 0 && TextWidth(test, size, bold) > maxW) { lines.Add(cur.ToString()); cur.Length = 0; cur.Append(word); }
                else { cur.Length = 0; cur.Append(test); }
            }
            if (cur.Length > 0) lines.Add(cur.ToString());
            if (lines.Count > maxLines) { lines.RemoveRange(maxLines, lines.Count - maxLines); lines[maxLines - 1] += "…"; }
            return string.Join("\n", lines.ToArray());
        }
        // Ancho de un texto (en unidades de panel)
        public static float TextWidth(string s, float size, bool bold, bool italic = false) { return TextWidth(s, size, Brand.FontFor(bold, italic, size), bold); }
        public static float TextWidth(string s, float size, Font f, bool bold)
        {
            if (f == null) f = LegacyFont;
            var clean = StripTags(s); bool pop = Brand.IsPoppins(f);
            var style = pop ? FontStyle.Normal : bold ? FontStyle.Bold : FontStyle.Normal;
            f.RequestCharactersInTexture(clean, UIText.FontPx, style);
            float adv = 0; CharacterInfo ci;
            foreach (char ch in clean) if (f.GetCharacterInfo(ch, out ci, UIText.FontPx, style)) adv += ci.advance; else adv += UIText.FontPx * .5f;
            return adv * size / UIText.FontPx * (pop ? Brand.PoppinsScale : 1f);
        }
        static string StripTags(string s)
        {
            var sb = new StringBuilder(); bool tag = false;
            foreach (char ch in s) { if (ch == '<') tag = true; else if (ch == '>') tag = false; else if (!tag) sb.Append(ch); }
            return sb.ToString();
        }
        public static float Chip(UIPanel p, float x, float y, string label, Color col, float size = 19, float h = 38, float padX = 16, Color? text = null)
        {
            float w = TextWidth(label, size, true) + padX * 2;
            Framed(p, x, y, w, h, Pal.A(col, .16f), Pal.A(col, .6f), h / 2, 2);
            T(p, label, x + padX, y + h / 2 + size * .38f, size, text ?? col, true);
            return w;
        }
        public static void PanelBg(UIPanel p, Color accent)
        {
            float W = Wd(p), H = Ht(p);
            bool hc = Access.HighContrast;
            Round(p, 0, 0, W, H, hc ? new Color(1, 1, 1, .8f) : new Color(1, 1, 1, .16f), 36);
            Round(p, 3, 3, W - 6, H - 6, PanelA, 33);
            if (!hc) Img(p, W * .02f, 10, W * .5f, H * .45f, Spr.Glow, Pal.A(accent, .12f));
            // Marca de agua: la P de Piki en una esquina, como en las piezas del manual
            var iso = Brand.Isotipo; if (iso != null && !hc) { float h = Mathf.Min(H * .34f, 190); Brand.Draw(p, iso, W - 26, H - h - 22, h, new Color(1, 1, 1, .045f), Al.R); }
            Round(p, 40, 3, W - 80, 7, accent, 3);
        }
        public static void Stepper(UIPanel p, float x, float y, float w, int cur, Color accent)
        {
            string[] L = { "Física", "Nutrición", "Calma" }; float st = w / 3;
            for (int i = 0; i < 3; i++)
            {
                float cx = x + st * i + st / 2;
                if (i < 2) Round(p, cx + 20, y - 1.5f, st - 40, 3, i < cur ? Pal.Teal : new Color(1, 1, 1, .18f), 1.5f);
                bool done = i < cur, now = i == cur;
                if (done) { Icon(p, Spr.Circle, cx, y, 16, Pal.Teal); Icon(p, Spr.Check, cx, y, 10, Pal.Ink); }
                else if (now) { Icon(p, Spr.Circle, cx, y, 16, accent); Icon(p, Spr.Circle, cx, y, 12.5f, Pal.Card); T(p, (i + 1).ToString(), cx, y + 6, 16, Pal.Text, true, Al.C, 40); }
                else { Icon(p, Spr.Ring, cx, y, 16, Access.HighContrast ? new Color(1, 1, 1, .7f) : new Color(1, 1, 1, .3f)); T(p, (i + 1).ToString(), cx, y + 6, 15, Pal.Dim, true, Al.C, 40); }
                T(p, L[i], cx, y + 40, 16, done || now ? Pal.Text : Pal.Dim, true, Al.C, st);
            }
        }
        public static UIImage Bar(UIPanel p, float x, float y, float w, float h, float k, Color c, out UIImage track)
        {
            track = Round(p, x, y, w, h, new Color(1, 1, 1, .1f), h / 2);
            return Round(p, x, y, Mathf.Max(h, w * k), h, c, h / 2);
        }
        public static void SetBar(UIImage fill, float w, float h, float k) { fill.Size = new Vector2(Mathf.Max(h, w * Mathf.Clamp01(k)), h); }

        public static void Fade(UIPanel c, float a) { if (c != null) c.Alpha = a; }
        public static Coroutine Appear(PikiButton b, float delay = 0, float dur = .45f) { return b == null ? null : Appear(b.Panel, delay, dur); }
        public static Coroutine Appear(UIPanel c, float delay = 0, float dur = .45f)
        {
            if (!Application.isPlaying || c == null) return null; // en el editor se ve completo
            Fade(c, 0); var baseS = c.localScale;
            return Runner.I.StartCoroutine(AppearCo(c, delay, dur, baseS));
        }
        static System.Collections.IEnumerator AppearCo(UIPanel c, float delay, float dur, Vector3 baseS)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            yield return Tw.Co(dur, k => { if (c == null) return; Fade(c, k); var b = c.GetComponent<PikiButton>(); if (b == null) c.localScale = baseS * (.93f + .07f * k); }, Ease.Out);
        }
        public static void Vanish(Component c, float dur = .3f)
        {
            if (c == null) return;
            var col = c.GetComponent<Collider>(); if (col != null) col.enabled = false;
            var p = c.GetComponent<UIPanel>();
            if (p == null) { Build.Kill(c.gameObject); return; }
            Runner.I.StartCoroutine(VanishCo(p, dur));
        }
        static System.Collections.IEnumerator VanishCo(UIPanel c, float dur)
        {
            float a0 = c.Alpha;
            yield return Tw.Co(dur, k => { if (c != null) c.Alpha = a0 * (1 - k); });
            if (c != null) UnityEngine.Object.Destroy(c.gameObject);
        }

        /* --------------------------- Botones --------------------------- */
        public enum Style { Primary, Secondary, Warm }
        public static PikiButton Button(Transform parent, Vector3 pos, float wM, float hM, string label, Style style, Action onClick, Vector3 euler = default(Vector3))
        {
            var c = Canvas(parent, pos, wM, hM, euler, "Botón: " + label);
            float W = Wd(c), H = Ht(c), r = H / 2 - 4;
            Color fill = style == Style.Primary ? Pal.Hex("#22f0bf") : style == Style.Warm ? Pal.Hex("#ff9a4d") : Color.black;
            Color txt = style == Style.Secondary ? Pal.Text : style == Style.Warm ? Pal.Hex("#1a0c02") : Pal.Ink;
            Color bcol = style == Style.Secondary ? Color.black : new Color(1, 1, 1, .5f);
            var border = Round(c, 0, 0, W, H, bcol, r + 4);
            var bg = Round(c, 4, 4, W - 8, H - 8, fill, r);
            T(c, label, W / 2, H / 2 + H * .14f, Mathf.Round(H * .34f), txt, true, Al.C, W);
            var b = AttachButton(c);
            b.onClick = onClick;
            b.onHover = h => { border.color = h ? Color.white : bcol; bg.color = h ? Color.Lerp(fill, Color.white, .18f) : fill; };
            return b;
        }
        public static PikiButton AttachButton(UIPanel c, float depth = 30)
        {
            var col = c.gameObject.AddComponent<BoxCollider>(); col.size = new Vector3(Wd(c), Ht(c), depth);
            return c.gameObject.AddComponent<PikiButton>();
        }
    }


}
