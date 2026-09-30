// =====================================================================
//  PIKI RECOVERY · Interfaz 3D sin paquetes extra.
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
    /* ------------------------------ Panel ------------------------------ */
    public class UIPanel : MonoBehaviour
    {
        public float W, H; public UIPanel root; public int baseOrder, queue;
        [SerializeField] int counter; [SerializeField] float alpha = 1;
        public UIPanel Root { get { return root != null ? root : this; } }
        public int NextOrder() { var r = Root; return r.baseOrder + (r.counter++); }
        public int Queue { get { return Root.queue; } }
        public float AlphaSelf { get { return alpha; } }
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; foreach (var e in GetComponentsInChildren<UIElem>(true)) e.Refresh(); }
        }
        // atajos para tratar al panel como un Transform
        public Vector3 localPosition { get { return transform.localPosition; } set { transform.localPosition = value; } }
        public Quaternion localRotation { get { return transform.localRotation; } set { transform.localRotation = value; } }
        public Vector3 localScale { get { return transform.localScale; } set { transform.localScale = value; } }
        public Vector3 localEulerAngles { get { return transform.localEulerAngles; } set { transform.localEulerAngles = value; } }
        // Posición local (centro) de un rectángulo dado en coordenadas de panel
        public Vector3 Center(float x, float y, float w, float h) { return new Vector3(x + w / 2 - W / 2, H / 2 - y - h / 2, 0); }
    }

    public abstract class UIElem : MonoBehaviour
    {
        public UIPanel panel; [SerializeField] protected Color baseColor = Color.white;
        protected float ChainAlpha()
        {
            float a = 1; var t = transform;
            while (t != null) { var p = t.GetComponent<UIPanel>(); if (p != null) a *= p.AlphaSelf; t = t.parent; }
            return a;
        }
        public abstract void Refresh();
    }

    /* ------------------------------ Imagen ------------------------------ */
    public class UIImage : UIElem
    {
        [SerializeField] SpriteRenderer sr; [SerializeField] float x, y, w, h, radius; [SerializeField] bool sliced;
        public Color color { get { return baseColor; } set { baseColor = value; Refresh(); } }
        public Vector2 Size { get { return new Vector2(w, h); } set { w = value.x; h = value.y; Layout(); } }
        public void Init(UIPanel p, float x, float y, float w, float h, Sprite s, Color c, float radius)
        {
            panel = p; this.x = x; this.y = y; this.w = w; this.h = h; baseColor = c;
            sr = gameObject.AddComponent<SpriteRenderer>(); sr.sprite = s != null ? s : Spr.White;
            sr.sharedMaterial = UI.SpriteMat(p.Queue); sr.sortingOrder = p.NextOrder();
            sliced = sr.sprite.border.x > 0; this.radius = radius > 0 ? radius : 30;
            Layout(); Refresh();
        }
        void Layout()
        {
            transform.localPosition = panel.Center(x, y, w, h);
            if (sliced)
            {
                float r = Mathf.Min(radius, Mathf.Min(w, h) / 2); float m = Mathf.Max(.001f, r * 2.083f);
                sr.drawMode = SpriteDrawMode.Sliced; transform.localScale = Vector3.one * m; sr.size = new Vector2(Mathf.Max(.01f, w / m), Mathf.Max(.01f, h / m));
            }
            else
            {
                var n = sr.sprite.bounds.size; sr.drawMode = SpriteDrawMode.Simple;
                transform.localScale = new Vector3(Mathf.Max(.0001f, w / n.x), Mathf.Max(.0001f, h / n.y), 1);
            }
        }
        public override void Refresh() { if (sr == null) return; var c = baseColor; c.a *= ChainAlpha(); sr.color = c; }
    }

    /* ------------------------------ Texto ------------------------------ */
    public class UIText : UIElem
    {
        public const int FontPx = 64; const float Unit = .1f; // TextMesh: 10 px de fuente = 1 unidad
        [SerializeField] TextMesh tm; [SerializeField] float x, baseline, size;
        public string text { get { return tm.text; } set { tm.text = value; } }
        public Color color { get { return baseColor; } set { baseColor = value; Refresh(); } }
        public float fontSize { get { return size; } set { size = value; Layout(); } }
        public float lineSpacing { get { return tm.lineSpacing; } set { tm.lineSpacing = value; } }
        public void Init(UIPanel p, string s, float x, float baseline, float size, Color c, bool bold, bool italic, UI.Al al)
        {
            panel = p; this.x = x; this.baseline = baseline; this.size = size; baseColor = c;
            tm = gameObject.AddComponent<TextMesh>(); tm.font = UI.Font; tm.fontSize = FontPx; tm.richText = true; tm.text = s;
            tm.fontStyle = bold ? (italic ? FontStyle.BoldAndItalic : FontStyle.Bold) : (italic ? FontStyle.Italic : FontStyle.Normal);
            tm.anchor = al == UI.Al.L ? TextAnchor.UpperLeft : al == UI.Al.C ? TextAnchor.UpperCenter : TextAnchor.UpperRight;
            tm.alignment = al == UI.Al.L ? TextAlignment.Left : al == UI.Al.C ? TextAlignment.Center : TextAlignment.Right;
            var mr = GetComponent<MeshRenderer>(); mr.sharedMaterial = UI.TextMat(p.Queue); mr.sortingOrder = p.NextOrder();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            Layout(); Refresh();
        }
        void Layout()
        {
            tm.characterSize = size / (FontPx * Unit);
            float top = baseline - size * .95f;
            transform.localPosition = new Vector3(x - panel.W / 2, panel.H / 2 - top, 0);
        }
        // Achica el texto si no entra en el ancho indicado
        public void FitWidth(float maxW)
        {
            float wNow = UI.TextWidth(tm.text, size, tm.fontStyle == FontStyle.Bold || tm.fontStyle == FontStyle.BoldAndItalic);
            if (wNow > maxW && wNow > 0) fontSize = size * maxW / wNow;
        }
        public override void Refresh() { if (tm == null) return; var c = baseColor; c.a *= ChainAlpha(); tm.color = c; }
    }

    /* ------------------------------ Fábrica de interfaz ------------------------------ */
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

        static readonly Dictionary<int, Material> spriteMats = new Dictionary<int, Material>(), textMats = new Dictionary<int, Material>();
        public static void ClearCache() { spriteMats.Clear(); textMats.Clear(); }
        public static Material SpriteMat(int queue)
        {
            Material m; if (spriteMats.TryGetValue(queue, out m) && m != null) return m;
            m = Mat.Unlit(Color.white, null, queue > 0 ? queue : 3000); spriteMats[queue] = m; return m;
        }
        public static Material TextMat(int queue)
        {
            Material m; if (textMats.TryGetValue(queue, out m) && m != null) return m;
            m = new Material(Font.material); if (queue > 0) m.renderQueue = queue;
            textMats[queue] = Persist.Keep(m, "Texto"); return m;
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
                if (cur.Length > 0 && TextWidth(test, size, bold) > maxW) { lines.Add(cur.ToString()); cur.Length = 0; cur.Append(word); }
                else { cur.Length = 0; cur.Append(test); }
            }
            if (cur.Length > 0) lines.Add(cur.ToString());
            var t = T(p, string.Join("\n", lines.ToArray()), al == Al.C ? x + maxW / 2 : x, baseline, size, c, bold, al, maxW, italic);
            t.lineSpacing = lineH / (size * 1.15f);
            return baseline + lines.Count * lineH;
        }
        // Ancho de un texto (en unidades de panel)
        public static float TextWidth(string s, float size, bool bold)
        {
            var clean = StripTags(s); var style = bold ? FontStyle.Bold : FontStyle.Normal;
            Font.RequestCharactersInTexture(clean, UIText.FontPx, style);
            float adv = 0; CharacterInfo ci;
            foreach (char ch in clean) if (Font.GetCharacterInfo(ch, out ci, UIText.FontPx, style)) adv += ci.advance; else adv += UIText.FontPx * .5f;
            return adv * size / UIText.FontPx;
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
            Round(p, 0, 0, W, H, new Color(1, 1, 1, .16f), 36);
            Round(p, 3, 3, W - 6, H - 6, PanelA, 33);
            Img(p, W * .02f, 10, W * .5f, H * .45f, Spr.Glow, Pal.A(accent, .12f));
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
                else if (now) { Icon(p, Spr.Circle, cx, y, 16, accent); Icon(p, Spr.Circle, cx, y, 12.5f, new Color(.06f, .12f, .2f, 1)); T(p, (i + 1).ToString(), cx, y + 6, 16, Pal.Text, true, Al.C, 40); }
                else { Icon(p, Spr.Ring, cx, y, 16, new Color(1, 1, 1, .3f)); T(p, (i + 1).ToString(), cx, y + 6, 15, Pal.Dim, true, Al.C, 40); }
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
            Color fill = style == Style.Primary ? Pal.Hex("#22f0bf") : style == Style.Warm ? Pal.Hex("#ff9a4d") : new Color(1, 1, 1, .09f);
            Color txt = style == Style.Secondary ? Pal.Text : style == Style.Warm ? Pal.Hex("#1a0c02") : Pal.Ink;
            Color bcol = style == Style.Secondary ? new Color(1, 1, 1, .45f) : new Color(1, 1, 1, .5f);
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

    /* ------------------------------ Botón ------------------------------ */
    public class PikiButton : MonoBehaviour
    {
        public Action onClick; public Action<bool> onHover; public float hoverScale = 1.06f;
        public bool Disabled { get; private set; }
        public bool Hovered { get; private set; }
        public UIPanel Panel { get { return GetComponent<UIPanel>(); } }
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
            var p = Panel; if (p != null && alpha >= 0) p.Alpha = d ? alpha : 1;
        }
        public void Shake()
        {
            float x0 = transform.localPosition.x;
            StartCoroutine(Tw.Co(.4f, k => { var p = transform.localPosition; p.x = x0 + Mathf.Sin(k * Mathf.PI * 6) * .022f * (1 - k); transform.localPosition = p; }, Ease.Lin));
        }
        void Update() { transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, 1 - Mathf.Exp(-Time.deltaTime * 16)); }
    }

    /* ------------------------------ Textos en pantalla (PC / celular) ------------------------------ */
    public class PikiHud : MonoBehaviour
    {
        public string hint = ""; public bool hidden;
        GUIStyle big, small, hintSt; Texture2D bg;
        void OnGUI()
        {
            if (hidden) return;
            float s = Mathf.Max(.6f, Screen.height / 720f);
            if (big == null)
            {
                bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(.02f, .04f, .09f, .75f)); bg.Apply();
                big = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold }; big.normal.textColor = Pal.Text;
                small = new GUIStyle(GUI.skin.label); small.normal.textColor = Pal.Muted;
                hintSt = new GUIStyle(GUI.skin.box) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true }; hintSt.normal.textColor = Pal.Text; hintSt.normal.background = bg;
            }
            big.fontSize = Mathf.RoundToInt(15 * s); small.fontSize = Mathf.RoundToInt(12 * s); hintSt.fontSize = Mathf.RoundToInt(14 * s);
            GUI.Label(new Rect(16 * s, 10 * s, 400 * s, 24 * s), "PIKI RECOVERY", big);
            GUI.Label(new Rect(16 * s, 30 * s, 400 * s, 20 * s), "Fútbol · El trabajo invisible", small);
            var help = new GUIStyle(small) { alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width - 436 * s, 12 * s, 420 * s, 20 * s), "M: sonido · R: recentrar · flechas: mirar", help);
            if (!string.IsNullOrEmpty(hint))
            {
                float w = Mathf.Min(Screen.width - 32, 900 * s), h = 44 * s;
                GUI.Box(new Rect((Screen.width - w) / 2, Screen.height - h - 18 * s, w, h), hint, hintSt);
            }
        }
    }
}
