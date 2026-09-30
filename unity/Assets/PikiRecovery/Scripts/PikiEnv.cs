// =====================================================================
//  PIKI RECOVERY · Entornos 3D procedurales
//  · Hub (inicio)   · Estadio de fútbol (partido / calma / amanecer)
//  · Vestuario (etapa nutricional)
//  El usuario mira hacia +Z. La cancha va a lo largo de Z.
// =====================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    public struct SkyP { public Color zen, mid, hor, gnd; public float sun; public SkyP(string z, string m, string h, string g, float s) { zen = Pal.Hex(z); mid = Pal.Hex(m); hor = Pal.Hex(h); gnd = Pal.Hex(g); sun = s; } }
    public struct LightP
    {
        public Color sky, eq, gnd, dir, fog; public float dirI, fogD, stars, flood;
        public LightP(string sky, string eq, string gnd, string dir, float dirI, string fog, float fogD, float stars, float flood)
        { this.sky = Pal.Hex(sky); this.eq = Pal.Hex(eq); this.gnd = Pal.Hex(gnd); this.dir = Pal.Hex(dir); this.dirI = dirI; this.fog = Pal.Hex(fog); this.fogD = fogD; this.stars = stars; this.flood = flood; }
        public static LightP Lerp(LightP a, LightP b, float k)
        {
            var o = a; o.sky = Color.Lerp(a.sky, b.sky, k); o.eq = Color.Lerp(a.eq, b.eq, k); o.gnd = Color.Lerp(a.gnd, b.gnd, k); o.dir = Color.Lerp(a.dir, b.dir, k);
            o.fog = Color.Lerp(a.fog, b.fog, k); o.dirI = Mathf.Lerp(a.dirI, b.dirI, k); o.fogD = Mathf.Lerp(a.fogD, b.fogD, k); o.stars = Mathf.Lerp(a.stars, b.stars, k); o.flood = Mathf.Lerp(a.flood, b.flood, k); return o;
        }
    }

    public enum EnvName { Hub, Stadium, Locker }
    public enum StadiumMode { Match, Calm, Dawn }

    public class PikiEnv : MonoBehaviour
    {
        public static readonly SkyP SkyHub = new SkyP("#01030a", "#040b1a", "#0b1c33", "#02050b", 0);
        public static readonly SkyP SkyDusk = new SkyP("#070b22", "#27305f", "#f0874a", "#150f1a", .9f);
        public static readonly SkyP SkyNight = new SkyP("#010209", "#040b22", "#12284a", "#02040a", 0);
        public static readonly SkyP SkyDawn = new SkyP("#23376b", "#7f93c4", "#ffc58a", "#3a2c2a", 1);

        public static readonly LightP LHub = new LightP("#1b3350", "#0b1a2a", "#050a10", "#ffffff", .2f, "#050b16", .018f, .6f, 0);
        public static readonly LightP LMatch = new LightP("#8d9ccf", "#6f6f8a", "#2a3a2a", "#ffffff", 1.15f, "#2a2748", .0028f, .15f, 1);
        public static readonly LightP LCalm = new LightP("#26335a", "#1a2238", "#0a100c", "#a9bcff", .38f, "#050a18", .0055f, 1, .22f);
        public static readonly LightP LCalmDeep = new LightP("#1d2848", "#141b2e", "#080c0a", "#a9bcff", .3f, "#050a18", .006f, 1, .06f);
        public static readonly LightP LDawn = new LightP("#d9c3b0", "#9b8f86", "#3b3a2e", "#ffd2a1", 1.0f, "#c9a393", .0022f, 0, 0);
        public static readonly LightP LLocker = new LightP("#8a8378", "#6b655c", "#3a3530", "#ffffff", .25f, "#000000", 0, 0, 0);

        public EnvName Current { get; private set; }
        public SkyP SkyNow { get; private set; }
        public LightP LightNow { get; private set; }

        // Referencias guardadas en la escena (se completan al construirla)
        public Transform Hub, Stadium, Locker;
        [HideInInspector] public PikiRig rig;
        [HideInInspector] public Light dirLight; [HideInInspector] public MeshRenderer skyR; [HideInInspector] public Renderer starsR; [HideInInspector] public GameObject sun;
        [HideInInspector] public List<Renderer> floodGlows = new List<Renderer>(), floodPanels = new List<Renderer>();
        [HideInInspector] public Renderer roofLight, standR; [HideInInspector] public Texture2D crowdFull, crowdEmpty; [HideInInspector] public Transform players;
        [HideInInspector] public UIText sbTitle, sbHome, sbAway, sbScore, sbFoot, sbBig1, sbBig2;
        Texture2D skyTex; float giTimer;

        public void Create(bool hub = true, bool stadium = true, bool locker = true)
        {
            var lgo = new GameObject("Sol / Focos"); lgo.transform.SetParent(transform, false);
            dirLight = lgo.AddComponent<Light>(); dirLight.type = LightType.Directional; dirLight.shadows = LightShadows.None;
            lgo.transform.rotation = Quaternion.Euler(55, -30, 0);
            RenderSettings.skybox = null; RenderSettings.ambientMode = AmbientMode.Trilight; RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential;
            BuildSky();
            if (hub) BuildHub();
            if (stadium) BuildStadium();
            if (locker) BuildLocker();
        }

        // En juego se usan copias de los materiales para no modificar los assets de la escena
        void Awake()
        {
            if (!Application.isPlaying || skyR == null) return;
            skyTex = new Texture2D(2, 256, TextureFormat.RGBA32, false); skyTex.wrapMode = TextureWrapMode.Clamp;
            var sm = new Material(skyR.sharedMaterial); sm.mainTexture = skyTex; skyR.sharedMaterial = sm;
            var list = new List<Renderer>(floodGlows); list.AddRange(floodPanels); list.Add(roofLight); list.Add(standR); list.Add(starsR);
            if (sun != null) list.Add(sun.GetComponent<Renderer>());
            foreach (var r in list) if (r != null) r.sharedMaterial = new Material(r.sharedMaterial);
        }

        /* ------------------------------ Cielo y luces ------------------------------ */
        void BuildSky()
        {
            skyTex = new Texture2D(2, 256, TextureFormat.RGBA32, false); skyTex.wrapMode = TextureWrapMode.Clamp; Persist.Keep(skyTex, "Cielo");
            var sky = Build.MeshObj("Cielo", transform, Build.InvertedSphere(800), Mat.Unlit(Color.white, skyTex, 1000));
            skyR = sky.GetComponent<MeshRenderer>();
            sky.AddComponent<FollowCam>();
            var stars = Build.Particles(sky.transform, Vector3.zero, Color.white, 2.4f, 1600);
            var sh = stars.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Hemisphere; sh.radius = 700; sh.radiusThickness = 0; sh.rotation = new Vector3(-90, 0, 0);
            var main = stars.main; main.startLifetime = 100000f; main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 3f);
            starsR = stars.GetComponent<Renderer>(); starsR.sharedMaterial.renderQueue = 1001;
            Build.EmitStatic(stars, 1400);
            sun = Build.Glow(sky.transform, Quaternion.Euler(-4, -40, 0) * Vector3.forward * 700, 420, new Color(1, .85f, .6f, .8f));
            sun.GetComponent<Renderer>().sharedMaterial.renderQueue = 1002;
        }
        public void DrawSky(SkyP p)
        {
            SkyNow = p; if (skyTex == null) skyTex = skyR.sharedMaterial.mainTexture as Texture2D; if (skyTex == null) return; var px = new Color[512];
            for (int y = 0; y < 256; y++)
            {
                float v = y / 255f; Color c;
                if (v < .47f) c = p.gnd; else if (v < .5f) c = Color.Lerp(p.gnd, p.hor, (v - .47f) / .03f);
                else if (v < .68f) c = Color.Lerp(p.hor, p.mid, (v - .5f) / .18f); else c = Color.Lerp(p.mid, p.zen, (v - .68f) / .32f);
                px[y * 2] = px[y * 2 + 1] = c;
            }
            skyTex.SetPixels(px); skyTex.Apply(false);
            sun.GetComponent<Renderer>().sharedMaterial.color = new Color(1, .82f, .55f, .75f * p.sun);
            sun.SetActive(p.sun > .01f);
        }
        public void BlendSky(SkyP a, SkyP b, float k)
        {
            var o = new SkyP(); o.zen = Color.Lerp(a.zen, b.zen, k); o.mid = Color.Lerp(a.mid, b.mid, k); o.hor = Color.Lerp(a.hor, b.hor, k); o.gnd = Color.Lerp(a.gnd, b.gnd, k); o.sun = Mathf.Lerp(a.sun, b.sun, k);
            DrawSky(o);
        }
        public void ApplyLight(LightP p, bool forceGI = true)
        {
            LightNow = p;
            RenderSettings.ambientSkyColor = p.sky; RenderSettings.ambientEquatorColor = p.eq; RenderSettings.ambientGroundColor = p.gnd;
            dirLight.color = p.dir; dirLight.intensity = p.dirI;
            RenderSettings.fog = p.fogD > 0; RenderSettings.fogColor = p.fog; RenderSettings.fogDensity = p.fogD;
            starsR.sharedMaterial.color = new Color(1, 1, 1, p.stars);
            foreach (var g in floodGlows) g.sharedMaterial.color = new Color(1, .95f, .84f, .85f * p.flood);
            foreach (var r in floodPanels) r.sharedMaterial.SetColor("_EmissionColor", Color.white * (.25f + 2.2f * p.flood));
            if (roofLight != null) roofLight.sharedMaterial.color = Color.Lerp(new Color(.2f, .2f, .2f), new Color(1, .97f, .88f), Mathf.Clamp01(p.flood));
            giTimer -= Time.deltaTime; if (forceGI || giTimer <= 0) { DynamicGI.UpdateEnvironment(); giTimer = .25f; }
        }

        public void Set(EnvName env, StadiumMode mode = StadiumMode.Match)
        {
            Current = env;
            Transform want = env == EnvName.Hub ? Hub : env == EnvName.Stadium ? Stadium : Locker;
            if (want == null) { Debug.LogWarning("Piki Recovery: esta escena no tiene el entorno " + env + "."); return; }
            if (Hub != null) Hub.gameObject.SetActive(env == EnvName.Hub);
            if (Stadium != null) Stadium.gameObject.SetActive(env == EnvName.Stadium);
            if (Locker != null) Locker.gameObject.SetActive(env == EnvName.Locker);
            skyR.gameObject.SetActive(env != EnvName.Locker);
            var rig = this.rig != null ? this.rig : PikiRig.I;
            if (env == EnvName.Hub) { DrawSky(SkyHub); ApplyLight(LHub); rig.PlaceAt(Vector3.zero); }
            if (env == EnvName.Locker) { ApplyLight(LLocker); rig.PlaceAt(new Vector3(0, 0, -.6f)); }
            if (env == EnvName.Stadium)
            {
                SetStadiumMode(mode);
                DrawSky(mode == StadiumMode.Match ? SkyDusk : mode == StadiumMode.Calm ? SkyNight : SkyDawn);
                ApplyLight(mode == StadiumMode.Match ? LMatch : mode == StadiumMode.Calm ? LCalm : LDawn);
                rig.PlaceAt(new Vector3(0, 0, -6));
            }
        }

        /* ------------------------------ HUB ------------------------------ */
        void BuildHub()
        {
            Hub = Build.Group("Hub", transform);
            var grid = new Px(64, 64, new Color32(0, 0, 0, 0)); grid.Rect(0, 0, 64, 2, Pal.A(Pal.Teal, .55f)); grid.Rect(0, 0, 2, 64, Pal.A(Pal.Teal, .55f));
            var gm = Mat.Unlit(new Color(1, 1, 1, .6f), grid.ToTex(true, TextureWrapMode.Repeat), 2900);
            Build.MeshObj("Grilla", Hub, Build.Floor(200, 200, 100, 100), gm);
            var disc = Build.MeshObj("Disco", Hub, Build.Floor(14, 14), Mat.Unlit(Pal.A(Pal.Teal, .35f), Tex.Glow, 2901)); disc.transform.localPosition = new Vector3(0, .01f, 0);
            for (int i = 0; i < 3; i++)
            {
                var r = Build.MeshObj("Anillo", Hub, Build.Floor(1, 1), Mat.Unlit(Pal.A(Pal.Teal, .55f), Spr.Ring.texture, 2902));
                r.transform.localPosition = new Vector3(0, .02f, 0); r.transform.localScale = Vector3.one * (4.4f + i * 2.4f); var pu = r.AddComponent<Pulse>(); pu.phase = i;
            }
            var ballTex = new Px(256, 128, new Color32(0, 0, 0, 0));
            for (int i = 0; i < 256; i += 16) ballTex.Rect(i, 0, 2, 128, Pal.A(Pal.Teal, .8f)); for (int j = 0; j < 128; j += 16) ballTex.Rect(0, j, 256, 2, Pal.A(Pal.Teal, .8f));
            var ball = Build.Prim(PrimitiveType.Sphere, Hub, new Vector3(0, 12, 55), Vector3.one * 20, Mat.Unlit(new Color(1, 1, 1, .35f), ballTex.ToTex(true, TextureWrapMode.Repeat), 2903));
            ball.AddComponent<Spin>().degreesPerSecond = new Vector3(3, 5, 0);
            Build.Glow(Hub, new Vector3(0, 12, 56), 70, Pal.A(Pal.Teal, .22f));
            var hubDust = Build.Particles(Hub, new Vector3(0, 15, 0), new Color(.56f, .96f, .86f, .8f), .15f, 1200);
            var sh = hubDust.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(140, 30, 140);
            var main = hubDust.main; main.startLifetime = 100000f; main.startSize = new ParticleSystem.MinMaxCurve(.08f, .22f);
            Build.EmitStatic(hubDust, 1100); hubDust.gameObject.AddComponent<Spin>().degreesPerSecond = new Vector3(0, .4f, 0);
        }

        /* ------------------------------ ESTADIO ------------------------------ */
        static Texture2D PitchTexture()
        {
            const float TW = 86, TL = 121, ppm = 10; var px = new Px((int)(TW * ppm), (int)(TL * ppm), new Color32(44, 112, 50, 255));
            System.Func<float, float> X = x => (x + TW / 2) * ppm; System.Func<float, float> Z = z => (z + TL / 2) * ppm;
            int n = 22; float sl = TL / n;
            for (int i = 0; i < n; i++) px.Rect(0, i * sl * ppm, px.W, sl * ppm + 1, i % 2 == 1 ? new Color32(53, 127, 57, 255) : new Color32(44, 112, 50, 255));
            var rnd = new System.Random(3);
            for (int i = 0; i < 40000; i++) px.Set(rnd.Next(px.W), rnd.Next(px.H), rnd.NextDouble() < .5 ? new Color(0, 0, 0, .07f) : new Color(1, 1, 1, .04f));
            Color L = new Color(1, 1, 1, .93f); float th = 1.6f;
            px.RectOutline(X(-34), Z(-52.5f), 68 * ppm, 105 * ppm, th, L);
            px.Rect(X(-34), Z(0) - th / 2, 68 * ppm, th, L);
            px.Circle(X(0), Z(0), 9.15f * ppm, th, L); px.Disc(X(0), Z(0), .25f * ppm, L);
            foreach (int s in new[] { -1, 1 })
            {
                float gl = 52.5f * s;
                px.RectOutline(X(-20.16f), Z(s < 0 ? gl : gl - 16.5f), 40.32f * ppm, 16.5f * ppm, th, L);
                px.RectOutline(X(-9.16f), Z(s < 0 ? gl : gl - 5.5f), 18.32f * ppm, 5.5f * ppm, th, L);
                float spot = gl - s * 11; px.Disc(X(0), Z(spot), .22f * ppm, L);
                if (s < 0) px.Circle(X(0), Z(spot), 9.15f * ppm, th, L, (x, y) => y > Z(-36)); else px.Circle(X(0), Z(spot), 9.15f * ppm, th, L, (x, y) => y < Z(36));
            }
            px.Circle(X(-34), Z(-52.5f), ppm, th, L, (x, y) => x > X(-34) && y > Z(-52.5f)); px.Circle(X(34), Z(-52.5f), ppm, th, L, (x, y) => x < X(34) && y > Z(-52.5f));
            px.Circle(X(34), Z(52.5f), ppm, th, L, (x, y) => x < X(34) && y < Z(52.5f)); px.Circle(X(-34), Z(52.5f), ppm, th, L, (x, y) => x > X(-34) && y < Z(52.5f));
            return px.ToTex();
        }
        static Texture2D CrowdTexture(bool full, int seed)
        {
            int cols = 24, rows = 40, cw = 20, rh = 25; var px = new Px(cols * cw, rows * rh, new Color32(22, 29, 38, 255)); var r = new System.Random(seed);
            Color[] home = { Pal.Teal, Pal.Teal, Color.white, Pal.Hex("#0b2a45"), Pal.Teal, Pal.Hex("#e8fff8") }, away = { Pal.Hex("#d63447"), Color.white, Pal.Hex("#b01e30") }, misc = { Pal.Yellow, Pal.Hex("#3a86ff"), Pal.Hex("#222222"), Pal.Hex("#8d99ae"), Pal.Orange };
            Color[] skin = { Pal.Hex("#f1c7a5"), Pal.Hex("#d9a47f"), Pal.Hex("#a86f4c"), Pal.Hex("#6d4430"), Pal.Hex("#e8b894") };
            for (int row = 0; row < rows; row++)
            {
                float y = row * rh; px.Rect(0, y, px.W, 3, new Color32(13, 18, 24, 255));
                for (int k = 0; k < cols; k++)
                {
                    float x = k * cw; bool white = ((k / 6) + (row / 8)) % 5 == 0;
                    px.Rect(x + 3, y + 3, cw - 6, rh * .42f, white ? Pal.Hex("#e6f3f0") : Pal.Hex("#0f7466"));
                    if (full && r.NextDouble() < .88)
                    {
                        double roll = r.NextDouble(); var pal = roll < .72 ? home : roll < .86 ? away : misc; Color shirt = pal[r.Next(pal.Length)];
                        float cx = x + cw / 2f + (float)(r.NextDouble() * 4 - 2), by = y + rh * .38f;
                        px.Rect(cx - 7, by, 14, 12, shirt);
                        if (r.NextDouble() < .22) { var sk = skin[r.Next(skin.Length)]; px.Rect(cx - 9, by + 8, 3, 10, sk); px.Rect(cx + 6, by + 8, 3, 10, sk); }
                        px.Disc(cx, by + 16, 4.6f, skin[r.Next(skin.Length)]);
                        if (r.NextDouble() < .5) px.Rect(cx - 4.5f, by + 18, 9, 3.5f, r.NextDouble() < .5 ? new Color(.17f, .1f, .07f) : new Color(.07f, .07f, .07f));
                    }
                }
            }
            var t = px.ToTex(true, TextureWrapMode.Clamp); t.wrapModeU = TextureWrapMode.Repeat; return t;
        }
        struct Path2 { public Vector2[] p, n; }
        static Path2 Superellipse(float a, float b, float nExp, int N)
        {
            var o = new Path2 { p = new Vector2[N], n = new Vector2[N] };
            for (int i = 0; i < N; i++) { float t = (float)i / N * Mathf.PI * 2, c = Mathf.Cos(t), s = Mathf.Sin(t); o.p[i] = new Vector2(a * Mathf.Sign(c) * Mathf.Pow(Mathf.Abs(c), 2 / nExp), b * Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), 2 / nExp)); }
            for (int i = 0; i < N; i++) { var d = o.p[(i + 1) % N] - o.p[(i - 1 + N) % N]; o.n[i] = new Vector2(d.y, -d.x).normalized; }
            return o;
        }
        struct Prof { public float o, h, v; public Prof(float o, float h, float v) { this.o = o; this.h = h; this.v = v; } }
        static Mesh RingStrip(Path2 path, Prof[] prof, float uLen)
        {
            int N = path.p.Length, P = prof.Length; var v = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
            var acc = new float[N + 1]; for (int i = 1; i <= N; i++) acc[i] = acc[i - 1] + Vector2.Distance(path.p[i % N], path.p[i - 1]);
            float reps = Mathf.Max(1, Mathf.Round(acc[N] / uLen));
            for (int i = 0; i <= N; i++)
            {
                var p = path.p[i % N]; var n = path.n[i % N]; float u = acc[i] / acc[N] * reps;
                for (int j = 0; j < P; j++) { v.Add(new Vector3(p.x + n.x * prof[j].o, prof[j].h, p.y + n.y * prof[j].o)); uv.Add(new Vector2(u, prof[j].v)); }
            }
            for (int i = 0; i < N; i++) for (int j = 0; j < P - 1; j++) { int a = i * P + j, b = (i + 1) * P + j; t.Add(a); t.Add(a + 1); t.Add(b); t.Add(b); t.Add(a + 1); t.Add(b + 1); }
            return Build.Finish(v, uv, t, true);
        }

        void BuildStadium()
        {
            Stadium = Build.Group("Estadio", transform); var S = Stadium;
            Build.MeshObj("Suelo", S, Build.Floor(420, 420), Mat.Lit(Pal.Hex("#1c2a1f"), .05f)).transform.localPosition = new Vector3(0, -.02f, 0);
            Build.MeshObj("Borde de césped", S, Build.Floor(92, 128), Mat.Lit(Pal.Hex("#255f2b"), .05f)).transform.localPosition = new Vector3(0, -.01f, 0);
            Build.MeshObj("Cancha", S, Build.Floor(86, 121), Mat.Lit(Color.white, .08f, 0, PitchTexture()));

            // Tribunas: anillo continuo
            var path = Superellipse(41, 59, 10, 240);
            crowdFull = CrowdTexture(true, 11); crowdEmpty = CrowdTexture(false, 11);
            standR = Build.MeshObj("Tribunas", S, RingStrip(path, new[] { new Prof(0, 1.4f, 0), new Prof(26, 22, 1) }, 12), Mat.Lit(Color.white, .05f, 0, crowdFull)).GetComponent<Renderer>();
            Build.MeshObj("Muro", S, RingStrip(path, new[] { new Prof(0, 0, 0), new Prof(0, 1.4f, 1) }, 12), Mat.Lit(Pal.Hex("#0f1822"), .2f));
            Build.MeshObj("Fachada", S, RingStrip(path, new[] { new Prof(26, 21.5f, 0), new Prof(27, 22.5f, .3f), new Prof(27.5f, 28, .6f), new Prof(27.5f, 0, 1) }, 12), Mat.Lit(Pal.Hex("#1a222d"), .4f, .3f));
            Build.MeshObj("Techo", S, RingStrip(path, new[] { new Prof(27.5f, 28, 0), new Prof(9, 30.5f, 1) }, 12), Mat.Lit(Pal.Hex("#2a323c"), .5f, .4f));
            roofLight = Build.MeshObj("Luces del techo", S, RingStrip(path, new[] { new Prof(11, 30.05f, 0), new Prof(12.2f, 29.95f, 1) }, 12), Mat.Unlit(Color.white, null, 2000)).GetComponent<Renderer>();
            var vom = Mat.Lit(Pal.Hex("#0a0f14"));
            for (int i = 0; i < 16; i++)
            {
                int k = i * path.p.Length / 16; var p = path.p[k]; var n = path.n[k];
                var m = Build.Prim(PrimitiveType.Cube, S, new Vector3(p.x + n.x * 11, 10.2f, p.y + n.y * 11), new Vector3(3, .3f, 2.2f), vom);
                m.transform.rotation = Quaternion.LookRotation(new Vector3(n.x, 0, n.y)) * Quaternion.Euler(-38.4f, 0, 0);
            }

            // Carteles LED
            BuildLED(S, 100, new Vector3(37, .55f, 0), 90); BuildLED(S, 100, new Vector3(-37, .55f, 0), -90);
            BuildLED(S, 56, new Vector3(0, .55f, 56.5f), 0); BuildLED(S, 56, new Vector3(0, .55f, -56.5f), 180);

            // Arcos
            var netTex = new Px(32, 32, new Color32(0, 0, 0, 0)); netTex.Rect(0, 0, 32, 2, new Color(1, 1, 1, .9f)); netTex.Rect(0, 0, 2, 32, new Color(1, 1, 1, .9f));
            var netMat = Mat.Unlit(new Color(1, 1, 1, .8f), netTex.ToTex(true, TextureWrapMode.Repeat), 3000);
            var postM = Mat.Lit(Color.white, .7f);
            foreach (int dir in new[] { -1, 1 }) BuildGoal(S, 52.5f * dir, dir, postM, netMat);

            // Banderines
            var flagM = Mat.Lit(Pal.Yellow, .3f); int flagIndex = 0; var poleM = Mat.Lit(Color.white, .5f);
            foreach (var c in new[] { new Vector2(-34, -52.5f), new Vector2(34, -52.5f), new Vector2(34, 52.5f), new Vector2(-34, 52.5f) })
            {
                Build.Prim(PrimitiveType.Cylinder, S, new Vector3(c.x, .75f, c.y), new Vector3(.04f, .75f, .04f), poleM);
                var piv = Build.Group("Banderín", S, new Vector3(c.x, 1.35f, c.y));
                var f = Build.MeshObj("tela", piv, Build.Wall(.42f, .28f, 1, 1, true), flagM); f.transform.localPosition = new Vector3(.21f, 0, 0);
                piv.gameObject.AddComponent<FlagWave>().phase = flagIndex++;
            }

            // Torres de iluminación
            var lamp = new Px(256, 128, new Color32(26, 31, 38, 255));
            for (int r = 0; r < 4; r++) for (int k = 0; k < 8; k++) { lamp.Disc(16 + k * 32, 16 + r * 32, 13, new Color(.55f, .55f, .5f)); lamp.Disc(16 + k * 32, 16 + r * 32, 9, Color.white); }
            var lampTex = lamp.ToTex();
            foreach (var c in new[] { new Vector2(-64, -82), new Vector2(64, -82), new Vector2(64, 82), new Vector2(-64, 82) })
            {
                Build.Prim(PrimitiveType.Cylinder, S, new Vector3(c.x, 25, c.y), new Vector3(1.2f, 25, 1.2f), Mat.Lit(Pal.Hex("#39424e"), .5f, .5f));
                var pm = Mat.Emissive(Color.white, Color.white * 2, lampTex);
                var panel = Build.Prim(PrimitiveType.Cube, S, new Vector3(c.x * .97f, 52, c.y * .97f), new Vector3(12, 6, .5f), pm); floodPanels.Add(panel.GetComponent<Renderer>());
                panel.transform.rotation = Quaternion.LookRotation(new Vector3(-c.x, -52, -c.y).normalized);
                var glow = Build.Glow(S, new Vector3(c.x * .95f, 51.5f, c.y * .95f), 46, new Color(1, .95f, .84f, .85f));
                floodGlows.Add(glow.GetComponent<Renderer>());
            }

            // Marcadores
            BuildScoreboard(S);

            // Bancos de suplentes
            var seatM = Mat.Lit(Pal.Teal, .4f); var shellM = Mat.Unlit(new Color(.62f, .86f, 1, .3f), null, 3000); var darkM = Mat.Lit(Pal.Hex("#0f1822"));
            foreach (float z in new[] { -9f, 9f })
            {
                var d = Build.Group("Banco", S, new Vector3(-39.2f, 0, z));
                Build.Prim(PrimitiveType.Cube, d, new Vector3(0, 2.1f, 0), new Vector3(1.6f, .08f, 7), shellM);
                Build.Prim(PrimitiveType.Cube, d, new Vector3(-.8f, 1.05f, 0), new Vector3(.1f, 2.1f, 7), darkM);
                for (int i = 0; i < 8; i++) Build.Prim(PrimitiveType.Cube, d, new Vector3(-.4f, .45f, -3 + i * .85f), new Vector3(.5f, .5f, .6f), seatM);
            }
            Build.Prim(PrimitiveType.Cube, S, new Vector3(-41.5f, 1.5f, 0), new Vector3(2, 3, 6), Mat.Lit(Pal.Hex("#06090d")));
            Build.Glow(S, new Vector3(-40.3f, 1.6f, 0), 6, new Color(1, .9f, .7f, .5f));

            // Pelota, botellas y toalla junto al jugador
            var ballTex = new Px(512, 256, new Color32(244, 246, 248, 255));
            for (int i = 0; i < 14; i++) { float x = (i % 7) * 76 + ((i / 7) % 2) * 38 + 20, y = (i / 7) * 128 + 64; ballTex.Disc(x, y, 22, new Color(.06f, .08f, .1f)); }
            Build.Prim(PrimitiveType.Sphere, S, new Vector3(-2.3f, .11f, -4.6f), Vector3.one * .22f, Mat.Lit(Color.white, .5f, 0, ballTex.ToTex()));
            var botM = Mat.Lit(Pal.Teal, .7f); var capM = Mat.Lit(Color.white, .5f);
            for (int i = 0; i < 3; i++)
            {
                var b = Build.Group("Botella", S, new Vector3(-1.4f + i * .16f, 0, -4.2f - (i % 2) * .1f));
                Build.Prim(PrimitiveType.Cylinder, b, new Vector3(0, .11f, 0), new Vector3(.08f, .11f, .08f), botM);
                Build.Prim(PrimitiveType.Cylinder, b, new Vector3(0, .245f, 0), new Vector3(.05f, .025f, .05f), capM);
                if (i == 2) { b.localEulerAngles = new Vector3(0, 0, 90); b.localPosition += new Vector3(0, .04f, 0); }
            }
            Build.Prim(PrimitiveType.Cube, S, new Vector3(-1f, .01f, -4.6f), new Vector3(.5f, .02f, .3f), Mat.Lit(new Color(.93f, .93f, .93f)), new Vector3(0, 23, 0));

            // Jugadores que se retiran al túnel
            players = Build.Group("Jugadores", S);
            for (int i = 0; i < 6; i++) PlayerWalker.Make(players, Pal.Teal, Color.white, Pal.Teal, i);
            for (int i = 0; i < 5; i++) PlayerWalker.Make(players, Pal.Hex("#d63447"), Pal.Hex("#1a1a1a"), Pal.Hex("#d63447"), 6 + i);
            PlayerWalker.Make(players, Pal.Hex("#111111"), Pal.Hex("#111111"), Pal.Hex("#111111"), 11);
        }

        void BuildLED(Transform S, float len, Vector3 pos, float ry)
        {
            var back = Build.Prim(PrimitiveType.Cube, S, pos + Quaternion.Euler(0, ry, 0) * new Vector3(0, -.05f, .15f), new Vector3(len, 1, .25f), Mat.Lit(Pal.Hex("#0a0f14")));
            back.transform.localEulerAngles = new Vector3(0, ry, 0);
            var c = UI.Canvas(S, pos, len, .9f, new Vector3(0, ry, 0), "LED", .01f, 2f);
            UI.Img(c, 0, 0, UI.Wd(c), UI.Ht(c), null, Pal.Hex("#04080e"));
            string seg = "PIKI RECOVERY   •   EL TRABAJO INVISIBLE   •   RECUPERÁ · HIDRATÁ · RESPIRÁ   •   ";
            float cw = UI.TextWidth(seg, 56, true) / seg.Length; int vis = Mathf.Max(10, Mathf.FloorToInt((UI.Wd(c) - 40) / Mathf.Max(1, cw)));
            var t = UI.T(c, "", 20, 66, 56, Pal.Teal, true);
            var led = t.gameObject.AddComponent<LedScroll>(); led.content = seg; led.visible = vis; led.Apply();
        }

        void BuildGoal(Transform S, float z, int dir, Material postM, Material netMat)
        {
            var g = Build.Group("Arco", S, new Vector3(0, 0, z));
            foreach (int s in new[] { -1, 1 }) Build.Prim(PrimitiveType.Cylinder, g, new Vector3(s * 3.66f, 1.22f, 0), new Vector3(.12f, 1.22f, .12f), postM);
            Build.Prim(PrimitiveType.Cylinder, g, new Vector3(0, 2.44f, 0), new Vector3(.12f, 3.72f, .12f), postM, new Vector3(0, 0, 90));
            float D1 = 1.4f * dir, D2 = 2.2f * dir, Y2 = 2.2f, c = .14f;
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
            System.Action<Vector3[], Vector2[]> quad = (q, u) => { int b = v.Count; v.AddRange(q); uv.AddRange(u); t.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 }); };
            float L1 = Mathf.Sqrt(.24f * .24f + 1.4f * 1.4f) / c, L2 = Mathf.Sqrt(2.2f * 2.2f + .8f * .8f) / c, Wn = 7.32f / c;
            quad(new[] { new Vector3(-3.66f, 2.44f, 0), new Vector3(3.66f, 2.44f, 0), new Vector3(3.66f, Y2, D1), new Vector3(-3.66f, Y2, D1) }, new[] { new Vector2(0, 0), new Vector2(Wn, 0), new Vector2(Wn, L1), new Vector2(0, L1) });
            quad(new[] { new Vector3(-3.66f, Y2, D1), new Vector3(3.66f, Y2, D1), new Vector3(3.66f, 0, D2), new Vector3(-3.66f, 0, D2) }, new[] { new Vector2(0, 0), new Vector2(Wn, 0), new Vector2(Wn, L2), new Vector2(0, L2) });
            foreach (int s in new[] { -1, 1 })
            {
                float x = s * 3.66f; var pts = new[] { new Vector3(x, 0, 0), new Vector3(x, 2.44f, 0), new Vector3(x, Y2, D1), new Vector3(x, 0, D2) };
                int b = v.Count; foreach (var p in pts) { v.Add(p); uv.Add(new Vector2(Mathf.Abs(p.z) / c, p.y / c)); }
                t.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
            }
            Build.MeshObj("Red", g, Build.Finish(v, uv, t, true), netMat);
        }

        void BuildScoreboard(Transform S)
        {
            Build.Prim(PrimitiveType.Cube, S, new Vector3(0, 25.5f, 66.6f), new Vector3(23, 8.6f, 1), Mat.Lit(Pal.Hex("#10151c")));
            var c = UI.Canvas(S, new Vector3(0, 25.5f, 66f), 22, 7.7f, Vector3.zero, "Marcador", .02f, 2f);
            UI.Img(c, 0, 0, UI.Wd(c), UI.Ht(c), null, Pal.Hex("#03070c"));
            UI.Framed(c, 8, 8, UI.Wd(c) - 16, UI.Ht(c) - 16, Pal.Hex("#03070c"), Pal.Teal, 10, 5);
            float W = UI.Wd(c);
            sbTitle = UI.T(c, "FINAL", W / 2, 70, 48, Pal.Yellow, true, UI.Al.C, W);
            sbHome = UI.T(c, "PIKI FC", 220, 205, 60, Pal.Teal, true, UI.Al.C, 400);
            sbAway = UI.T(c, "VISITANTE", W - 220, 205, 60, Pal.Hex("#ff6b7a"), true, UI.Al.C, 400);
            sbScore = UI.T(c, "2 – 1", W / 2, 230, 140, Color.white, true, UI.Al.C, 600);
            sbFoot = UI.T(c, "", W / 2, 330, 32, Pal.Hex("#ccffee"), true, UI.Al.C, W);
            sbBig1 = UI.T(c, "", W / 2, 170, 96, Pal.Teal, true, UI.Al.C, W);
            sbBig2 = UI.T(c, "", W / 2, 280, 96, Color.white, true, UI.Al.C, W);
            Build.Glow(S, new Vector3(0, 25.5f, 65), 38, Pal.A(Pal.Teal, .2f));
            // marcador de enfrente con el logo
            Build.Prim(PrimitiveType.Cube, S, new Vector3(0, 25.5f, -66.6f), new Vector3(23, 8.6f, 1), Mat.Lit(Pal.Hex("#10151c")));
            var l = UI.Canvas(S, new Vector3(0, 25.5f, -66f), 22, 7.7f, new Vector3(0, 180, 0), "Logo", .02f, 2f);
            UI.Img(l, 0, 0, UI.Wd(l), UI.Ht(l), null, Pal.Hex("#03070c"));
            UI.Icon(l, Spr.Ball, 200, 192, 120, Color.white);
            UI.T(l, "PIKI", 370, 190, 130, Pal.Teal, true); UI.T(l, "RECOVERY", 374, 290, 76, Color.white, true);
        }
        public void Scoreboard(StadiumMode mode, bool done = false, string foot = null)
        {
            if (sbTitle == null) return;
            bool match = mode == StadiumMode.Match && !done;
            sbTitle.gameObject.SetActive(match); sbHome.gameObject.SetActive(match); sbAway.gameObject.SetActive(match); sbScore.gameObject.SetActive(match); sbFoot.gameObject.SetActive(match || done);
            sbBig1.gameObject.SetActive(!match); sbBig2.gameObject.SetActive(!match);
            if (match) { sbFoot.text = foot ?? "El esfuerzo terminó. La recuperación empieza."; }
            else if (done) { sbBig1.fontSize = 96; sbBig1.text = "RECUPERACIÓN"; sbBig1.color = Pal.Teal; sbBig1.FitWidth(1040); sbBig2.fontSize = 96; sbBig2.text = "COMPLETADA"; sbFoot.text = "<color=#ffd93d>El trabajo invisible también es entrenamiento</color>"; }
            else { sbBig1.fontSize = 96; sbBig1.text = "VUELTA A LA CALMA"; sbBig1.color = Pal.Purple; sbBig1.FitWidth(1040); sbBig2.fontSize = 70; sbBig2.text = "Inhalá · Exhalá · Soltá"; sbBig2.FitWidth(1040); }
        }
        public void SetStadiumMode(StadiumMode m)
        {
            standR.sharedMaterial.mainTexture = m == StadiumMode.Match ? crowdFull : crowdEmpty;
            players.gameObject.SetActive(m == StadiumMode.Match);
            if (m == StadiumMode.Match) foreach (var p in players.GetComponentsInChildren<PlayerWalker>(true)) p.ResetPos();
            Scoreboard(m, m == StadiumMode.Dawn);
        }
        public void MatchScore(int home, int away, int extra) { if (sbScore == null) return; sbScore.text = home + " – " + away; sbTitle.text = "FINAL · 90+" + extra + "'"; }

        /* ------------------------------ VESTUARIO ------------------------------ */
        static Texture2D Tiles(Color baseC, Color grout, int tiles, int size, int seed)
        {
            var px = new Px(size, size, grout); float s = (float)size / tiles; var r = new System.Random(seed);
            for (int i = 0; i < tiles; i++) for (int j = 0; j < tiles; j++) { float v = (float)(r.NextDouble() * .06 - .03); px.Rect(i * s + 2, j * s + 2, s - 4, s - 4, new Color(baseC.r + v, baseC.g + v, baseC.b + v)); }
            return px.ToTex(true, TextureWrapMode.Repeat);
        }
        void BuildLocker()
        {
            Locker = Build.Group("Vestuario", transform); var L = Locker; float W = 16, D = 16, H = 4.4f;
            Build.MeshObj("Piso", L, Build.Floor(W, D, 4, 4), Mat.Lit(Color.white, .55f, .1f, Tiles(Pal.Hex("#3b4a58"), Pal.Hex("#1f2830"), 8, 512, 1)));
            // paredes: azulejos blancos arriba, franja teal, verde oscuro abajo
            var wall = new Px(512, 256, Pal.Hex("#9aa7b3"));
            for (int y = 0; y < 256; y += 16) for (int x = 0; x < 512; x += 32) { bool low = y < 150; bool band = y >= 150 && y < 168; if (band) continue; float v = (x * 7 + y * 13) % 11 / 300f; Color b = low ? Pal.Hex("#0f5a52") : Pal.Hex("#e8eef2"); wall.Rect(x + 1, y + 1, 30, 14, new Color(b.r + v, b.g + v, b.b + v)); }
            wall.Rect(0, 150, 512, 18, Pal.Teal);
            var wallM = Mat.Lit(Color.white, .6f, 0, wall.ToTex(true, TextureWrapMode.Repeat));
            var walls = new[] { new Vector4(0, H / 2, D / 2, 0), new Vector4(0, H / 2, -D / 2, 180), new Vector4(W / 2, H / 2, 0, 90), new Vector4(-W / 2, H / 2, 0, -90) };
            foreach (var w in walls) { var m = Build.MeshObj("Pared", L, Build.Wall(W, H, 2, 1), wallM); m.transform.localPosition = new Vector3(w.x, w.y, w.z); m.transform.localEulerAngles = new Vector3(0, w.w, 0); }
            var bandText = UI.Canvas(L, new Vector3(0, 2.73f, D / 2 - .02f), 8, .2f, Vector3.zero, "Franja", .005f, 2);
            UI.T(bandText, "PIKI FC · VESTUARIO LOCAL", UI.Wd(bandText) / 2, 34, 30, Pal.Ink, true, UI.Al.C, UI.Wd(bandText));
            var ceil = Build.MeshObj("Techo", L, Build.Floor(W, D), Mat.Lit(Pal.Hex("#2a3038"))); ceil.transform.localPosition = new Vector3(0, H, 0); ceil.transform.localEulerAngles = new Vector3(180, 0, 0);
            var lightM = Mat.Unlit(Pal.Hex("#fff8ea"), null, 2000);
            for (int i = -1; i <= 1; i++) for (int j = -1; j <= 1; j += 2) Build.Prim(PrimitiveType.Cube, L, new Vector3(i * 4.5f, H - .02f, j * 3), new Vector3(1.8f, .03f, .32f), lightM);
            foreach (var p in new[] { new Vector3(0, 3.8f, 3), new Vector3(0, 3.8f, -4) })
            { var lg = new GameObject("Luz"); lg.transform.SetParent(L, false); lg.transform.localPosition = p; var li = lg.AddComponent<Light>(); li.type = LightType.Point; li.color = Pal.Hex("#ffe9c9"); li.range = 14; li.intensity = 1.6f; li.shadows = LightShadows.None; }

            // Taquillas con camisetas
            var wood = Mat.Lit(Pal.Hex("#6b4a2f"), .3f); var woodDark = Mat.Lit(Pal.Hex("#4a3220"), .3f); var bench = Mat.Lit(Pal.Hex("#7a5636"), .4f); var boot = Mat.Lit(Pal.Hex("#101010"), .6f);
            var jerseyTex = JerseyTexture(); var jerseyM = Mat.Unlit(Color.white, jerseyTex, 2450);
            Color[] bags = { Pal.Teal, Pal.Hex("#0b2a45"), Pal.Hex("#2c2c2c") };
            int num = 1;
            System.Action<float, float, float> locker = (x, z, ry) =>
            {
                var g = Build.Group("Taquilla " + num, L, new Vector3(x, 0, z)); g.localEulerAngles = new Vector3(0, ry, 0);
                Build.Prim(PrimitiveType.Cube, g, new Vector3(0, 1.2f, 0), new Vector3(1.7f, 2.4f, .06f), wood);
                foreach (int s in new[] { -1, 1 }) Build.Prim(PrimitiveType.Cube, g, new Vector3(s * .85f, 1.2f, -.27f), new Vector3(.05f, 2.4f, .55f), woodDark);
                Build.Prim(PrimitiveType.Cube, g, new Vector3(0, 2f, -.27f), new Vector3(1.7f, .05f, .55f), woodDark);
                Build.Prim(PrimitiveType.Cube, g, new Vector3(0, 2.4f, -.27f), new Vector3(1.7f, .06f, .55f), woodDark);
                var j = Build.MeshObj("Camiseta", g, Build.Wall(.75f, .75f), jerseyM); j.transform.localPosition = new Vector3(0, 1.45f, -.06f);
                var nc = UI.Canvas(g, new Vector3(0, 1.36f, -.07f), .5f, .3f, Vector3.zero, "Número", .002f, 3);
                UI.T(nc, num.ToString(), UI.Wd(nc) / 2, 118, 110, Color.white, true, UI.Al.C, UI.Wd(nc));
                foreach (int s in new[] { -1, 1 }) Build.Prim(PrimitiveType.Cube, g, new Vector3(s * .08f, .5f, -.2f), new Vector3(.1f, .08f, .27f), boot);
                Build.Prim(PrimitiveType.Capsule, g, new Vector3(0, 2.16f, -.25f), new Vector3(.24f, .3f, .24f), Mat.Lit(bags[num % 3], .3f), new Vector3(0, 0, 90));
                Build.Prim(PrimitiveType.Cube, g, new Vector3(0, .46f, -.75f), new Vector3(1.8f, .08f, .45f), bench);
                foreach (int s in new[] { -1, 1 }) Build.Prim(PrimitiveType.Cube, g, new Vector3(s * .75f, .22f, -.75f), new Vector3(.06f, .44f, .38f), woodDark);
                num++;
            };
            for (int i = 0; i < 9; i++) locker(-7.2f + i * 1.8f, D / 2 - .3f, 0);
            for (int i = 0; i < 7; i++) locker(-W / 2 + .3f, 5.4f - i * 1.8f, -90);
            for (int i = 0; i < 7; i++) locker(W / 2 - .3f, 5.4f - i * 1.8f, 90);

            // Escudo
            var crest = UI.Canvas(L, new Vector3(-5.4f, 3.3f, D / 2 - .05f), 1.2f, 1.2f, Vector3.zero, "Escudo", .004f, 2);
            float cw = UI.Wd(crest);
            UI.Round(crest, 20, 20, cw - 40, cw - 40, Pal.Teal, 60); UI.Round(crest, 34, 34, cw - 68, cw - 68, Pal.Hex("#0b2a45"), 50);
            UI.T(crest, "PIKI FC", cw / 2, 90, 52, Color.white, true, UI.Al.C, cw); UI.Icon(crest, Spr.Ball, cw / 2, cw / 2 + 10, 80, Color.white);
            UI.T(crest, "EST. 2024", cw / 2, cw - 50, 26, Pal.Teal, true, UI.Al.C, cw);

            // Pizarra táctica y puerta (detrás del jugador)
            var boardC = UI.Canvas(L, new Vector3(2.5f, 1.9f, -D / 2 + .05f), 2.8f, 1.75f, new Vector3(0, 180, 0), "Pizarra", .004f, 2);
            float bw = UI.Wd(boardC), bh = UI.Ht(boardC);
            UI.Img(boardC, 0, 0, bw, bh, null, Pal.Hex("#f7f8fa"));
            UI.Framed(boardC, 40, 60, bw - 80, bh - 100, Pal.Hex("#f7f8fa"), Pal.Hex("#2d8a44"), 4, 4);
            UI.Img(boardC, bw / 2 - 2, 60, 4, bh - 100, null, Pal.Hex("#2d8a44"));
            UI.Icon(boardC, Spr.Ring, bw / 2, 60 + (bh - 100) / 2, 60, Pal.Hex("#2d8a44"));
            foreach (var p in new[] { new Vector2(160, 150), new Vector2(160, 330), new Vector2(260, 240), new Vector2(320, 160), new Vector2(320, 320), new Vector2(470, 240) }) UI.Icon(boardC, Spr.Ring, p.x, p.y, 18, Pal.Hex("#1b5fc1"));
            foreach (var p in new[] { new Vector2(540, 180), new Vector2(540, 300), new Vector2(620, 240) }) UI.Icon(boardC, Spr.Cross, p.x, p.y, 16, Pal.Hex("#d63447"));
            UI.T(boardC, "RECUPERAR = RENDIR", bw / 2, 44, 34, Pal.Hex("#0b2a45"), true, UI.Al.C, bw);
            Build.Prim(PrimitiveType.Cube, L, new Vector3(-3, 1.15f, -D / 2 + .05f), new Vector3(1.2f, 2.3f, .1f), Mat.Lit(Pal.Hex("#1b2530")));
            var exit = UI.Canvas(L, new Vector3(-3, 2.55f, -D / 2 + .12f), .8f, .3f, new Vector3(0, 180, 0), "Salida", .004f, 2);
            UI.Img(exit, 0, 0, UI.Wd(exit), UI.Ht(exit), null, Pal.Hex("#0a7f3f")); UI.T(exit, "A LA CANCHA →", UI.Wd(exit) / 2, 50, 30, Color.white, true, UI.Al.C, UI.Wd(exit));

            // Mesa de hidratación
            Build.Prim(PrimitiveType.Cube, L, new Vector3(5.2f, .9f, -5.4f), new Vector3(2.4f, .08f, .8f), Mat.Lit(Pal.Hex("#dfe6ea"), .7f));
            foreach (var o in new[] { new Vector2(-1.1f, -.35f), new Vector2(1.1f, -.35f), new Vector2(-1.1f, .35f), new Vector2(1.1f, .35f) })
                Build.Prim(PrimitiveType.Cylinder, L, new Vector3(5.2f + o.x, .45f, -5.4f + o.y), new Vector3(.06f, .45f, .06f), Mat.Lit(Pal.Hex("#777777"), .6f, .6f));
            for (int i = 0; i < 6; i++) Build.Prim(PrimitiveType.Cylinder, L, new Vector3(4.4f + i * .3f, 1.05f, -5.3f), new Vector3(.07f, .11f, .07f), Mat.Lit(i % 2 == 1 ? Pal.Blue : Pal.Teal, .8f));
            Color[] fruit = { Pal.Yellow, Pal.Orange, Pal.Hex("#d63447"), Pal.Hex("#7bd13d") };
            for (int i = 0; i < 5; i++) Build.Prim(PrimitiveType.Sphere, L, new Vector3(5.9f + Random.Range(-.1f, .1f), 1.02f, -5.5f + Random.Range(-.1f, .1f)), Vector3.one * .14f, Mat.Lit(fruit[i % 4], .5f));
            Locker.gameObject.SetActive(false);
        }
        static Texture2D JerseyTexture()
        {
            var poly = new[] { new Vector2(78, 236), new Vector2(20, 196), new Vector2(42, 146), new Vector2(62, 156), new Vector2(62, 16), new Vector2(194, 16), new Vector2(194, 156), new Vector2(214, 146), new Vector2(236, 196), new Vector2(178, 236), new Vector2(128, 214) };
            return Tex.Func(256, 256, (u, v) =>
            {
                float x = u * 256, y = v * 256; bool inside = false;
                for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++) if (((poly[i].y > y) != (poly[j].y > y)) && (x < (poly[j].x - poly[i].x) * (y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)) inside = !inside;
                if (!inside) return new Color(1, 1, 1, 0);
                if (y > 186 && y < 196 && x > 62 && x < 194) return Color.white;
                return Pal.Teal;
            }, 2);
        }

    }


}
