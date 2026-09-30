// =====================================================================
//  PIKI RECOVERY · Núcleo: colores, materiales, texturas, sprites,
//  mallas procedurales y tweens. Todo se genera por código: el proyecto
//  no necesita modelos, imágenes ni sonidos importados.
// =====================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    /* ------------------------------------------------------------------
       Persistencia: cuando el constructor de escenas (menú Piki Recovery)
       está activo, cada textura, material, sprite y malla que se genera se
       guarda como asset para que quede en la escena.
       ------------------------------------------------------------------ */
    public static class Persist
    {
        public static Action<UnityEngine.Object, string> Save;
        public static T Keep<T>(T o, string name) where T : UnityEngine.Object { if (Save != null && o != null) Save(o, name); return o; }
        public static bool Baking { get { return Save != null; } }
    }

    public static class Ease
    {
        public static float InOut(float t) { return t < .5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2; }
        public static float Out(float t) { return 1 - Mathf.Pow(1 - t, 3); }
        public static float In(float t) { return t * t * t; }
        public static float Lin(float t) { return t; }
        public static float Sine(float t) { return -(Mathf.Cos(Mathf.PI * t) - 1) / 2; }
    }

    public static class Pal
    {
        public static readonly Color Teal = Hex("#19e3b1"), Cyan = Hex("#39d5ff"), Orange = Hex("#ff8a3d"), Yellow = Hex("#ffd93d"),
            Red = Hex("#ff4d5e"), Green = Hex("#3ddc97"), Purple = Hex("#b388ff"), Blue = Hex("#4fc3ff"), Text = Hex("#eaf2fb"),
            Muted = Hex("#9fb3c8"), Dim = Hex("#6f8396"), Ink = Hex("#04131f"), Body = Hex("#c9d6e6"), Ok = Hex("#2ecc71");
        public static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString(h, out c); return c; }
        public static Color A(Color c, float a) { c.a = a; return c; }
        public static string Tag(Color c) { return ColorUtility.ToHtmlStringRGB(c); }
    }

    /* ------------------------------ Materiales ------------------------------ */
    public static class Mat
    {
        static Shader lit, unlit;
        public static bool URP { get { return GraphicsSettings.currentRenderPipeline != null; } }
        public static Shader LitShader
        {
            get
            {
                if (lit == null)
                {
                    lit = URP ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
                    if (lit == null) lit = Shader.Find("Standard");
                }
                return lit;
            }
        }
        // "Sprites/Default" es un shader integrado sin iluminación que funciona en Built-in y en URP.
        public static Shader UnlitShader { get { if (unlit == null) unlit = Shader.Find("Sprites/Default"); return unlit; } }

        public static Material Lit(Color c, float smooth = .25f, float metal = 0f, Texture tex = null)
        {
            var m = new Material(LitShader); m.color = c;
            if (tex != null) m.mainTexture = tex;
            m.SetFloat("_Smoothness", smooth); m.SetFloat("_Glossiness", smooth); m.SetFloat("_Metallic", metal);
            return Persist.Keep(m, "Lit_" + ColorUtility.ToHtmlStringRGB(c));
        }
        public static Material Emissive(Color c, Color em, Texture tex = null)
        {
            var m = Lit(c, .3f, 0, tex);
            m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", em);
            if (tex != null) m.SetTexture("_EmissionMap", tex);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            return m;
        }
        public static Material Unlit(Color c, Texture tex = null, int queue = 3000)
        {
            var m = new Material(UnlitShader); m.color = c;
            if (tex != null) m.mainTexture = tex;
            m.renderQueue = queue; return Persist.Keep(m, "Unlit_" + ColorUtility.ToHtmlStringRGB(c));
        }
    }

    /* ------------------------------ Texturas ------------------------------ */
    public class Px
    {
        public readonly int W, H; public readonly Color32[] P;
        public Px(int w, int h, Color32 fill) { W = w; H = h; P = new Color32[w * h]; for (int i = 0; i < P.Length; i++) P[i] = fill; }
        public void Set(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            int i = y * W + x; if (c.a >= .999f) { P[i] = c; return; }
            Color b = P[i]; P[i] = Color.Lerp(b, new Color(c.r, c.g, c.b, 1), c.a);
        }
        public void Rect(float x, float y, float w, float h, Color c)
        {
            int x0 = Mathf.Max(0, Mathf.RoundToInt(x)), y0 = Mathf.Max(0, Mathf.RoundToInt(y));
            int x1 = Mathf.Min(W, Mathf.RoundToInt(x + w)), y1 = Mathf.Min(H, Mathf.RoundToInt(y + h));
            for (int yy = y0; yy < y1; yy++) for (int xx = x0; xx < x1; xx++) Set(xx, yy, c);
        }
        public void RectOutline(float x, float y, float w, float h, float t, Color c)
        { Rect(x - t / 2, y - t / 2, w + t, t, c); Rect(x - t / 2, y + h - t / 2, w + t, t, c); Rect(x - t / 2, y, t, h, c); Rect(x + w - t / 2, y, t, h, c); }
        public void Disc(float cx, float cy, float r, Color c)
        {
            for (int y = Mathf.FloorToInt(cy - r - 1); y <= cy + r + 1; y++) for (int x = Mathf.FloorToInt(cx - r - 1); x <= cx + r + 1; x++)
                { float d = Mathf.Sqrt((x + .5f - cx) * (x + .5f - cx) + (y + .5f - cy) * (y + .5f - cy)); float a = Mathf.Clamp01(r - d + .5f); if (a > 0) Set(x, y, new Color(c.r, c.g, c.b, c.a * a)); }
        }
        // Circunferencia (o arco) de grosor t; filtro opcional por punto
        public void Circle(float cx, float cy, float r, float t, Color c, Func<float, float, bool> keep = null)
        {
            for (int y = Mathf.FloorToInt(cy - r - t); y <= cy + r + t; y++) for (int x = Mathf.FloorToInt(cx - r - t); x <= cx + r + t; x++)
            {
                float px = x + .5f, py = y + .5f; float d = Mathf.Abs(Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy)) - r);
                float a = Mathf.Clamp01(t / 2 - d + .5f); if (a <= 0) continue; if (keep != null && !keep(px, py)) continue; Set(x, y, new Color(c.r, c.g, c.b, c.a * a));
            }
        }
        public Texture2D ToTex(bool mip = true, TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            var t = new Texture2D(W, H, TextureFormat.RGBA32, mip, false); t.wrapMode = wrap; t.filterMode = FilterMode.Trilinear; t.anisoLevel = 8;
            t.SetPixels32(P); t.Apply(mip, false); return Persist.Keep(t, "Tex_" + W + "x" + H);
        }
    }

    public static class Tex
    {
        // f(u,v) con u,v en [0,1] (v hacia arriba); supersampling opcional
        public static Texture2D Func(int w, int h, Func<float, float, Color> f, int ss = 1, TextureWrapMode wrap = TextureWrapMode.Clamp, bool mip = true)
        {
            var px = new Color[w * h]; float inv = 1f / (ss * ss);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                {
                    Color acc = new Color(0, 0, 0, 0);
                    for (int sy = 0; sy < ss; sy++) for (int sx = 0; sx < ss; sx++)
                        {
                            Color c = f((x + (sx + .5f) / ss) / w, (y + (sy + .5f) / ss) / h);
                            acc.r += c.r * c.a; acc.g += c.g * c.a; acc.b += c.b * c.a; acc.a += c.a;
                        }
                    Color o = acc.a > 0 ? new Color(acc.r / acc.a, acc.g / acc.a, acc.b / acc.a, acc.a * inv) : new Color(1, 1, 1, 0);
                    px[y * w + x] = o;
                }
            var t = new Texture2D(w, h, TextureFormat.RGBA32, mip, false); t.wrapMode = wrap; t.filterMode = FilterMode.Trilinear; t.anisoLevel = 4;
            t.SetPixels(px); t.Apply(mip, false); return Persist.Keep(t, "Tex_" + w + "x" + h);
        }
        public static Texture2D Gradient(Color[] stops, float[] at, int h = 256)
        {
            return Func(2, h, (u, v) =>
            {
                for (int i = 0; i < at.Length - 1; i++) if (v <= at[i + 1]) return Color.Lerp(stops[i], stops[i + 1], Mathf.InverseLerp(at[i], at[i + 1], v));
                return stops[stops.Length - 1];
            }, 1, TextureWrapMode.Clamp, false);
        }
        static Texture2D glow;
        public static void ClearCache() { glow = null; }
        public static Texture2D Glow { get { if (glow == null) glow = Func(128, 128, (u, v) => { float r = Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), new Vector2(.5f, .5f)) * 2); float a = Mathf.Pow(1 - r, 2.2f); return new Color(1, 1, 1, a); }); return glow; } }
    }

    /* ------------------------------ Sprites (íconos) ------------------------------ */
    public static class Spr
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        public static void ClearCache() { cache.Clear(); }
        static Sprite Make(string key, int size, Func<float, float, Color> f, int ss = 3, float border = 0)
        {
            Sprite s; if (cache.TryGetValue(key, out s) && s != null) return s;
            var tex = Tex.Func(size, size, (u, v) => f(u * 2 - 1, v * 2 - 1), ss, TextureWrapMode.Clamp, false);
            tex.filterMode = FilterMode.Bilinear;
            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            s.name = key; cache[key] = Persist.Keep(s, "Sprite_" + key); return s;
        }
        static Color W(bool b) { return b ? Color.white : new Color(1, 1, 1, 0); }
        static bool Circ(float u, float v, float cx, float cy, float r) { return (u - cx) * (u - cx) + (v - cy) * (v - cy) <= r * r; }
        static bool Seg(float u, float v, float ax, float ay, float bx, float by, float r)
        {
            float dx = bx - ax, dy = by - ay; float t = Mathf.Clamp01(((u - ax) * dx + (v - ay) * dy) / (dx * dx + dy * dy));
            float px = ax + dx * t - u, py = ay + dy * t - v; return px * px + py * py <= r * r;
        }
        static bool RR(float u, float v, float cx, float cy, float hw, float hh, float r)
        {
            float qx = Mathf.Abs(u - cx) - (hw - r), qy = Mathf.Abs(v - cy) - (hh - r);
            float o = Mathf.Sqrt(Mathf.Max(qx, 0) * Mathf.Max(qx, 0) + Mathf.Max(qy, 0) * Mathf.Max(qy, 0)) + Mathf.Min(Mathf.Max(qx, qy), 0) - r; return o <= 0;
        }
        static bool Poly(float u, float v, float[] p)
        {
            bool inside = false; int n = p.Length / 2;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                float xi = p[i * 2], yi = p[i * 2 + 1], xj = p[j * 2], yj = p[j * 2 + 1];
                if (((yi > v) != (yj > v)) && (u < (xj - xi) * (v - yi) / (yj - yi) + xi)) inside = !inside;
            }
            return inside;
        }
        static float[] StarPts(float ro, float ri, int n, float rot)
        {
            var p = new float[n * 4];
            for (int i = 0; i < n * 2; i++) { float a = rot + i * Mathf.PI / n; float r = i % 2 == 0 ? ro : ri; p[i * 2] = Mathf.Cos(a) * r; p[i * 2 + 1] = Mathf.Sin(a) * r; }
            return p;
        }
        static float[] Pent(float cx, float cy, float r, float rot)
        {
            var p = new float[10]; for (int i = 0; i < 5; i++) { float a = rot + i * Mathf.PI * 2 / 5; p[i * 2] = cx + Mathf.Cos(a) * r; p[i * 2 + 1] = cy + Mathf.Sin(a) * r; }
            return p;
        }

        // 9-slice: esquinas de 48 px de radio (borde 52)
        public static Sprite Rounded { get { return Make("rounded", 128, (u, v) => W(RR(u, v, 0, 0, 1, 1, .75f)), 3, 52); } }
        public static Sprite Circle { get { return Make("circle", 128, (u, v) => W(u * u + v * v <= .94f)); } }
        public static Sprite Ring { get { return Make("ring", 256, (u, v) => { float r = Mathf.Sqrt(u * u + v * v); return W(r > .86f && r < .97f); }); } }
        public static Sprite Glow { get { return Make("glow", 128, (u, v) => { float r = Mathf.Clamp01(Mathf.Sqrt(u * u + v * v)); return new Color(1, 1, 1, Mathf.Pow(1 - r, 2)); }, 1); } }
        public static Sprite Check { get { return Make("check", 96, (u, v) => W(Seg(u, v, -.62f, .02f, -.16f, -.46f, .15f) || Seg(u, v, -.16f, -.46f, .66f, .5f, .15f))); } }
        public static Sprite Cross { get { return Make("cross", 96, (u, v) => W(Seg(u, v, -.56f, -.56f, .56f, .56f, .15f) || Seg(u, v, -.56f, .56f, .56f, -.56f, .15f))); } }
        public static Sprite Star { get { var p = StarPts(.95f, .42f, 5, Mathf.PI / 2); return Make("star", 96, (u, v) => W(Poly(u, v, p))); } }
        public static Sprite Snow
        {
            get
            {
                return Make("snow", 128, (u, v) =>
                {
                    if (u * u + v * v < .02f) return Color.white;
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i * Mathf.PI / 3, ex = Mathf.Cos(a) * .9f, ey = Mathf.Sin(a) * .9f;
                        if (Seg(u, v, 0, 0, ex, ey, .07f)) return Color.white;
                        float bx = Mathf.Cos(a) * .55f, by = Mathf.Sin(a) * .55f;
                        for (int s = -1; s <= 1; s += 2) { float b = a + s * .75f; if (Seg(u, v, bx, by, bx + Mathf.Cos(b) * .3f, by + Mathf.Sin(b) * .3f, .06f)) return Color.white; }
                    }
                    return W(false);
                });
            }
        }
        public static Sprite Flame { get { var tri = new float[] { 0, .95f, .5f, -.12f, -.5f, -.12f }; return Make("flame", 128, (u, v) => W(Circ(u, v, 0, -.32f, .55f) || Poly(u, v, tri))); } }
        public static Sprite Drop { get { var tri = new float[] { 0, .95f, .47f, -.05f, -.47f, -.05f }; return Make("drop", 128, (u, v) => W(Circ(u, v, 0, -.3f, .55f) || Poly(u, v, tri))); } }
        public static Sprite Bolt { get { var p = new float[] { .18f, .95f, -.52f, -.08f, -.02f, -.08f, -.2f, -.95f, .52f, .12f, .02f, .12f }; return Make("bolt", 128, (u, v) => W(Poly(u, v, p))); } }
        public static Sprite Dumbbell { get { return Make("dumbbell", 128, (u, v) => W(RR(u, v, 0, 0, .62f, .09f, .04f) || RR(u, v, -.6f, 0, .12f, .52f, .06f) || RR(u, v, .6f, 0, .12f, .52f, .06f) || RR(u, v, -.86f, 0, .08f, .32f, .04f) || RR(u, v, .86f, 0, .08f, .32f, .04f))); } }
        public static Sprite Heart { get { var tri = new float[] { -.78f, .12f, .78f, .12f, 0, -.85f }; return Make("heart", 128, (u, v) => W(Circ(u, v, -.37f, .28f, .42f) || Circ(u, v, .37f, .28f, .42f) || Poly(u, v, tri))); } }
        public static Sprite Lock { get { return Make("lock", 96, (u, v) => { float r = Mathf.Sqrt(u * u + (v - .15f) * (v - .15f)); return W(RR(u, v, 0, -.35f, .62f, .45f, .12f) || (v > .1f && r > .3f && r < .48f)); }); } }
        public static Sprite Hands
        {
            get
            {
                return Make("hands", 128, (u, v) =>
                {
                    if (RR(u, v, 0, -.28f, .42f, .36f, .14f)) return Color.white;
                    float[] fx = { -.3f, -.1f, .1f, .3f }, ft = { .38f, .55f, .52f, .35f };
                    for (int i = 0; i < 4; i++) if (Seg(u, v, fx[i], -.1f, fx[i], ft[i], .09f)) return Color.white;
                    if (Seg(u, v, -.38f, -.35f, -.72f, -.02f, .09f)) return Color.white;
                    float r = Mathf.Sqrt(u * u + (v + .2f) * (v + .2f));
                    if (Mathf.Abs(u) > .7f && r > .88f && r < .98f && v > -.6f && v < .3f) return new Color(1, 1, 1, .8f);
                    return W(false);
                });
            }
        }
        public static Sprite BodyIcon { get { return Make("bodyicon", 128, (u, v) => W(Circ(u, v, 0, .68f, .2f) || RR(u, v, 0, .12f, .26f, .32f, .1f) || Seg(u, v, -.12f, -.18f, -.17f, -.9f, .1f) || Seg(u, v, .12f, -.18f, .17f, -.9f, .1f) || Seg(u, v, -.3f, .38f, -.42f, -.22f, .08f) || Seg(u, v, .3f, .38f, .42f, -.22f, .08f))); } }
        public static Sprite Waves
        {
            get
            {
                return Make("waves", 128, (u, v) =>
                {
                    if (Mathf.Abs(u) > .88f) return W(false);
                    for (int k = 0; k < 3; k++) { float y0 = .48f - k * .48f; if (Mathf.Abs(v - (y0 + .13f * Mathf.Sin(u * 4.7f + k))) < .08f) return Color.white; }
                    return W(false);
                });
            }
        }
        public static Sprite Ball
        {
            get
            {
                var pents = new List<float[]> { Pent(0, 0, .32f, Mathf.PI / 2) };
                for (int i = 0; i < 5; i++) { float a = Mathf.PI / 2 + i * Mathf.PI * 2 / 5; pents.Add(Pent(Mathf.Cos(a) * .86f, Mathf.Sin(a) * .86f, .27f, a + Mathf.PI / 5 + Mathf.PI)); }
                return Make("ball", 192, (u, v) =>
                {
                    float r = Mathf.Sqrt(u * u + v * v); if (r > .96f) return W(false);
                    foreach (var p in pents) if (Poly(u, v, p)) return new Color(.07f, .08f, .1f, 1);
                    for (int i = 0; i < 5; i++) { float a = Mathf.PI / 2 + i * Mathf.PI * 2 / 5; if (Seg(u, v, Mathf.Cos(a) * .3f, Mathf.Sin(a) * .3f, Mathf.Cos(a) * .62f, Mathf.Sin(a) * .62f, .025f)) return new Color(.07f, .08f, .1f, 1); }
                    float sh = Mathf.Lerp(1f, .78f, Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), new Vector2(-.35f, .35f)) / 1.3f));
                    return new Color(sh, sh, sh * 1.02f, 1);
                });
            }
        }
    }

    /* ------------------------------ Mallas y objetos ------------------------------ */
    public static class Build
    {
        // Destroy en juego, DestroyImmediate en el editor
        public static void Kill(UnityEngine.Object o) { if (o == null) return; if (Application.isPlaying) UnityEngine.Object.Destroy(o); else UnityEngine.Object.DestroyImmediate(o); }
        // Partículas fijas (estrellas, polvo): se emiten al iniciar la escena
        public static void EmitStatic(ParticleSystem ps, int n) { var e = ps.gameObject.AddComponent<EmitOnStart>(); e.count = n; }
        public static Transform Group(string name, Transform parent, Vector3 pos = default(Vector3))
        {
            var g = new GameObject(name).transform; g.SetParent(parent, false); g.localPosition = pos; return g;
        }
        public static GameObject Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Material m, Vector3 euler = default(Vector3))
        {
            var g = GameObject.CreatePrimitive(t);
            var col = g.GetComponent<Collider>(); if (col != null) UnityEngine.Object.DestroyImmediate(col);
            g.transform.SetParent(parent, false); g.transform.localPosition = pos; g.transform.localEulerAngles = euler; g.transform.localScale = scale;
            var r = g.GetComponent<Renderer>(); r.sharedMaterial = m; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            return g;
        }
        public static GameObject MeshObj(string name, Transform parent, Mesh mesh, Material m)
        {
            var g = new GameObject(name); g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = m; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            return g;
        }
        public static Mesh Finish(List<Vector3> v, List<Vector2> uv, List<int> tri, bool doubleSided)
        {
            var m = new Mesh(); m.indexFormat = IndexFormat.UInt32;
            if (doubleSided)
            {
                int n = v.Count; var v2 = new List<Vector3>(v); v2.AddRange(v); var uv2 = new List<Vector2>(uv); uv2.AddRange(uv);
                var t2 = new List<int>(tri); for (int i = 0; i < tri.Count; i += 3) { t2.Add(tri[i] + n); t2.Add(tri[i + 2] + n); t2.Add(tri[i + 1] + n); }
                v = v2; uv = uv2; tri = t2;
            }
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0); m.RecalculateNormals(); m.RecalculateBounds(); return Persist.Keep(m, "Mesh_" + v.Count);
        }
        // Plano horizontal (XZ) con UV 0..1 (o repetido)
        public static Mesh Floor(float w, float d, float ru = 1, float rv = 1)
        {
            var v = new List<Vector3> { new Vector3(-w / 2, 0, -d / 2), new Vector3(w / 2, 0, -d / 2), new Vector3(w / 2, 0, d / 2), new Vector3(-w / 2, 0, d / 2) };
            var uv = new List<Vector2> { new Vector2(0, 0), new Vector2(ru, 0), new Vector2(ru, rv), new Vector2(0, rv) };
            return Finish(v, uv, new List<int> { 0, 2, 1, 0, 3, 2 }, false);
        }
        // Plano vertical (XY) visible desde -Z
        public static Mesh Wall(float w, float h, float ru = 1, float rv = 1, bool doubleSided = false)
        {
            var v = new List<Vector3> { new Vector3(-w / 2, -h / 2, 0), new Vector3(w / 2, -h / 2, 0), new Vector3(w / 2, h / 2, 0), new Vector3(-w / 2, h / 2, 0) };
            var uv = new List<Vector2> { new Vector2(0, 0), new Vector2(ru, 0), new Vector2(ru, rv), new Vector2(0, rv) };
            return Finish(v, uv, new List<int> { 0, 2, 1, 0, 3, 2 }, doubleSided);
        }
        public static Mesh InvertedSphere(float r, int seg = 48, int rings = 24)
        {
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
            for (int y = 0; y <= rings; y++) for (int x = 0; x <= seg; x++)
                {
                    float th = (float)y / rings * Mathf.PI, ph = (float)x / seg * Mathf.PI * 2;
                    v.Add(new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), -Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(ph)) * r);
                    uv.Add(new Vector2((float)x / seg, (float)y / rings));
                }
            for (int y = 0; y < rings; y++) for (int x = 0; x < seg; x++)
                { int a = y * (seg + 1) + x, b = a + seg + 1; t.Add(a); t.Add(b); t.Add(a + 1); t.Add(b); t.Add(b + 1); t.Add(a + 1); }
            return Finish(v, uv, t, false);
        }
        public static GameObject Glow(Transform parent, Vector3 pos, float size, Color c, bool billboard = true)
        {
            var g = MeshObj("glow", parent, Wall(1, 1), Mat.Unlit(c, Tex.Glow));
            g.transform.localPosition = pos; g.transform.localScale = Vector3.one * size;
            if (billboard) g.AddComponent<Billboard>();
            return g;
        }
        public static ParticleSystem Particles(Transform parent, Vector3 pos, Color c, float size, int max)
        {
            var go = new GameObject("particles"); go.transform.SetParent(parent, false); go.transform.localPosition = pos;
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.playOnAwake = false; main.startColor = c; main.startSize = size; main.maxParticles = max;
            main.startSpeed = 0; main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var em = ps.emission; em.enabled = false;
            var sh = ps.shape; sh.enabled = false;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = Mat.Unlit(Color.white, Tex.Glow); r.shadowCastingMode = ShadowCastingMode.Off;
            return ps;
        }
    }

    public class Billboard : MonoBehaviour
    {
        public bool yOnly;
        void LateUpdate()
        {
            var cam = PikiRig.CamT; if (cam == null) return;
            Vector3 d = transform.position - cam.position; if (yOnly) d.y = 0;
            if (d.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(d, Vector3.up);
        }
    }

    /* ------------------------------ Componentes de animación del entorno ------------------------------ */
    public class EmitOnStart : MonoBehaviour
    {
        public int count = 100;
        void Start() { var ps = GetComponent<ParticleSystem>(); if (ps == null) return; ps.Play(); ps.Emit(count); }
    }
    public class Spin : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0, 10, 0);
        void Update() { transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self); }
    }
    public class Pulse : MonoBehaviour
    {
        public float amount = .02f, speed = .8f, phase; Vector3 baseScale;
        void Start() { baseScale = transform.localScale; }
        void Update() { transform.localScale = baseScale * (1 + Mathf.Sin(Time.time * speed + phase) * amount); }
    }
    public class FlagWave : MonoBehaviour
    {
        public float phase;
        void Update() { transform.localEulerAngles = new Vector3(0, Mathf.Sin(Time.time * 2.2f + phase) * 28 + 30, 0); }
    }
    public class LedScroll : MonoBehaviour
    {
        public float period = 1000, speed = 180;
        void Update() { var rt = (RectTransform)transform; var p = rt.anchoredPosition; p.x = -((Time.time * speed) % period); rt.anchoredPosition = p; }
    }

    /* ------------------------------ Tweens / corrutinas ------------------------------ */
    public class Runner : MonoBehaviour
    {
        static Runner inst;
        public static Runner I { get { if (inst == null) inst = new GameObject("PikiRunner").AddComponent<Runner>(); return inst; } }
    }
    public static class Tw
    {
        public static IEnumerator Co(float dur, Action<float> f, Func<float, float> ease = null)
        {
            if (ease == null) ease = Ease.InOut; float t = 0;
            while (t < dur) { f(ease(Mathf.Clamp01(t / dur))); yield return null; t += Time.deltaTime; }
            f(ease(1));
        }
        public static Coroutine Go(float dur, Action<float> f, Func<float, float> ease = null) { return Runner.I.StartCoroutine(Co(dur, f, ease)); }
        public static Coroutine Later(float s, Action a) { return Runner.I.StartCoroutine(LaterCo(s, a)); }
        static IEnumerator LaterCo(float s, Action a) { yield return new WaitForSeconds(s); a(); }
    }
}
