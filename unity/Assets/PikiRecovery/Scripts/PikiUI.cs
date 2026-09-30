// =====================================================================
//  PIKI RECOVERY · Interfaz 3D (canvas en espacio de mundo).
//  1 unidad de canvas = 2,5 mm  →  un panel de 2,6 m mide 1040 unidades.
//  Los textos se posicionan por su línea base (y hacia abajo desde arriba).
// =====================================================================
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Piki
{
    public static class UI
    {
        public const float S = 0.0025f;
        static Font font;
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return font;
            }
        }
        public static readonly Color PanelA = new Color(.05f, .11f, .19f, .94f);

        public static RectTransform Canvas(Transform parent, Vector3 pos, float wM, float hM, Vector3 euler = default(Vector3), string name = "Panel", float unit = S, float dppu = 3f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.WorldSpace;
            var sc = go.AddComponent<CanvasScaler>(); sc.dynamicPixelsPerUnit = dppu;
            go.AddComponent<CanvasGroup>();
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(wM / unit, hM / unit); rt.localPosition = pos; rt.localEulerAngles = euler; rt.localScale = Vector3.one * unit;
            return rt;
        }
        public static float Wd(RectTransform c) { return c.sizeDelta.x; }
        public static float Ht(RectTransform c) { return c.sizeDelta.y; }

        public static RectTransform Rect(RectTransform p, float x, float y, float w, float h, string n = "R")
        {
            var go = new GameObject(n, typeof(RectTransform)); var rt = (RectTransform)go.transform; rt.SetParent(p, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y); rt.sizeDelta = new Vector2(w, h); return rt;
        }
        // radius = radio de las esquinas (en unidades) si el sprite es 9-slice
        public static Image Img(RectTransform p, float x, float y, float w, float h, Sprite s, Color c, float radius = 0)
        {
            var rt = Rect(p, x, y, w, h, "img"); var im = rt.gameObject.AddComponent<Image>(); im.sprite = s; im.color = c; im.raycastTarget = false;
            if (s != null && s.border.x > 0) { im.type = Image.Type.Sliced; im.pixelsPerUnitMultiplier = radius > 0 ? 48f / radius : 1.6f; }
            else im.preserveAspect = true;
            return im;
        }
        public static Image Icon(RectTransform p, Sprite s, float cx, float cy, float r, Color c) { return Img(p, cx - r, cy - r, r * 2, r * 2, s, c); }
        public static Image Round(RectTransform p, float x, float y, float w, float h, Color c, float radius) { return Img(p, x, y, w, h, Spr.Rounded, c, Mathf.Min(radius, Mathf.Min(w, h) / 2)); }
        public static void Framed(RectTransform p, float x, float y, float w, float h, Color fill, Color border, float radius, float t = 2.5f)
        {
            Round(p, x - t, y - t, w + 2 * t, h + 2 * t, border, radius + t);
            Round(p, x, y, w, h, fill, radius);
        }

        public enum Al { L, C, R }
        public static Text T(RectTransform p, string s, float x, float baseline, float size, Color c, bool bold = false, Al al = Al.L, float w = 1400, bool italic = false)
        {
            float rx = al == Al.L ? x : al == Al.C ? x - w / 2 : x - w;
            var rt = Rect(p, rx, baseline - size * 1.02f, w, size * 1.4f, "txt");
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font; t.fontSize = Mathf.RoundToInt(size); t.color = c; t.text = s; t.supportRichText = true; t.raycastTarget = false;
            t.fontStyle = bold ? (italic ? FontStyle.BoldAndItalic : FontStyle.Bold) : (italic ? FontStyle.Italic : FontStyle.Normal);
            t.alignment = al == Al.L ? TextAnchor.UpperLeft : al == Al.C ? TextAnchor.UpperCenter : TextAnchor.UpperRight;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
        // Párrafo con ajuste de línea; devuelve la línea base siguiente
        public static float Wrap(RectTransform p, string s, float x, float baseline, float maxW, float lineH, float size, Color c, bool bold = false, bool italic = false, Al al = Al.L)
        {
            var t = T(p, s, al == Al.C ? x + maxW / 2 : x, baseline, size, c, bold, al, maxW, italic);
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.lineSpacing = lineH / (size * 1.15f);
            float h = t.preferredHeight; t.rectTransform.sizeDelta = new Vector2(maxW, h + 8);
            int lines = Mathf.Max(1, Mathf.RoundToInt(h / (size * 1.15f * t.lineSpacing)));
            return baseline + lines * lineH;
        }
        public static float TextWidth(string s, float size, bool bold)
        {
            var go = new GameObject("measure", typeof(RectTransform)); var t = go.AddComponent<Text>();
            t.font = Font; t.fontSize = Mathf.RoundToInt(size); t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; t.horizontalOverflow = HorizontalWrapMode.Overflow; t.text = s;
            float w = t.preferredWidth; Build.Kill(go); return w;
        }
        public static float Chip(RectTransform p, float x, float y, string label, Color col, float size = 19, float h = 38, float padX = 16, Color? text = null)
        {
            float w = TextWidth(label, size, true) + padX * 2;
            Framed(p, x, y, w, h, Pal.A(col, .16f), Pal.A(col, .6f), h / 2, 2);
            T(p, label, x + padX, y + h / 2 + size * .38f, size, text ?? col, true);
            return w;
        }
        public static void PanelBg(RectTransform p, Color accent)
        {
            float W = Wd(p), H = Ht(p);
            Round(p, 0, 0, W, H, new Color(1, 1, 1, .16f), 36);
            Round(p, 3, 3, W - 6, H - 6, PanelA, 33);
            var g = Img(p, -W * .25f, -W * .35f, W * .9f, W * .7f, Spr.Glow, Pal.A(accent, .16f));
            g.preserveAspect = false;
            Round(p, 40, 3, W - 80, 7, accent, 3);
        }
        public static void Stepper(RectTransform p, float x, float y, float w, int cur, Color accent)
        {
            string[] L = { "Física", "Nutrición", "Calma" }; float st = w / 3;
            for (int i = 0; i < 3; i++)
            {
                float cx = x + st * i + st / 2;
                if (i < 2) Round(p, cx + 20, y - 1.5f, st - 40, 3, i < cur ? Pal.Teal : new Color(1, 1, 1, .18f), 1.5f);
                bool done = i < cur, now = i == cur;
                if (done) { Icon(p, Spr.Circle, cx, y, 16, Pal.Teal); Icon(p, Spr.Check, cx, y, 10, Pal.Ink); }
                else if (now) { Icon(p, Spr.Circle, cx, y, 16, accent); Icon(p, Spr.Circle, cx, y, 12.5f, new Color(.06f, .12f, .2f, 1)); T(p, (i + 1).ToString(), cx, y + 6, 16, Pal.Text, true, Al.C, 40); }
                else { Icon(p, Spr.Ring, cx, y, 16, new Color(1, 1, 1, .3f)); T(p, (i + 1).ToString(), cx, y + 6, 15, Pal.Dim, true, Al.C, 40); }
                T(p, L[i], cx, y + 40, 16, done || now ? Pal.Text : Pal.Dim, true, Al.C, st);
            }
        }
        public static Image Bar(RectTransform p, float x, float y, float w, float h, float k, Color c, out Image track)
        {
            track = Round(p, x, y, w, h, new Color(1, 1, 1, .1f), h / 2);
            var fill = Round(p, x, y, Mathf.Max(h, w * k), h, c, h / 2);
            return fill;
        }
        public static void SetBar(Image fill, float w, float h, float k) { fill.rectTransform.sizeDelta = new Vector2(Mathf.Max(h, w * Mathf.Clamp01(k)), h); }

        public static void Fade(RectTransform c, float a) { var g = c.GetComponent<CanvasGroup>(); if (g != null) g.alpha = a; }
        public static Coroutine Appear(RectTransform c, float delay = 0, float dur = .45f)
        {
            if (!Application.isPlaying) return null; // en el editor se ve completo
            Fade(c, 0); var baseS = c.localScale;
            return Runner.I.StartCoroutine(AppearCo(c, delay, dur, baseS));
        }
        static System.Collections.IEnumerator AppearCo(RectTransform c, float delay, float dur, Vector3 baseS)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            yield return Tw.Co(dur, k => { if (c == null) return; Fade(c, k); var b = c.GetComponent<PikiButton>(); if (b == null) c.localScale = baseS * (.93f + .07f * k); }, Ease.Out);
        }
        public static void Vanish(Component c, float dur = .3f)
        {
            if (c == null) return; var rt = c.transform as RectTransform;
            var col = c.GetComponent<Collider>(); if (col != null) col.enabled = false;
            if (rt == null) { UnityEngine.Object.Destroy(c.gameObject); return; }
            Runner.I.StartCoroutine(VanishCo(rt, dur));
        }
        static System.Collections.IEnumerator VanishCo(RectTransform c, float dur)
        {
            var g = c.GetComponent<CanvasGroup>(); float a0 = g != null ? g.alpha : 1;
            yield return Tw.Co(dur, k => { if (c != null && g != null) g.alpha = a0 * (1 - k); });
            if (c != null) UnityEngine.Object.Destroy(c.gameObject);
        }

        /* --------------------------- Botones --------------------------- */
        public enum Style { Primary, Secondary, Warm }
        public static PikiButton Button(Transform parent, Vector3 pos, float wM, float hM, string label, Style style, Action onClick, Vector3 euler = default(Vector3))
        {
            var c = Canvas(parent, pos, wM, hM, euler, "Button: " + label);
            float W = Wd(c), H = Ht(c), r = H / 2 - 4;
            Color fill = style == Style.Primary ? Pal.Hex("#22f0bf") : style == Style.Warm ? Pal.Hex("#ff9a4d") : new Color(1, 1, 1, .09f);
            Color txt = style == Style.Secondary ? Pal.Text : style == Style.Warm ? Pal.Hex("#1a0c02") : Pal.Ink;
            var border = Round(c, 0, 0, W, H, style == Style.Secondary ? new Color(1, 1, 1, .45f) : new Color(1, 1, 1, .5f), r + 4);
            var bg = Round(c, 4, 4, W - 8, H - 8, fill, r);
            T(c, label, W / 2, H / 2 + H * .14f, Mathf.Round(H * .34f), txt, true, Al.C, W);
            var b = AttachButton(c);
            b.onClick = onClick;
            b.onHover = h => { border.color = h ? Color.white : (style == Style.Secondary ? new Color(1, 1, 1, .45f) : new Color(1, 1, 1, .5f)); bg.color = h ? Color.Lerp(fill, Color.white, .18f) : fill; };
            return b;
        }
        public static PikiButton AttachButton(RectTransform c, float depth = 30)
        {
            var col = c.gameObject.AddComponent<BoxCollider>(); col.size = new Vector3(Wd(c), Ht(c), depth);
            return c.gameObject.AddComponent<PikiButton>();
        }
    }

    public class PikiButton : MonoBehaviour
    {
        public Action onClick; public Action<bool> onHover; public float hoverScale = 1.06f;
        public bool Disabled { get; private set; }
        public bool Hovered { get; private set; }
        static float lastClick;
        Vector3 baseScale; float target = 1;
        void Awake() { baseScale = transform.localScale; }
        public void SetHover(bool h)
        {
            if (Disabled) h = false; if (h == Hovered) return; Hovered = h; target = h ? hoverScale : 1;
            if (onHover != null) onHover(h); if (h && PikiAudio.I != null) PikiAudio.I.Hover();
        }
        public void Click()
        {
            if (Disabled || !isActiveAndEnabled) return;
            if (Time.unscaledTime - lastClick < .35f) return; lastClick = Time.unscaledTime;
            if (PikiAudio.I != null) PikiAudio.I.Click();
            if (onClick != null) onClick();
        }
        public void SetDisabled(bool d, float alpha = .35f)
        {
            Disabled = d; if (d) SetHover(false);
            var col = GetComponent<Collider>(); if (col != null) col.enabled = !d;
            var g = GetComponent<CanvasGroup>(); if (g != null && alpha >= 0) g.alpha = d ? alpha : 1;
        }
        public void Shake()
        {
            float x0 = transform.localPosition.x;
            StartCoroutine(Tw.Co(.4f, k => { var p = transform.localPosition; p.x = x0 + Mathf.Sin(k * Mathf.PI * 6) * .022f * (1 - k); transform.localPosition = p; }, Ease.Lin));
        }
        void Update() { transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, 1 - Mathf.Exp(-Time.deltaTime * 16)); }
    }
}
