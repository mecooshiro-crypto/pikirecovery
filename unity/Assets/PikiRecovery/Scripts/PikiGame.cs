// =====================================================================
//  PIKI RECOVERY · FÚTBOL — Flujo de la experiencia
//  Inicio → ¿Qué deporte practicás? → Cancha (final del partido)
//  → Etapa 1 Recuperación física → Etapa 2 Recuperación nutricional
//  → Etapa 3 Vuelta a la calma → RECUPERACIÓN COMPLETADA
//
//  Escenas: 00_Inicio · 01_Cancha · 02_Vestuario · 03_Calma
//  (se construyen desde el menú Piki Recovery ▸ Construir escenas).
//  Abrí 00_Inicio y apretá Play.
// =====================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Piki
{
    public static class Scenes
    {
        public const string Inicio = "00_Inicio", Cancha = "01_Cancha", Vestuario = "02_Vestuario", Calma = "03_Calma", Partido = "04_Partido";
    }
    public class Nutri { public float E = 30, H = 22, R = 18, t, balance; public int good, bad, missed, over, score, stars, level = 1; public bool running; }
    public class CalmRes { public int cycles = 6, sync, hr = 64; public bool guided = true, failed; }
    public class CardInfo { public string kicker, title, sub; public Color color = Pal.Teal; public float hold = 1.5f, fadeT = .9f; }

    // Datos que viajan de una escena a la otra durante una partida
    public static class PikiSession
    {
        public static MatchStats match; public static List<Situation> sits; public static int firstTry;
        public static Nutri nutri; public static CalmRes calm; public static CardInfo card;
        public static void NewRun() { match = MatchStats.New(); sits = null; firstTry = 0; nutri = null; calm = null; }
    }

    public class PikiGame : MonoBehaviour
    {
        public enum StartAt { Inicio, Deporte, Cancha, Fisica, Situacion, Nutricion, Minijuego, Calma, Respiracion, Final, Partido }
        [Tooltip("Etapa con la que arranca esta escena")] public StartAt startAt = StartAt.Inicio;

        // Referencias guardadas en la escena por el constructor (menú Piki Recovery)
        [SerializeField] PikiRig rig; [SerializeField] PikiEnv env;
        [SerializeField] MeshRenderer fadeR; [SerializeField] UIPanel card; [SerializeField] UIText cardKicker, cardTitle, cardSub;
        [SerializeField] PikiHud hud; [SerializeField] GameObject previewRoot;

        PikiAudio au; Material fadeM; bool leaving;
        Transform stage, screen; float floorY; PikiBody body;
        MatchStats match { get { return PikiSession.match; } set { PikiSession.match = value; } }
        List<Situation> sits { get { return PikiSession.sits; } set { PikiSession.sits = value; } }
        int firstTry { get { return PikiSession.firstTry; } set { PikiSession.firstTry = value; } }
        Nutri nutri { get { return PikiSession.nutri; } set { PikiSession.nutri = value; } }
        CalmRes calm { get { return PikiSession.calm; } set { PikiSession.calm = value; } }

        public static EnvName EnvFor(StartAt s)
        {
            if (s == StartAt.Inicio || s == StartAt.Deporte) return EnvName.Hub;
            if (s == StartAt.Nutricion || s == StartAt.Minijuego) return EnvName.Locker;
            return EnvName.Stadium;
        }
        public static StadiumMode ModeFor(StartAt s) { return s == StartAt.Calma || s == StartAt.Respiracion ? StadiumMode.Calm : s == StartAt.Final ? StadiumMode.Dawn : StadiumMode.Match; }

        /* ------------------------------ Construcción de la escena ------------------------------ */
        // Lo llama el constructor de escenas en el editor (y también en juego si la escena está vacía)
        public void Bake(bool allEnvironments, bool withPreview)
        {
            rig = PikiRig.Create(Application.isPlaying ? Camera.main : null);
            env = new GameObject("Entornos").AddComponent<PikiEnv>(); env.rig = rig;
            var need = EnvFor(startAt);
            env.Create(allEnvironments || need == EnvName.Hub, allEnvironments || need == EnvName.Stadium, allEnvironments || need == EnvName.Locker);
            BuildFadeAndCard(); BuildOverlay();
            if (match == null) match = MatchStats.New();
            env.MatchScore(match.home, match.away, match.minutes - 90);
            env.Set(need, ModeFor(startAt));
            if (withPreview)
            {
                NewStage();
                if (need == EnvName.Hub) ShowWelcome();
                else if (need == EnvName.Locker) ShowNutriIntro();
                else if (startAt == StartAt.Partido) { EnsureSampleResults(); ShowNextMatchPanel(Performance(), false); }
                else if (ModeFor(startAt) == StadiumMode.Match) ShowArrival();
                else ShowCalmIntro();
                previewRoot = stage.gameObject; previewRoot.name = "UI (vista previa · se regenera al dar Play)";
                stage = null; screen = null; Hint("");
            }
        }

        /* ------------------------------ Arranque ------------------------------ */
        void Awake()
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            if (previewRoot != null) Destroy(previewRoot);
            if (rig == null)
            {
                // Escena sin construir: se arma todo al vuelo
                Camera existing = Camera.main;
                foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None)) if (c != existing) c.gameObject.SetActive(false);
                foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) l.gameObject.SetActive(false);
                Bake(true, false);
            }
            au = gameObject.AddComponent<PikiAudio>();
            rig.onMute = () => au.SetMuted(!au.Muted);
            rig.onRecenter = Recenter;
            fadeM = fadeR.sharedMaterial != null ? new Material(fadeR.sharedMaterial) : Mat.Unlit(Color.black, null, 4000); fadeR.sharedMaterial = fadeM;
            fadeM.color = Color.black; fadeR.gameObject.SetActive(true);
        }
        void Start()
        {
            if (match == null) match = MatchStats.New();
            env.MatchScore(match.home, match.away, match.minutes - 90);
            StartCoroutine(Begin());
        }
        IEnumerator Begin()
        {
            var st = startAt;
            env.Set(EnvFor(st), ModeFor(st)); NewStage();
            if (st == StartAt.Cancha) au.CrowdStart(.42f);
            if (st == StartAt.Fisica || st == StartAt.Situacion) au.CrowdStart(.22f);
            if (st == StartAt.Calma || st == StartAt.Respiracion || st == StartAt.Final) au.PadStart(.55f);
            // Cartel de transición que viene de la escena anterior
            var c = PikiSession.card; PikiSession.card = null;
            if (c != null)
            {
                cardKicker.text = c.kicker; cardKicker.color = c.color; cardTitle.text = c.title; cardSub.text = c.sub ?? "";
                card.gameObject.SetActive(true); UI.Fade(card, 0);
                yield return Tw.Co(.5f, k => UI.Fade(card, k));
                yield return new WaitForSeconds(c.hold);
                yield return Tw.Co(.45f, k => UI.Fade(card, 1 - k)); card.gameObject.SetActive(false);
            }
            StartCoroutine(Fade(0, c != null ? c.fadeT : 1.2f));
            switch (st)
            {
                case StartAt.Deporte: ShowSport(); break;
                case StartAt.Cancha: au.Whistle(); Tw.Later(1.4f, au.CrowdCheer); ShowArrival(); break;
                case StartAt.Fisica: ShowPhysIntro(); break;
                case StartAt.Situacion: sits = Content.Generate(match); ShowSituation(0); break;
                case StartAt.Nutricion: ShowNutriIntro(); break;
                case StartAt.Minijuego: StartCoroutine(NutritionGame()); break;
                case StartAt.Calma: ShowCalmIntro(); break;
                case StartAt.Respiracion: StartCoroutine(Breathing()); break;
                case StartAt.Final:
                    if (sits == null) { sits = Content.Generate(match); firstTry = 2; }
                    if (nutri == null) nutri = new Nutri { score = 82, stars = 2, good = 19 };
                    if (calm == null) calm = new CalmRes { guided = false, sync = 86 };
                    ShowFinal(); break;
                case StartAt.Partido: au.CrowdStart(.5f); EnsureSampleResults(); StartCoroutine(NextMatch()); break;
                default: ShowWelcome(); break;
            }
        }
        void Update() { if (hud != null && rig != null) hud.hidden = rig.XR; }

        /* Pasa a otra escena (con fundido y cartel). Si la escena no está en Build Settings,
           cambia de entorno dentro de la misma escena. */
        IEnumerator Go(string scene, Action fallback, string kicker = null, string title = null, string sub = null, Color? col = null, float hold = 1.5f, float fadeT = .9f)
        {
            if (Application.CanStreamedLevelBeLoaded(scene))
            {
                leaving = true; au.Whoosh();
                yield return Fade(1, fadeT);
                PikiSession.card = title == null ? null : new CardInfo { kicker = kicker, title = title, sub = sub, color = col ?? Pal.Teal, hold = hold, fadeT = fadeT };
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
            }
            else yield return Transition(fallback, kicker, title, sub, col, hold, fadeT);
        }

        /* ------------------------------ Utilidades de escena ------------------------------ */
        void NewStage()
        {
            if (stage != null) Build.Kill(stage.gameObject);
            body = null; screen = null;
            stage = Build.Group("Escenario", null);
            float lift = rig.XR ? Mathf.Clamp(rig.HeadPos.y - 1.6f, -.6f, .4f) : 0;
            var hp = rig.HeadPos; stage.position = new Vector3(hp.x, lift, hp.z); stage.rotation = Quaternion.Euler(0, rig.Yaw, 0);
            floorY = -lift;
        }
        Transform NewScreen() { if (screen != null) Build.Kill(screen.gameObject); screen = Build.Group("Pantalla", stage); return screen; }
        void Recenter() { if (stage == null) return; var hp = rig.HeadPos; stage.position = new Vector3(hp.x, stage.position.y, hp.z); stage.rotation = Quaternion.Euler(0, rig.Yaw, 0); }
        UIPanel Panel(Vector3 pos, float w, float h, Color accent, Transform parent = null)
        {
            var c = UI.Canvas(parent ?? screen, pos, w, h); UI.PanelBg(c, accent); return c;
        }
        PikiButton Btn(string label, Vector3 pos, float w, float h, UI.Style style, Action a, Transform parent = null)
        {
            return UI.Button(parent ?? screen, pos, w, h, label, style, a);
        }
        void Hint(string s) { if (hud != null) hud.hint = s; }

        void BuildFadeAndCard()
        {
            var cam = rig.Cam.transform;
            var fm = Mat.Unlit(new Color(0, 0, 0, 0), null, 4000);
            var f = Build.MeshObj("Fundido", cam, Build.Wall(1, 1), fm); f.transform.localPosition = new Vector3(0, 0, 1.3f); f.transform.localScale = new Vector3(7, 7, 1);
            fadeR = f.GetComponent<MeshRenderer>(); fadeR.sortingOrder = 40; f.SetActive(false);
            card = UI.Canvas(cam, new Vector3(0, 0, 1.1f), 1.5f, .56f, Vector3.zero, "Cartel", UI.S, 3, 4001, 30000);
            float W = UI.Wd(card), H = UI.Ht(card);
            cardKicker = UI.T(card, "", W / 2, H * .3f, 26, Pal.Teal, true, UI.Al.C, W);
            cardTitle = UI.T(card, "", W / 2, H * .6f, 70, Color.white, true, UI.Al.C, W);
            cardSub = UI.T(card, "", W / 2, H * .84f, 26, Pal.Muted, false, UI.Al.C, W);
            card.gameObject.SetActive(false);
        }
        void BuildOverlay()
        {
            hud = new GameObject("Textos en pantalla (PC y celular)").AddComponent<PikiHud>();
        }

        IEnumerator Fade(float to, float t)
        {
            float from = fadeM.color.a; fadeR.gameObject.SetActive(true);
            yield return Tw.Co(t, k => fadeM.color = new Color(0, 0, 0, Mathf.Lerp(from, to, k)));
            if (to <= 0) fadeR.gameObject.SetActive(false);
        }
        IEnumerator Transition(Action setup, string kicker = null, string title = null, string sub = null, Color? col = null, float hold = 1.5f, float fadeT = .9f)
        {
            au.Whoosh();
            yield return Fade(1, fadeT);
            if (title != null)
            {
                cardKicker.text = kicker; cardKicker.color = col ?? Pal.Teal; cardTitle.text = title; cardSub.text = sub ?? "";
                card.gameObject.SetActive(true); yield return Tw.Co(.5f, k => UI.Fade(card, k));
            }
            setup();
            if (title != null) { yield return new WaitForSeconds(hold); yield return Tw.Co(.45f, k => UI.Fade(card, 1 - k)); card.gameObject.SetActive(false); }
            yield return Fade(0, fadeT);
        }

        /* ===================================================================
           0. BIENVENIDA
           =================================================================== */
        void ShowWelcome()
        {
            NewScreen();
            Hint("Arrastrá para mirar en 360° · Clic o toque para elegir · En VR: gatillo o mirada");
            var p = Panel(new Vector3(0, 1.86f, 2.7f), 2.6f, 1.5f, Pal.Teal); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "EXPERIENCIA INMERSIVA · 360° · VR", 60, 76, 20, Pal.Teal, true);
            UI.Icon(p, Spr.Ball, W - 110, 110, 56, Color.white);
            UI.T(p, "PIKI <color=#19e3b1>RECOVERY</color>", 54, 170, 88, Color.white, true);
            UI.T(p, "El trabajo invisible", 60, 224, 36, Pal.Orange, false, UI.Al.L, 800, true);
            UI.Wrap(p, "Todo lo que pasa después del partido, y que casi nadie ve, también es rendimiento. Acabás de jugar: ahora tu cuerpo necesita recuperarse en tres etapas.", 60, 278, W - 120, 37, 25, Pal.Body);
            var chips = new[] { new { n = "1", l = "Recuperación física", c = Pal.Red, s = Spr.BodyIcon }, new { n = "2", l = "Recuperación nutricional", c = Pal.Yellow, s = Spr.Drop }, new { n = "3", l = "Vuelta a la calma", c = Pal.Purple, s = Spr.Waves } };
            float cw = (W - 160) / 3;
            for (int i = 0; i < 3; i++)
            {
                float x = 60 + i * (cw + 20), y = 388; var ch = chips[i];
                UI.Framed(p, x, y, cw, 100, new Color(.08f, .15f, .24f, 1), Pal.A(ch.c, .55f), 20, 2);
                UI.Icon(p, Spr.Circle, x + 50, y + 50, 30, Pal.A(ch.c, .2f)); UI.Icon(p, ch.s, x + 50, y + 50, 17, ch.c);
                UI.T(p, "ETAPA " + ch.n, x + 96, y + 38, 15, ch.c, true);
                UI.Wrap(p, ch.l, x + 96, y + 64, cw - 110, 24, 20, Pal.Text, true);
            }
            UI.T(p, "Mirá a tu alrededor · Elegí con clic, toque, gatillo o sosteniendo la mirada", W / 2, H - 38, 19, Pal.Muted, false, UI.Al.C, W);
            var b = Btn("COMENZAR", new Vector3(0, .95f, 2.66f), 1.05f, .23f, UI.Style.Primary, ShowSport);
            UI.Appear(p); UI.Appear(b, .25f);
        }

        /* ===================================================================
           1. ELECCIÓN DE DEPORTE
           =================================================================== */
        void ShowSport()
        {
            NewScreen();
            Hint("Elegí tu deporte · Esta versión está dedicada al fútbol");
            var p = Panel(new Vector3(0, 1.73f, 2.75f), 2.7f, 1.75f, Pal.Teal); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "PASO 1 · TU DISCIPLINA", 56, 66, 19, Pal.Teal, true);
            UI.T(p, "¿Qué deporte practicás?", 56, 128, 54, Color.white, true);
            UI.T(p, "Tu elección define el espacio en el que vas a aparecer.", 56, 168, 23, Pal.Muted);
            var msg = UI.T(p, "Esta versión de Piki Recovery está dedicada al fútbol.", W / 2, H - 30, 20, Pal.Muted, true, UI.Al.C, W);
            UI.Appear(p);
            string[] names = { "Fútbol", "Básquet", "Vóley", "Tenis", "Hockey", "Rugby" };
            for (int i = 0; i < 6; i++)
            {
                bool ok = i == 0; string name = names[i];
                float x = i % 3 == 0 ? -.84f : i % 3 == 1 ? 0 : .84f, y = i < 3 ? 1.83f : 1.3f;
                var c = UI.Canvas(screen, new Vector3(x, y, 2.73f), .76f, .48f, Vector3.zero, "Deporte " + name); float cw = UI.Wd(c), ch = UI.Ht(c);
                var border = UI.Round(c, 6, 6, cw - 12, ch - 12, ok ? Pal.Teal : new Color(1, 1, 1, .18f), 26);
                UI.Round(c, 9, 9, cw - 18, ch - 18, ok ? new Color(.07f, .32f, .31f, 1) : new Color(.07f, .12f, .19f, 1), 23);
                if (ok) UI.Icon(c, Spr.Ball, cw / 2, ch * .4f, ch * .21f, Color.white);
                else { UI.Icon(c, Spr.Circle, cw / 2, ch * .4f, ch * .2f, new Color(1, 1, 1, .08f)); UI.T(c, name.Substring(0, 1), cw / 2, ch * .4f + 22, 60, Pal.Dim, true, UI.Al.C, 100); UI.Icon(c, Spr.Lock, cw - 44, 44, 16, Pal.Dim); }
                UI.T(c, name, cw / 2, ch * .76f, 34, ok ? Color.white : Pal.Dim, true, UI.Al.C, cw);
                if (ok) { float w = UI.TextWidth("DISPONIBLE", 15, true) + 28; UI.Chip(c, cw / 2 - w / 2, ch * .82f, "DISPONIBLE", Pal.Teal, 15, 30, 14); }
                else UI.T(c, "PRÓXIMAMENTE", cw / 2, ch * .92f, 16, Pal.Dim, true, UI.Al.C, cw);
                var b = UI.AttachButton(c);
                b.onHover = h => { border.color = ok ? (h ? Color.white : Pal.Teal) : new Color(1, 1, 1, h ? .35f : .18f); };
                b.onClick = () =>
                {
                    if (!ok) { au.Soft(); msg.text = name + " va a estar disponible próximamente. Elegí Fútbol para empezar."; msg.color = Pal.Orange; b.Shake(); return; }
                    StartCoroutine(StartMatch());
                };
                UI.Appear(c, .12f + i * .07f);
            }
        }

        /* ===================================================================
           2. APARICIÓN EN LA CANCHA
           =================================================================== */
        IEnumerator StartMatch()
        {
            PikiSession.NewRun(); env.MatchScore(match.home, match.away, match.minutes - 90);
            au.PadStop(1); au.BeatStop(.5f);
            yield return Go(Scenes.Cancha, () => { env.Set(EnvName.Stadium, StadiumMode.Match); NewStage(); NewScreen(); au.CrowdStart(.42f); },
                "FÚTBOL", "Estadio Piki", "Final del partido · Piki FC " + match.home + " – " + match.away + " Visitante", Pal.Teal, 1.8f);
            if (leaving) yield break;
            au.Whistle(); Tw.Later(1.4f, au.CrowdCheer);
            ShowArrival();
        }
        void ShowArrival()
        {
            NewScreen();
            Hint("Mirá a tu alrededor: el partido terminó y el estadio empieza a vaciarse");
            var m = match;
            var p = Panel(new Vector3(0, 1.84f, 2.8f), 2.7f, 1.62f, Pal.Orange); float W = UI.Wd(p);
            UI.T(p, "FÚTBOL · ESTADIO PIKI · 90+" + (m.minutes - 90) + "'", 56, 66, 19, Pal.Orange, true);
            UI.Stepper(p, W - 420, 70, 380, 0, Pal.Orange);
            UI.T(p, "¡Final del partido!", 56, 132, 60, Color.white, true);
            UI.T(p, "El esfuerzo terminó. La recuperación recién comienza.", 56, 178, 26, Pal.Teal, true);
            var stats = new[] { new[] { m.minutes + "'", "Minutos jugados" }, new[] { Content.F1(m.km) + " km", "Distancia recorrida" }, new[] { m.sprints.ToString(), "Sprints" },
                                new[] { m.fcmax + " lpm", "Frecuencia cardíaca máx." }, new[] { "≈" + Content.F1(m.sweat) + " L", "Líquido perdido (sudor)" }, new[] { Content.Thousands(m.kcal) + " kcal", "Energía gastada" } };
            float cw = (W - 152) / 3;
            for (int i = 0; i < 6; i++)
            {
                float x = 56 + (i % 3) * (cw + 20), y = 208 + (i / 3) * 116;
                UI.Framed(p, x, y, cw, 100, new Color(.08f, .15f, .23f, 1), new Color(1, 1, 1, .12f), 18, 2);
                UI.T(p, stats[i][0], x + 22, y + 52, 38, Color.white, true); UI.T(p, stats[i][1], x + 22, y + 84, 18, Pal.Muted);
            }
            UI.Wrap(p, "Lo que hagas en las próximas horas es trabajo invisible: nadie lo ve, pero define cómo llegás al próximo entrenamiento y al próximo partido.", 56, 478, W - 112, 32, 23, Pal.Body);
            var b = Btn("INICIAR RECUPERACIÓN  →", new Vector3(0, .9f, 2.76f), 1.3f, .23f, UI.Style.Primary, ShowPhysIntro);
            UI.Appear(p, 2.8f, .7f); UI.Appear(b, 3.3f);
        }

        /* ===================================================================
           3. ETAPA 1 · RECUPERACIÓN FÍSICA
           =================================================================== */
        void EnsureBody()
        {
            if (body != null) return;
            body = PikiBody.Create(stage, new Vector3(1.55f, floorY, 2.45f));
            var lab = UI.Canvas(stage, new Vector3(1.55f, 2.1f + floorY, 2.45f), .9f, .16f, new Vector3(0, 32, 0), "Escaneo");
            UI.T(lab, "ESCANEO CORPORAL", UI.Wd(lab) / 2, 42, 26, Pal.Cyan, true, UI.Al.C, UI.Wd(lab));
            UI.Appear(lab, .4f);
        }
        void ShowPhysIntro()
        {
            sits = Content.Generate(match); au.CrowdLevel(.22f, 3);
            NewScreen(); EnsureBody(); body.HideMarker();
            Hint("Etapa 1 · Recuperación física");
            var p = Panel(new Vector3(-.4f, 1.82f, 2.75f), 2.25f, 1.55f, Pal.Red); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "ETAPA 1 DE 3", 48, 62, 19, Pal.Red, true); UI.Stepper(p, W - 400, 58, 370, 0, Pal.Red);
            UI.T(p, "Recuperación física", 48, 150, 54, Color.white, true);
            UI.Wrap(p, "Después del partido tu cuerpo te muestra señales. En cada situación el sistema genera al azar qué sentís, en qué zona y con qué intensidad.", 48, 204, W - 96, 34, 24, Pal.Body);
            float x = 48; foreach (Kind k in Enum.GetValues(typeof(Kind))) x += UI.Chip(p, x, 300, Content.KindLabel(k), Content.KindColor(k), 18) + 12;
            UI.Wrap(p, "Leé las señales, decidí qué necesita la zona y aplicalo vos: agarrá la bolsa de hielo, la compresa tibia o la pelota de masaje y pasala por la zona marcada.", 48, 390, W - 96, 34, 24, Pal.Text, true);
            UI.T(p, "3 situaciones · distintas en cada partida", 48, H - 42, 19, Pal.Muted);
            var b = Btn("VER SITUACIÓN 1  →", new Vector3(-.4f, .9f, 2.72f), 1.1f, .22f, UI.Style.Primary, () => ShowSituation(0));
            UI.Appear(p); UI.Appear(b, .25f);
        }

        void ShowSituation(int i)
        {
            NewScreen(); EnsureBody();
            var s = sits[i]; body.SetMarker(s.pos); body.spin = false;
            Hint("Situación " + (i + 1) + " de 3 · Arrastrá la herramienta correcta hasta la zona marcada y mantenela ahí");
            Color tc = Content.KindColor(s.kind);
            var p = Panel(new Vector3(-.42f, 1.86f, 2.75f), 2.2f, 1.62f, tc); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "ETAPA 1 · RECUPERACIÓN FÍSICA", 44, 58, 17, Pal.Red, true);
            UI.T(p, "SITUACIÓN " + (i + 1) + " / 3", W - 44, 58, 17, Pal.Muted, true, UI.Al.R, 400);
            string kl = Content.KindLabel(s.kind); float bw = UI.TextWidth(kl, 26, true) + 40;
            UI.Round(p, 44, 82, bw, 50, tc, 25); UI.T(p, kl, 64, 117, 26, Pal.Hex("#150a05"), true);
            UI.T(p, "INTENSIDAD " + s.intensity.ToUpper() + " · " + s.level + "/10", W - 44, 100, 17, Pal.Text, true, UI.Al.R, 500);
            for (int k = 0; k < 10; k++) UI.Round(p, W - 44 - 250 + k * 25.5f, 112, 20, 16, k < s.level ? Color.HSVToRGB((50 - k * 5.5f) / 360f, .85f, 1f) : new Color(1, 1, 1, .12f), 4);
            UI.T(p, s.zone.name, 44, 192, 44, Color.white, true);
            UI.T(p, s.zone.desc + (s.side != null ? " · lado " + s.side : ""), 44, 226, 21, Pal.Muted, true);
            float y = UI.Wrap(p, s.text, 44, 272, W - 88, 32, 23, Pal.Hex("#dbe6f2"));
            float x = 44; foreach (var sg in s.signals) x += UI.Chip(p, x, y - 4, sg, tc, 16, 34, 13) + 10;
            var box = UI.Rect(p, 0, 0, W, H, "feedback");
            Action<string> mode = null;
            UIImage barFill = null; UIText timeText = null, instrText = null;
            mode = md =>
            {
                foreach (Transform ch in box.transform) Destroy(ch.gameObject);
                float by = H - 196, bh = 170; Color col = Content.TColor(s.correct);
                Color bc = md == "question" ? Pal.Teal : md == "wrong" ? Pal.Red : md == "done" ? Pal.Green : col;
                UI.Framed(box, 30, by, W - 60, bh, Color.Lerp(UI.PanelA, bc, .14f), Pal.A(bc, .6f), 22, 2.5f);
                if (md == "question")
                {
                    UI.T(box, "¿Qué necesita esta zona?", W / 2, by + 74, 40, Color.white, true, UI.Al.C, W);
                    UI.T(box, "Agarrá una herramienta de abajo y pasala por la zona marcada en el cuerpo", W / 2, by + 118, 20, Pal.Muted, true, UI.Al.C, W);
                }
                else if (md == "wrong")
                {
                    UI.Icon(box, Spr.Circle, 76, by + 50, 22, Pal.Red); UI.Icon(box, Spr.Cross, 76, by + 50, 11, Color.white);
                    UI.T(box, "No es la mejor opción para esta situación.", 112, by + 59, 26, Pal.Hex("#ffb3bb"), true);
                    float yy = UI.Wrap(box, Content.Hint(s.correct, s.wrongs[s.wrongs.Count - 1]), 60, by + 100, W - 120, 28, 20, Pal.Hex("#f3dde0"));
                    UI.T(box, "Probá con otra herramienta.", 60, Mathf.Min(yy + 4, by + bh - 14), 19, Pal.Text, true);
                }
                else if (md == "applying")
                {
                    UI.Icon(box, Content.TIcon(s.correct), 76, by + 48, 22, col);
                    UI.T(box, "Respuesta correcta · " + Content.TVerb(s.correct), 112, by + 57, 26, Pal.Hex("#b9f7dc"), true);
                    timeText = UI.T(box, "00:00 / " + Content.TMins(s.correct) + ":00", W - 60, by + 57, 24, Pal.Text, true, UI.Al.R, 400);
                    instrText = UI.T(box, "", 60, by + 98, 21, Color.white, true);
                    UIImage track; barFill = UI.Bar(box, 60, by + 116, W - 120, 20, 0, col, out track);
                    UI.T(box, Content.THow(s.correct), 60, by + 160, 18, Pal.Muted, true);
                }
                else
                {
                    UI.Icon(box, Spr.Circle, 76, by + 48, 22, Pal.Green); UI.Icon(box, Spr.Check, 76, by + 48, 12, Pal.Ink);
                    UI.T(box, "Zona recuperada · " + (s.attempts == 1 ? "acertaste a la primera" : "lo resolviste en " + s.attempts + " intentos"), 112, by + 57, 25, Pal.Hex("#b9f7dc"), true);
                    UI.Wrap(box, Content.TExpl(s.correct), 60, by + 98, W - 120, 27, 19, Pal.Hex("#e0f5ec"));
                }
            };
            mode("question");
            UI.Appear(p);
            var tag = UI.Canvas(screen, new Vector3(1.55f, 2.3f + floorY, 2.45f), .9f, .14f, new Vector3(0, 32, 0), "Zona");
            UI.Framed(tag, 4, 4, UI.Wd(tag) - 8, UI.Ht(tag) - 8, new Color(.02f, .04f, .09f, .85f), tc, 24, 3);
            UI.T(tag, s.zone.name.ToUpper() + (s.side != null ? " · " + s.side.ToUpper() : ""), UI.Wd(tag) / 2, 37, 24, tc, true, UI.Al.C, UI.Wd(tag));
            UI.Appear(tag, .3f);

            // El cuerpo muestra la zona hacia el usuario y se queda quieto
            body.FaceZone(s.pos);

            // Herramientas: bolsa de hielo, compresa tibia y pelota de masaje
            var toolsRoot = Build.Group("Herramientas", screen);
            var tools = new List<PikiTool>(); var opts = new[] { Treat.Frio, Treat.Calor, Treat.Masaje };
            for (int k = 0; k < 3; k++)
            {
                var opt = opts[k]; Vector3 home = new Vector3(-1.13f + k * .71f, .9f, 2.6f); Color oc = Content.TColor(opt);
                var ped = Build.MeshObj("Base", toolsRoot, Build.Floor(.42f, .42f), Mat.Unlit(Pal.A(oc, .7f), Spr.Ring.texture, 3000)); ped.transform.localPosition = home + new Vector3(0, -.14f, 0);
                Build.Glow(toolsRoot, home + new Vector3(0, -.13f, 0), .5f, Pal.A(oc, .25f), false).transform.localEulerAngles = new Vector3(90, 0, 0);
                var lab = UI.Canvas(toolsRoot, home + new Vector3(0, -.24f, -.02f), .6f, .14f, Vector3.zero, "Etiqueta " + Content.TLabel(opt));
                UI.T(lab, Content.TLabel(opt), UI.Wd(lab) / 2, 34, 30, Color.white, true, UI.Al.C, UI.Wd(lab));
                UI.T(lab, opt == Treat.Frio ? "bolsa de hielo" : opt == Treat.Calor ? "compresa tibia" : "pelota de masaje", UI.Wd(lab) / 2, 52, 15, oc, true, UI.Al.C, UI.Wd(lab));
                UI.Appear(lab, .2f + k * .09f);
                var tool = ToolModel.Make(opt, toolsRoot, home); tools.Add(tool);
            }
            PikiTool.Focus = body.MarkerT;
            StartCoroutine(SituationLoop(s, i, screen, mode, () => barFill, () => timeText, () => instrText, toolsRoot, tools));
        }

        IEnumerator SituationLoop(Situation s, int i, Transform scr, Action<string> mode, Func<UIImage> bar, Func<UIText> time, Func<UIText> instr, Transform toolsRoot, List<PikiTool> tools)
        {
            float progress = 0, tick = 0, idleMsg = 0; bool decided = false; const float DUR = 5.5f; float BW = 880 - 120;
            int mins = Content.TMins(s.correct);
            string keep = s.correct == Treat.Frio ? "Mantené la bolsa de hielo apoyada sobre la zona" : s.correct == Treat.Calor ? "Mantené la compresa tibia apoyada sobre la zona" : "Mové la pelota en círculos suaves sobre la zona";
            while (!s.done)
            {
                if (scr == null || body == null) yield break;
                var held = rig.Held;
                bool near = held != null && Vector3.Distance(held.transform.position, body.MarkerT.position) < .24f;
                body.Highlight(held != null && Vector3.Distance(held.transform.position, body.MarkerT.position) < .45f);
                if (near && !decided)
                {
                    s.attempts++;
                    if (held.kind == s.correct)
                    {
                        decided = true; s.firstTry = s.attempts == 1; au.Correct();
                        foreach (var t in tools) if (t != held) t.gameObject.SetActive(false);
                        body.StartFX(s.correct); body.SetFX(false); mode("applying");
                    }
                    else
                    {
                        au.Wrong(); s.wrongs.Add(held.kind); rig.ForceRelease(); held.SetDisabled(true); mode("wrong");
                    }
                }
                if (decided)
                {
                    float rate = 0;
                    if (near && held.kind == s.correct) rate = s.correct == Treat.Masaje ? Mathf.Clamp01(held.Speed / .5f) : 1f;
                    body.SetFX(rate > .15f);
                    progress = Mathf.Clamp01(progress + Time.deltaTime * rate / DUR);
                    tick -= Time.deltaTime; if (rate > .15f && tick <= 0) { tick = .45f; au.ApplyTick(progress); rig.Haptic(); }
                    idleMsg -= Time.deltaTime;
                    if (instr() != null && idleMsg <= 0)
                    {
                        idleMsg = .2f;
                        instr().text = rate > .15f ? keep : (held == null ? "Agarrá de nuevo la herramienta y llevala a la zona marcada" : s.correct == Treat.Masaje && near ? "Mové la pelota en círculos para hacer el masaje" : "Acercá la herramienta a la zona marcada");
                        instr().color = rate > .15f ? Color.white : Pal.Yellow;
                    }
                    if (bar() != null) UI.SetBar(bar(), BW, 20, progress);
                    if (time() != null) { float m = mins * progress; int mm = Mathf.FloorToInt(m), ss = Mathf.FloorToInt((m - mm) * 60); time().text = mm.ToString("00") + ":" + ss.ToString("00") + " / " + mins + ":00"; }
                    body.SetColor(Color.Lerp(Pal.Red, Pal.Green, progress));
                    if (progress >= 1) s.done = true;
                }
                yield return null;
            }
            rig.ForceRelease(); body.SetFX(false); body.StopFX(); body.SetColor(Pal.Green); body.Highlight(false); au.Correct();
            mode("done");
            if (toolsRoot != null) Destroy(toolsRoot.gameObject);
            bool last = i == sits.Count - 1;
            var nb = Btn(last ? "VER RESUMEN  →" : "SIGUIENTE SITUACIÓN  →", new Vector3(-.42f, .84f, 2.7f), 1.15f, .23f, UI.Style.Primary, () => { body.spin = true; if (last) ShowPhysSummary(); else ShowSituation(i + 1); });
            UI.Appear(nb, .3f);
        }
        void ShowPhysSummary()
        {
            NewScreen(); body.HideMarker(); body.spin = true;
            Hint("Recuperación física completada");
            firstTry = sits.Count(x => x.firstTry);
            var p = Panel(new Vector3(-.4f, 1.84f, 2.75f), 2.3f, 1.62f, Pal.Green); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "ETAPA 1 DE 3 · COMPLETADA", 48, 62, 19, Pal.Green, true); UI.Stepper(p, W - 400, 58, 370, 1, Pal.Green);
            UI.T(p, "Recuperación física", 48, 140, 50, Color.white, true);
            UI.T(p, firstTry + " de 3 decisiones correctas a la primera", 48, 180, 24, Pal.Teal, true);
            for (int k = 0; k < sits.Count; k++)
            {
                var s = sits[k]; float y = 210 + k * 84; Color c = Content.TColor(s.correct);
                UI.Round(p, 48, y, W - 96, 72, new Color(1, 1, 1, .05f), 16);
                UI.Icon(p, Spr.Circle, 88, y + 36, 24, Pal.A(c, .22f)); UI.Icon(p, Content.TIcon(s.correct), 88, y + 36, 14, c);
                UI.T(p, s.zone.name + (s.side != null ? " · " + s.side : ""), 128, y + 32, 24, Color.white, true);
                UI.T(p, Content.KindLabel(s.kind).ToLower() + " " + s.intensity + " → " + Content.TLabel(s.correct).ToLower() + " · " + Content.THow(s.correct).Split('·')[1].Trim(), 128, y + 58, 18, Pal.Muted, true);
                UI.T(p, s.firstTry ? "A la primera" : s.attempts + " intentos", W - 70, y + 44, 19, s.firstTry ? Pal.Green : Pal.Orange, true, UI.Al.R, 300);
            }
            UI.Wrap(p, "Trabajo invisible: elegir bien entre frío, calor o descarga reduce el riesgo de lesión y acelera la vuelta al entrenamiento.", 48, H - 62, W - 96, 30, 21, Pal.Body, false, true);
            var b = Btn("CONTINUAR · ETAPA 2  →", new Vector3(-.4f, .88f, 2.72f), 1.2f, .22f, UI.Style.Primary, () => StartCoroutine(StartNutrition()));
            UI.Appear(p); UI.Appear(b, .3f);
        }

        /* ===================================================================
           4. ETAPA 2 · RECUPERACIÓN NUTRICIONAL
           =================================================================== */
        static readonly string[] BarName = { "Energía", "Hidratación", "Reparación" };
        static readonly Color[] BarCol = { Pal.Yellow, Pal.Blue, Pal.Orange };
        static Sprite BarIcon(int i) { return i == 0 ? Spr.Bolt : i == 1 ? Spr.Drop : Spr.Dumbbell; }
        const float ZMIN = 60, ZMAX = 90;
        static float ZoneScore(float v) { return v >= ZMIN && v <= ZMAX ? 100 : v < ZMIN ? v / ZMIN * 100 : Mathf.Max(40, 100 - (v - ZMAX) * 4); }

        IEnumerator StartNutrition()
        {
            au.CrowdStop(1.5f);
            yield return Go(Scenes.Vestuario, () => { env.Set(EnvName.Locker); NewStage(); ShowNutriIntro(); },
                "ETAPA 2 DE 3", "Recuperación nutricional", "Vestuario · Ventana de recuperación post-partido", Pal.Yellow, 1.8f);
        }
        void ShowNutriIntro()
        {
            NewScreen(); Hint("Etapa 2 · Recuperación nutricional");
            var p = Panel(new Vector3(0, 1.84f, 2.75f), 2.5f, 1.62f, Pal.Yellow); float W = UI.Wd(p);
            UI.T(p, "ETAPA 2 DE 3", 52, 62, 19, Pal.Yellow, true); UI.Stepper(p, W - 410, 58, 370, 1, Pal.Yellow);
            UI.T(p, "Recuperación nutricional", 52, 146, 52, Color.white, true);
            UI.Wrap(p, "Perdiste cerca de " + Content.F1(match.sweat) + " L de sudor y gastaste " + Content.Thousands(match.kcal) + " kcal. Ahora tu cuerpo necesita recuperar energía y recursos.", 52, 196, W - 104, 32, 23, Pal.Body);
            float cw = (W - 136) / 3; string[] sub = { "Carbohidratos", "Líquidos y sales", "Proteínas" };
            for (int i = 0; i < 3; i++)
            {
                float x = 52 + i * (cw + 16), y = 268;
                UI.Framed(p, x, y, cw, 112, Color.Lerp(UI.PanelA, BarCol[i], .1f), Pal.A(BarCol[i], .5f), 18, 2);
                UI.Icon(p, BarIcon(i), x + 42, y + 42, 20, BarCol[i]); UI.T(p, BarName[i], x + 76, y + 51, 25, Color.white, true); UI.T(p, sub[i], x + 22, y + 92, 19, Pal.Muted, true);
            }
            UI.T(p, "ELEGÍ LO QUE TU CUERPO NECESITA PARA RECUPERARSE", 52, 428, 22, Pal.Yellow, true);
            UI.Wrap(p, "Seleccioná los alimentos que pasan a tu alrededor. Mantené las tres barras en la zona verde (60–90%): lo que falta baja el rendimiento y lo que sobra también desequilibra. Algunos alimentos no son prioritarios. Los niveles bajan con el tiempo y todo se acelera.", 52, 464, W - 104, 29, 20, Pal.Text);
            var b = Btn("EMPEZAR MINIJUEGO  →", new Vector3(0, .9f, 2.72f), 1.2f, .23f, UI.Style.Warm, () => StartCoroutine(NutritionGame()));
            UI.Appear(p); UI.Appear(b, .3f);
        }

        class Item { public Food f; public Transform root, model; public PikiButton btn; public float a, dir, r, h, w, ph; public bool alive = true; public GameObject ring; }

        IEnumerator NutritionGame()
        {
            var scr = NewScreen();
            Hint("Seleccioná los alimentos que tu cuerpo necesita · Mantené las barras en la zona verde");
            var N = new Nutri(); nutri = N;
            // HUD
            var hud = UI.Canvas(scr, new Vector3(0, 2.72f, 3.6f), 3.3f, .76f, new Vector3(-14, 0, 0), "HUD"); UI.PanelBg(hud, Pal.Yellow);
            float HW = UI.Wd(hud);
            UI.T(hud, "TIEMPO", 44, 62, 17, Pal.Muted, true);
            var tTime = UI.T(hud, "1:00", 44, 142, 76, Color.white, true);
            var tLevel = UI.T(hud, "NIVEL 1 / 3", 44, 190, 20, Pal.Yellow, true);
            var lvlDots = new UIImage[3]; for (int i = 0; i < 3; i++) lvlDots[i] = UI.Round(hud, 44 + i * 44, 204, 36, 10, i == 0 ? Pal.Yellow : new Color(1, 1, 1, .15f), 5);
            var tScore = UI.T(hud, "Elegidos 0 · No prioritarios 0", 44, 256, 20, Pal.Text, true);
            float bx = 330, x0 = bx + 200, bw = HW - bx - 170 - 200;
            var fills = new UIImage[3]; var vals = new UIText[3]; var sts = new UIText[3];
            for (int i = 0; i < 3; i++)
            {
                float y = 40 + i * 78;
                UI.Icon(hud, BarIcon(i), bx, y + 26, 16, BarCol[i]); UI.T(hud, BarName[i], bx + 28, y + 35, 24, Color.white, true);
                UI.Round(hud, x0, y + 8, bw, 32, new Color(1, 1, 1, .08f), 10);
                UI.Img(hud, x0 + bw * ZMIN / 100, y + 8, bw * (ZMAX - ZMIN) / 100, 32, null, new Color(.24f, .86f, .59f, .2f));
                UI.Img(hud, x0 + bw * ZMIN / 100 - 1, y + 2, 2, 44, null, Pal.A(Pal.Green, .8f)); UI.Img(hud, x0 + bw * ZMAX / 100 - 1, y + 2, 2, 44, null, Pal.A(Pal.Green, .8f));
                fills[i] = UI.Round(hud, x0, y + 8, 12, 32, BarCol[i], 10);
                for (int k = 1; k < 10; k++) UI.Img(hud, x0 + bw * k / 10 - 1.5f, y + 8, 3, 32, null, new Color(.02f, .04f, .09f, .55f));
                vals[i] = UI.T(hud, "0%", x0 + bw + 20, y + 34, 26, Color.white, true); sts[i] = UI.T(hud, "BAJO", x0 + bw + 20, y + 58, 15, Pal.Orange, true);
            }
            Action refreshHud = () =>
            {
                float left = Mathf.Max(0, 60 - N.t); int t = Mathf.CeilToInt(left);
                tTime.text = (t / 60) + ":" + (t % 60).ToString("00"); tTime.color = left < 10 ? Pal.Red : Color.white;
                tLevel.text = "NIVEL " + N.level + " / 3"; for (int i = 0; i < 3; i++) lvlDots[i].color = i < N.level ? Pal.Yellow : new Color(1, 1, 1, .15f);
                tScore.text = "Elegidos " + N.good + " · No prioritarios " + N.bad;
                float[] v = { N.E, N.H, N.R };
                for (int i = 0; i < 3; i++)
                {
                    fills[i].Size = new Vector2(Mathf.Max(12, bw * v[i] / 100), 32);
                    vals[i].text = Mathf.RoundToInt(v[i]) + "%";
                    sts[i].text = v[i] < ZMIN ? "BAJO" : v[i] > ZMAX ? "EXCESO" : "ÓPTIMO"; sts[i].color = v[i] < ZMIN ? (v[i] < 30 ? Pal.Red : Pal.Orange) : v[i] > ZMAX ? Pal.Yellow : Pal.Green;
                }
            };
            refreshHud(); UI.Appear(hud);

            // Cuenta regresiva
            var cd = UI.Canvas(scr, new Vector3(0, 1.65f, 2.6f), 1.4f, .7f, Vector3.zero, "Cuenta");
            var cdt = UI.T(cd, "3", UI.Wd(cd) / 2, 220, 190, Color.white, true, UI.Al.C, UI.Wd(cd));
            foreach (var v in new[] { "3", "2", "1", "¡YA!" })
            {
                cdt.text = v; cdt.fontSize = v.Length > 1 ? 110 : 190; cdt.color = v.Length > 1 ? Pal.Yellow : Color.white; au.Beep(v.Length > 1);
                yield return Tw.Co(.7f, k => { cd.localScale = Vector3.one * UI.S * (1.25f - .25f * k); UI.Fade(cd, v.Length > 1 ? 1 - k : 1); }, Ease.Out);
                if (scr == null) yield break;
                if (v.Length == 1) yield return new WaitForSeconds(.15f);
            }
            Destroy(cd.gameObject);

            var banner = UI.Canvas(scr, new Vector3(0, 2.15f, 3.2f), 2.4f, .4f, Vector3.zero, "Aviso");
            UI.Framed(banner, 6, 6, UI.Wd(banner) - 12, UI.Ht(banner) - 12, new Color(.02f, .04f, .09f, .88f), Pal.Yellow, 74, 4);
            var bt = UI.T(banner, "", UI.Wd(banner) / 2, 100, 50, Pal.Yellow, true, UI.Al.C, UI.Wd(banner));
            banner.gameObject.SetActive(false);
            Func<string, IEnumerator> showBanner = txt => BannerCo(banner, bt, txt);

            au.BeatStart(1); N.running = true; rig.FuseTime = .75f;
            var items = new List<Item>();
            var good = Content.Foods.Where(f => f.good).ToArray(); var bad = Content.Foods.Where(f => !f.good).ToArray();
            var byCat = new[] { good.Where(f => f.E >= Mathf.Max(f.H, f.R)).ToArray(), good.Where(f => f.H > f.E && f.H >= f.R).ToArray(), good.Where(f => f.R > f.E && f.R > f.H).ToArray() };
            float[] spawnEvery = { 0, 1.45f, 1f, .68f }, speed = { 0, 1f, 1.45f, 1.95f }, badP = { 0, .26f, .32f, .38f }, decay = { 0, .45f, .65f, .85f };
            float spawnT = 1, lastLevel = 1, hudT = 0;

            Action<Item> collect = it =>
            {
                it.alive = false; items.Remove(it); var f = it.f; Vector3 wp = it.root.position;
                float bE = N.E, bH = N.H, bR = N.R;
                N.E = Mathf.Clamp(N.E + f.E, 0, 100); N.H = Mathf.Clamp(N.H + f.H, 0, 100); N.R = Mathf.Clamp(N.R + f.R, 0, 100);
                if (f.good)
                {
                    N.good++; au.Good(); Burst(wp, Pal.Teal);
                    int main = f.E >= f.H && f.E >= f.R ? 0 : f.H >= f.R ? 1 : 2; int amount = main == 0 ? f.E : main == 1 ? f.H : f.R;
                    bool over = (f.E > 0 && bE + f.E > ZMAX) || (f.H > 0 && bH + f.H > ZMAX) || (f.R > 0 && bR + f.R > ZMAX); if (over) N.over++;
                    FloatText(scr, it.root.localPosition, "+" + amount + " " + BarName[main] + (over ? " · ¡ojo con el exceso!" : ""), over ? Pal.Yellow : Pal.Teal);
                }
                else { N.bad++; au.Bad(); Burst(wp, Pal.Red); FloatText(scr, it.root.localPosition, "× " + f.tip, Pal.Red); }
                Destroy(it.root.gameObject);
                refreshHud();
            };
            Action<int> spawn = lvl =>
            {
                Food f;
                if (Random.value < badP[lvl]) f = bad[Random.Range(0, bad.Length)];
                else { var cat = byCat[Random.Range(0, 3)]; f = cat[Random.Range(0, cat.Length)]; }
                var it = new Item { f = f, dir = Random.value < .5f ? 1 : -1, r = Random.Range(3.4f, 4.4f), h = Random.Range(.95f, 2.25f), ph = Random.Range(0f, 6f) };
                it.a = -it.dir * 1.35f; it.w = speed[lvl] * Random.Range(.85f, 1.15f) / it.r;
                it.root = Build.Group("Alimento", scr);
                it.model = FoodModel.Make(f, it.root); it.model.localScale = Vector3.one * 1.25f;
                var lab = UI.Canvas(it.root, new Vector3(0, -.3f, 0), .9f, .14f, Vector3.zero, "Etiqueta");
                float lw = UI.TextWidth(f.name, 30, true) + 36; UI.Framed(lab, UI.Wd(lab) / 2 - lw / 2, 4, lw, 48, new Color(.02f, .04f, .09f, .9f), new Color(1, 1, 1, .4f), 24, 2);
                UI.T(lab, f.name, UI.Wd(lab) / 2, 40, 30, Color.white, true, UI.Al.C, UI.Wd(lab));
                it.ring = Build.Glow(it.root, Vector3.zero, .9f, Pal.A(Pal.Teal, .7f)); it.ring.SetActive(false);
                var col = it.root.gameObject.AddComponent<SphereCollider>(); col.radius = .32f; col.center = new Vector3(0, -.05f, 0);
                it.btn = it.root.gameObject.AddComponent<PikiButton>(); it.btn.hoverScale = 1.15f;
                it.btn.onHover = h => { if (it.ring != null) it.ring.SetActive(h); };
                it.btn.onClick = () => { if (it.alive && N.running) collect(it); };
                items.Add(it);
            };

            while (N.running)
            {
                float dt = Time.deltaTime; N.t += dt;
                int lvl = N.t < 20 ? 1 : N.t < 40 ? 2 : 3; N.level = lvl;
                if (lvl != lastLevel) { lastLevel = lvl; au.Level(); au.BeatLevel(lvl); StartCoroutine(showBanner(lvl == 2 ? "NIVEL 2 · ¡Más rápido!" : "NIVEL 3 · ¡Máxima presión!")); rig.FuseTime = lvl == 3 ? .55f : .65f; }
                N.E = Mathf.Clamp(N.E - decay[lvl] * dt, 0, 100); N.H = Mathf.Clamp(N.H - decay[lvl] * 1.15f * dt, 0, 100); N.R = Mathf.Clamp(N.R - decay[lvl] * .75f * dt, 0, 100);
                if (N.E >= ZMIN && N.E <= ZMAX && N.H >= ZMIN && N.H <= ZMAX && N.R >= ZMIN && N.R <= ZMAX) N.balance += dt;
                spawnT -= dt; if (spawnT <= 0) { spawn(lvl); if (lvl == 3 && Random.value < .3f) spawn(lvl); spawnT = spawnEvery[lvl] * Random.Range(.8f, 1.2f); }
                foreach (var it in items.ToArray())
                {
                    it.a += it.dir * it.w * dt; float x = Mathf.Sin(it.a) * it.r, z = Mathf.Cos(it.a) * it.r;
                    it.root.localPosition = new Vector3(x, it.h + Mathf.Sin(Time.time * 2.2f + it.ph) * .08f, z);
                    it.root.localRotation = Quaternion.LookRotation(new Vector3(x, 0, z));
                    it.model.localRotation = Quaternion.Euler(Mathf.Sin(Time.time + it.ph) * 15, Time.time * 60 + it.ph * 30, 0);
                    if (Mathf.Abs(it.a) > 1.4f && Mathf.Sign(it.a) == it.dir) { it.alive = false; items.Remove(it); if (it.f.good) N.missed++; Destroy(it.root.gameObject); }
                }
                hudT -= dt; if (hudT <= 0) { hudT = .1f; refreshHud(); }
                if (N.t >= 60) N.running = false;
                yield return null;
            }
            au.BeatStop(1.5f); au.Whistle(true); rig.FuseTime = 1.4f;
            foreach (var it in items) if (it.root != null) Destroy(it.root.gameObject);
            items.Clear(); refreshHud();
            yield return showBanner("¡TIEMPO!");
            ShowNutriResults();
        }
        IEnumerator BannerCo(UIPanel banner, UIText t, string s)
        {
            if (banner == null) yield break;
            t.text = s; banner.gameObject.SetActive(true);
            yield return Tw.Co(.3f, k => { if (banner != null) UI.Fade(banner, k); });
            yield return new WaitForSeconds(1.3f);
            yield return Tw.Co(.4f, k => { if (banner != null) UI.Fade(banner, 1 - k); });
            if (banner != null) banner.gameObject.SetActive(false);
        }
        void FloatText(Transform parent, Vector3 localPos, string s, Color c)
        {
            if (parent == null) return;
            var ft = UI.Canvas(parent, localPos + new Vector3(0, .2f, 0), 1.8f, .26f, Vector3.zero, "Texto");
            ft.localRotation = Quaternion.LookRotation(new Vector3(localPos.x, 0, localPos.z));
            UI.T(ft, s, UI.Wd(ft) / 2 + 3, 72, 50, new Color(0, 0, 0, .8f), true, UI.Al.C, UI.Wd(ft));
            UI.T(ft, s, UI.Wd(ft) / 2, 69, 50, c, true, UI.Al.C, UI.Wd(ft));
            Vector3 p0 = ft.localPosition;
            Runner.I.StartCoroutine(Tw.Co(1.3f, k => { if (ft == null) return; ft.localPosition = p0 + new Vector3(0, k * .45f, 0); UI.Fade(ft, k < .6f ? 1 : 1 - (k - .6f) / .4f); }, Ease.Out));
            Destroy(ft.gameObject, 1.4f);
        }
        void Burst(Vector3 worldPos, Color c)
        {
            var ps = Build.Particles(null, worldPos, c, .06f, 40);
            var main = ps.main; main.startLifetime = .8f; main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.6f); main.gravityModifier = .6f; main.simulationSpace = ParticleSystemSimulationSpace.World; main.loop = false;
            var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = .05f;
            ps.Play(); ps.Emit(24); Destroy(ps.gameObject, 1.2f);
        }
        void ShowNutriResults()
        {
            var N = nutri; NewScreen();
            N.score = Mathf.RoundToInt((ZoneScore(N.E) + ZoneScore(N.H) + ZoneScore(N.R)) / 3); N.stars = N.score >= 85 ? 3 : N.score >= 65 ? 2 : 1;
            Hint("Recuperación nutricional completada");
            var p = Panel(new Vector3(0, 1.86f, 2.75f), 2.5f, 1.66f, Pal.Yellow); float W = UI.Wd(p);
            UI.T(p, "ETAPA 2 DE 3 · COMPLETADA", 52, 62, 19, Pal.Yellow, true); UI.Stepper(p, W - 410, 58, 370, 2, Pal.Yellow);
            UI.T(p, "Recuperación nutricional", 52, 138, 48, Color.white, true);
            for (int i = 0; i < 3; i++) UI.Icon(p, Spr.Star, 70 + i * 50, 186, 21, i < N.stars ? Pal.Yellow : new Color(1, 1, 1, .2f));
            UI.T(p, "Equilibrio " + N.score + "%", 230, 197, 28, Pal.Yellow, true);
            UI.T(p, N.score >= 85 ? "¡Excelente balance!" : N.score >= 65 ? "Buen trabajo, se puede afinar." : "Te faltó equilibrio: probá de nuevo.", 470, 196, 21, Pal.Muted, true);
            float[] v = { N.E, N.H, N.R };
            for (int i = 0; i < 3; i++)
            {
                float y = 232 + i * 56, xb = 330, w = W - xb - 190;
                UI.Icon(p, BarIcon(i), 70, y + 18, 15, BarCol[i]); UI.T(p, BarName[i], 100, y + 27, 23, Color.white, true);
                UI.Round(p, xb, y + 4, w, 28, new Color(1, 1, 1, .08f), 10); UI.Img(p, xb + w * .6f, y + 4, w * .3f, 28, null, new Color(.24f, .86f, .59f, .2f));
                UI.Round(p, xb, y + 4, Mathf.Max(10, w * v[i] / 100), 28, BarCol[i], 10);
                string st = v[i] < ZMIN ? "Bajo" : v[i] > ZMAX ? "Exceso" : "Óptimo";
                UI.T(p, Mathf.RoundToInt(v[i]) + "% · " + st, xb + w + 16, y + 27, 20, v[i] < ZMIN ? Pal.Orange : v[i] > ZMAX ? Pal.Yellow : Pal.Green, true, UI.Al.L, 200);
            }
            var stats = new[] { new[] { N.good.ToString(), "elecciones prioritarias" }, new[] { N.bad.ToString(), "no prioritarias" }, new[] { N.missed.ToString(), "oportunidades perdidas" }, new[] { Mathf.RoundToInt(N.balance) + " s", "en equilibrio total" } };
            float cw = (W - 140) / 4;
            for (int i = 0; i < 4; i++) { float x = 52 + i * (cw + 12), y = 408; UI.Round(p, x, y, cw, 84, new Color(1, 1, 1, .05f), 16); UI.T(p, stats[i][0], x + 18, y + 44, 32, Color.white, true); UI.T(p, stats[i][1], x + 18, y + 70, 16, Pal.Muted, true); }
            UI.Wrap(p, "Trabajo invisible: en las 2 horas post-partido combiná carbohidratos + proteínas y tomá líquido de a sorbos hasta reponer lo perdido (≈1,5 L por cada kg de peso perdido).", 52, 540, W - 104, 29, 20, Pal.Body, false, true);
            var b1 = Btn("REINTENTAR", new Vector3(-.62f, .9f, 2.72f), .95f, .22f, UI.Style.Secondary, () => StartCoroutine(NutritionGame()));
            var b2 = Btn("CONTINUAR · ETAPA 3  →", new Vector3(.52f, .9f, 2.72f), 1.2f, .22f, UI.Style.Primary, () => StartCoroutine(StartCalm()));
            UI.Appear(p); UI.Appear(b1, .25f); UI.Appear(b2, .32f);
        }

        /* ===================================================================
           5. ETAPA 3 · VUELTA A LA CALMA
           =================================================================== */
        IEnumerator StartCalm()
        {
            au.BeatStop(1);
            yield return Go(Scenes.Calma, () => { env.Set(EnvName.Stadium, StadiumMode.Calm); NewStage(); au.PadStart(.55f); ShowCalmIntro(); },
                "ETAPA 3 DE 3", "Vuelta a la calma", "El estadio se vació. El ruido se apagó.", Pal.Purple, 2.4f, 1.6f);
        }
        void ShowCalmIntro()
        {
            NewScreen(); Hint("Etapa 3 · Vuelta a la calma");
            var p = Panel(new Vector3(0, 1.8f, 2.75f), 2.4f, 1.5f, Pal.Purple); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "ETAPA 3 DE 3", 52, 62, 19, Pal.Purple, true); UI.Stepper(p, W - 410, 58, 370, 2, Pal.Purple);
            UI.T(p, "Vuelta a la calma", 52, 146, 54, Color.white, true);
            UI.Wrap(p, "El movimiento baja, la música baja. Volviste a la cancha, pero ahora está en silencio. Tu sistema nervioso también necesita recuperarse.", 52, 196, W - 104, 33, 23, Pal.Body);
            UI.T(p, "Seguí el camino luminoso con tu puntero:", 52, 300, 24, Color.white, true);
            UI.T(p, "INHALÁ", 52, 350, 28, Pal.Teal, true); UI.T(p, "el camino sube: llevá el punto hacia arriba (4 s)", 180, 350, 22, Pal.Text, true);
            UI.T(p, "EXHALÁ", 52, 390, 28, Pal.Purple, true); UI.T(p, "el camino baja: llevá el punto hacia abajo (6 s)", 180, 390, 22, Pal.Text, true);
            UI.Wrap(p, "PC: mové el mouse (sin hacer clic). Celular: deslizá el dedo o mové el teléfono. VR: apuntá con el control o con la mirada. Si te salís del camino, tu respiración se agita y la vista tiembla.", 52, 448, W - 104, 28, 19, Pal.Muted);
            UI.T(p, "6 ciclos · 1 minuto", 52, H - 34, 19, Pal.Purple, true);
            var b = Btn("COMENZAR RESPIRACIÓN", new Vector3(0, .92f, 2.72f), 1.2f, .22f, UI.Style.Primary, () => StartCoroutine(Breathing()));
            UI.Appear(p); UI.Appear(b, .3f);
        }
        // Curva de respiración: sube al inhalar (4 s) y baja al exhalar (6 s)
        const float IN = 4, OUT = 6;
        static float BreathCurve(float t)
        {
            if (t < 0) return 0;
            float ph = t % (IN + OUT);
            return ph < IN ? Ease.Sine(ph / IN) : 1 - Ease.Sine((ph - IN) / OUT);
        }

        IEnumerator Breathing()
        {
            var scr = NewScreen();
            Hint("Seguí el camino con el puntero: sube al INHALAR y baja al EXHALAR · Si te salís, te agitás");
            const int CYCLES = 6; float TOTAL = (IN + OUT) * CYCLES;
            // ---- Pista del camino ----
            const float TW = 2.4f, TH = 1.0f, PAST = 1.5f, FUTURE = 4.5f, YR = .36f, TOL = .085f;
            var track = UI.Canvas(scr, new Vector3(0, 1.5f, 2.6f), TW, TH, Vector3.zero, "Camino de respiración");
            float PW = UI.Wd(track), PH = UI.Ht(track);
            UI.Framed(track, 0, 0, PW, PH, new Color(.02f, .04f, .1f, .82f), Pal.A(Pal.Purple, .55f), 30, 3);
            for (int g = 1; g < 6; g++) UI.Img(track, 20, PH * g / 6, PW - 40, 1.5f, null, new Color(1, 1, 1, .05f));
            UI.T(track, "INHALÁ ↑", 24, 40, 18, Pal.A(Pal.Teal, .8f), true);
            UI.T(track, "EXHALÁ ↓", 24, PH - 18, 18, Pal.A(Pal.Purple, .8f), true);
            float xPlay = -TW / 2 + PAST / (PAST + FUTURE) * TW;                       // posición del "ahora"
            UI.Img(track, (xPlay + TW / 2) / UI.S - 1, 12, 2, PH - 24, null, new Color(1, 1, 1, .22f));
            // Línea del camino (pasado tenue, futuro brillante)
            var lineGo = new GameObject("Camino"); lineGo.transform.SetParent(track.transform, false); lineGo.transform.localScale = Vector3.one / UI.S;
            var line = lineGo.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.positionCount = 90; line.widthMultiplier = .022f;
            line.sharedMaterial = Mat.Unlit(Color.white, null, 3000); line.sortingOrder = 2000; line.numCapVertices = 4;
            var grad = new Gradient(); grad.SetKeys(new[] { new GradientColorKey(Pal.Purple, 0), new GradientColorKey(Pal.Teal, PAST / (PAST + FUTURE)), new GradientColorKey(Pal.Teal, 1) },
                new[] { new GradientAlphaKey(.25f, 0), new GradientAlphaKey(.9f, PAST / (PAST + FUTURE)), new GradientAlphaKey(.55f, 1) });
            line.colorGradient = grad;
            // Objetivo (anillo) y punto del usuario
            var target = Build.MeshObj("Objetivo", lineGo.transform, Build.Wall(1, 1), Mat.Unlit(Pal.A(Pal.Teal, .9f), Spr.Ring.texture, 3001)); target.transform.localScale = Vector3.one * .11f;
            var tGlow = Build.Glow(lineGo.transform, Vector3.zero, .35f, Pal.A(Pal.Teal, .5f), false); tGlow.GetComponent<Renderer>().sharedMaterial.renderQueue = 3001;
            var me = Build.MeshObj("Tu respiración", lineGo.transform, Build.Wall(1, 1), Mat.Unlit(Color.white, Spr.Circle.texture, 3002)); me.transform.localScale = Vector3.one * .06f;
            var meMat = me.GetComponent<Renderer>().sharedMaterial;
            foreach (var r in new[] { target.GetComponent<Renderer>(), tGlow.GetComponent<Renderer>(), me.GetComponent<Renderer>() }) r.sortingOrder = 2001;
            var pts = new Vector3[90];

            var lab = UI.Canvas(scr, new Vector3(0, 2.32f, 2.6f), 1.8f, .42f, Vector3.zero, "Fase");
            var tWord = UI.T(lab, "PREPARATE", UI.Wd(lab) / 2, 104, 92, Color.white, true, UI.Al.C, UI.Wd(lab));
            var tSec = UI.T(lab, "Mové el puntero hasta el anillo", UI.Wd(lab) / 2, 158, 26, Pal.Muted, true, UI.Al.C, UI.Wd(lab));
            var info = UI.Canvas(scr, new Vector3(0, .78f, 2.6f), 2.4f, .34f, new Vector3(12, 0, 0), "Info"); float IW = UI.Wd(info), IH = UI.Ht(info);
            UI.Framed(info, 6, 6, IW - 12, IH - 12, new Color(.02f, .04f, .09f, .75f), Pal.A(Pal.Purple, .5f), (IH - 12) / 2, 2);
            var cdots = new UIImage[CYCLES]; for (int i = 0; i < CYCLES; i++) cdots[i] = UI.Icon(info, Spr.Circle, 70 + i * 34, IH / 2, 10, new Color(1, 1, 1, .15f));
            var tCycle = UI.T(info, "Ciclo 1 de 6", 70 + CYCLES * 34 + 10, IH / 2 + 9, 25, Color.white, true);
            var tSync = UI.T(info, "En el camino —", IW * .6f, IH / 2 + 9, 25, Pal.Teal, true, UI.Al.C, 360);
            UI.Icon(info, Spr.Heart, IW - 190, IH / 2, 16, Pal.Red); var tHr = UI.T(info, "104 lpm", IW - 162, IH / 2 + 9, 25, Color.white, true);
            UI.Appear(track); UI.Appear(lab, .1f); UI.Appear(info, .2f);

            float yUser = -YR, T = -3f, hr = 104, shake = 0, infoT = 0, agit = 0; int samples = 0, onPath = 0; string phase = ""; bool failed = false;
            Func<float, float> Y = tt => -YR + BreathCurve(tt) * 2 * YR;
            Action draw = () =>
            {
                for (int k = 0; k < pts.Length; k++)
                {
                    float u = (float)k / (pts.Length - 1), tt = T - PAST + u * (PAST + FUTURE);
                    pts[k] = new Vector3(-TW / 2 + u * TW, Y(tt), -.01f);
                }
                line.SetPositions(pts);
                float yt = Y(T); target.transform.localPosition = new Vector3(xPlay, yt, -.015f); tGlow.transform.localPosition = new Vector3(xPlay, yt, -.012f);
                me.transform.localPosition = new Vector3(xPlay, yUser, -.02f);
            };
            Action readPointer = () =>
            {
                var ray = rig.PointerRay; var plane = new Plane(-lineGo.transform.forward, lineGo.transform.position); float d;
                if (plane.Raycast(ray, out d)) { var lp = lineGo.transform.InverseTransformPoint(ray.GetPoint(d)); yUser = Mathf.Lerp(yUser, Mathf.Clamp(lp.y, -YR - .1f, YR + .1f), 1 - Mathf.Exp(-Time.deltaTime * 18)); }
            };
            au.PadLevel(.32f, 60);
            // Cuenta previa de 3 s para ubicar el puntero
            while (T < 0)
            {
                T += Time.deltaTime; readPointer(); draw();
                tSec.text = "Ubicá tu punto en el anillo · empieza en " + Mathf.CeilToInt(-T);
                if (scr == null) yield break; yield return null;
            }
            while (T < TOTAL)
            {
                T += Time.deltaTime; float t = Mathf.Min(T, TOTAL);
                int cyc = Mathf.Min(CYCLES - 1, Mathf.FloorToInt(t / (IN + OUT))); float ph = t - cyc * (IN + OUT); bool inhale = ph < IN;
                string key = cyc + (inhale ? "i" : "e");
                if (key != phase) { phase = key; au.Breath(inhale); rig.Haptic(); }
                readPointer(); draw();
                float err = Mathf.Abs(yUser - Y(T));
                samples++; if (err < TOL) onPath++;
                // Fuera del camino: la cámara tiembla como si te agitaras
                float want = Mathf.Clamp01((err - TOL) / .22f);
                shake = Mathf.Lerp(shake, want, 1 - Mathf.Exp(-Time.deltaTime * (want > shake ? 6 : 2.5f)));
                rig.Shake = shake;
                meMat.color = Color.Lerp(Color.white, Pal.Orange, shake);
                // Agitación acumulada: si llega al 100 % la misión falla
                agit = Mathf.Clamp01(agit + Time.deltaTime * (shake > .4f ? shake * .12f : -.04f));
                if (agit >= 1) { failed = true; break; }
                hr = Mathf.Lerp(104, 64, Ease.Out(t / TOTAL)) + shake * 18 + Mathf.Sin(Time.time * 1.3f) * 1.2f;
                infoT -= Time.deltaTime;
                if (infoT <= 0)
                {
                    infoT = .12f; int secs = Mathf.CeilToInt(inhale ? IN - ph : IN + OUT - ph);
                    tWord.text = inhale ? "INHALÁ ↑" : "EXHALÁ ↓"; tWord.color = inhale ? Pal.Teal : Pal.Purple;
                    tSec.text = shake > .35f ? "Volvé al camino… respirá tranquilo" : secs.ToString();
                    tSec.color = shake > .35f ? Pal.Orange : Pal.Muted;
                    for (int i = 0; i < CYCLES; i++) cdots[i].color = i < cyc ? Pal.Purple : i == cyc ? Pal.A(Pal.Purple, .45f) : new Color(1, 1, 1, .15f);
                    tCycle.text = "Ciclo " + (cyc + 1) + " de " + CYCLES;
                    tSync.text = "Agitación " + Mathf.RoundToInt(agit * 100) + "%"; tSync.color = Color.Lerp(Pal.Teal, Pal.Red, agit);
                    tHr.text = Mathf.RoundToInt(hr) + " lpm";
                }
                env.ApplyLight(LightP.Lerp(PikiEnv.LCalm, PikiEnv.LCalmDeep, t / TOTAL), false);
                if (scr == null) { rig.Shake = 0; yield break; }
                yield return null;
            }
            rig.Shake = 0;
            if (failed)
            {
                // ---- Misión fallida ----
                au.Wrong(); tWord.text = "MISIÓN FALLIDA"; tWord.color = Pal.Red; tSec.text = "Te agitaste demasiado: tu cuerpo no logró volver a la calma"; tSec.color = Pal.Hex("#ffb3bb");
                tSync.text = "Agitación 100%"; tSync.color = Pal.Red;
                bool decided = false, retry = false;
                var bR = Btn("REINTENTAR", new Vector3(-.62f, .42f, 2.55f), 1.05f, .22f, UI.Style.Primary, () => { decided = true; retry = true; });
                var bC = Btn("CONTINUAR IGUAL", new Vector3(.62f, .42f, 2.55f), 1.05f, .22f, UI.Style.Secondary, () => { decided = true; });
                UI.Appear(bR, .3f); UI.Appear(bC, .4f);
                while (!decided) { if (scr == null) yield break; yield return null; }
                if (retry) { StartCoroutine(Breathing()); yield break; }
                calm = new CalmRes { cycles = CYCLES, guided = false, failed = true, sync = Mathf.RoundToInt(onPath * 100f / Mathf.Max(1, samples)), hr = Mathf.RoundToInt(hr) };
                UI.Vanish(bR, .3f); UI.Vanish(bC, .3f);
            }
            else
            {
                calm = new CalmRes { cycles = CYCLES, guided = false, sync = Mathf.RoundToInt(onPath * 100f / Mathf.Max(1, samples)), hr = Mathf.RoundToInt(hr) };
                for (int i = 0; i < CYCLES; i++) cdots[i].color = Pal.Purple;
                tSync.text = "En el camino " + calm.sync + "%"; tSync.color = Pal.Teal;
                tWord.text = "TERMINADO"; tWord.color = Color.white; tSec.text = "Tu respiración y tu pulso volvieron a la calma"; tSec.color = Pal.Muted;
            }
            yield return new WaitForSeconds(1.6f);
            UI.Vanish(track, .8f); UI.Vanish(lab, .8f); UI.Vanish(info, .8f);
            yield return new WaitForSeconds(1f);
            // El entorno vuelve de a poco a la cancha, ahora completamente tranquila
            Hint("");
            var skyFrom = env.SkyNow; var lightFrom = env.LightNow;
            yield return Tw.Co(5.2f, k => { env.BlendSky(skyFrom, PikiEnv.SkyDawn, k); env.ApplyLight(LightP.Lerp(lightFrom, PikiEnv.LDawn, k), false); });
            env.ApplyLight(PikiEnv.LDawn); env.Scoreboard(StadiumMode.Dawn, true);
            ShowFinal();
        }

        /* ===================================================================
           6. FINAL
           =================================================================== */
        void ShowFinal()
        {
            var scr = NewScreen();
            au.Final(); au.PadLevel(.4f, 3);
            Hint("Recuperación completada · Podés volver a jugar con nuevas situaciones");
            var S = sits ?? new List<Situation>(); var N = nutri ?? new Nutri { score = 0, stars = 1 }; var K = calm ?? new CalmRes();
            var treats = string.Join(", ", S.Select(x => Content.TLabel(x.correct).ToLower()).Distinct().ToArray());
            var rows = new[]
            {
                new { n = "Recuperación física", d = S.Count + " situaciones resueltas · " + firstTry + "/3 a la primera · " + treats, c = Pal.Red },
                new { n = "Recuperación nutricional", d = "Equilibrio " + N.score + "% · " + N.stars + "/3 estrellas · " + N.good + " elecciones prioritarias", c = Pal.Yellow },
                new { n = "Vuelta a la calma", d = K.cycles + " ciclos de respiración · " + K.sync + "% en el camino" + " · FC " + K.hr + " lpm", c = Pal.Purple },
            };
            var p = Panel(new Vector3(0, 1.9f, 2.85f), 2.7f, 1.78f, Pal.Green); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "PIKI RECOVERY · FÚTBOL", 56, 64, 19, Pal.Green, true); UI.Stepper(p, W - 420, 60, 380, 3, Pal.Green);
            UI.Icon(p, Spr.Circle, 100, 158, 42, Pal.Green); UI.Icon(p, Spr.Check, 100, 158, 24, Pal.Ink);
            UI.T(p, K.failed ? "RECUPERACIÓN INCOMPLETA" : "RECUPERACIÓN COMPLETADA", 164, 180, 56, Color.white, true);
            UI.T(p, "Terminaste el trabajo invisible. Tu cuerpo está listo para volver a entrenar.", 58, 244, 23, Pal.Teal, true);
            for (int i = 0; i < 3; i++)
            {
                float y = 276 + i * 100; var r = rows[i];
                UI.Framed(p, 56, y, W - 112, 86, new Color(.08f, .14f, .22f, 1), Pal.A(r.c, .4f), 18, 2);
                bool fail = i == 2 && K.failed; Color st = fail ? Pal.Red : Pal.Ok;
                UI.Icon(p, Spr.Glow, 100, y + 43, 30, Pal.A(st, .7f)); UI.Icon(p, Spr.Circle, 100, y + 43, 17, st);
                UI.T(p, r.n, 136, y + 38, 28, Color.white, true);
                var dt = UI.T(p, r.d, 136, y + 68, 18, Pal.Muted, true, UI.Al.L, W - 460); dt.FitWidth(W - 460);
                UI.T(p, fail ? "Misión fallida" : "Completada", W - 84, y + 52, 26, st, true, UI.Al.R, 300);
            }
            UI.T(p, "“Nadie lo ve. Pero también es entrenamiento.”", W / 2, H - 42, 27, Pal.Orange, false, UI.Al.C, W, true);
            var b1 = Btn("VER TU PRÓXIMO PARTIDO  →", new Vector3(0, .88f, 2.8f), 1.5f, .24f, UI.Style.Primary, () => StartCoroutine(GoNextMatch()));
            UI.Appear(p, 0, .9f); UI.Appear(b1, .5f);
            // Partículas doradas suaves
            var ps = Build.Particles(scr, new Vector3(0, 0, 0), new Color(1, .89f, .64f, .8f), .06f, 300);
            var main = ps.main; main.startLifetime = 14; main.startSpeed = new ParticleSystem.MinMaxCurve(.05f, .2f); main.startSize = new ParticleSystem.MinMaxCurve(.03f, .08f);
            var em = ps.emission; em.enabled = true; em.rateOverTime = 12;
            var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(16, .5f, 12); sh.position = new Vector3(0, 0, 4); sh.rotation = new Vector3(-90, 0, 0);
            ps.Play(); ps.Emit(120);
        }
        /* ===================================================================
           7. EL PRÓXIMO PARTIDO · rendimiento según tu recuperación
           =================================================================== */
        void EnsureSampleResults()
        {
            if (sits == null) { sits = Content.Generate(match); foreach (var x in sits) { x.attempts = 1; x.firstTry = true; } sits[2].attempts = 2; sits[2].firstTry = false; firstTry = 2; }
            if (nutri == null) nutri = new Nutri { score = 78, stars = 2, good = 18 };
            if (calm == null) calm = new CalmRes { guided = false, sync = 80 };
        }
        // Puntaje de cada etapa (0-100) y total ponderado
        float PhysScore() { if (sits == null || sits.Count == 0) return 50; float t = 0; foreach (var x in sits) t += x.attempts <= 1 ? 100 : x.attempts == 2 ? 60 : 30; return t / sits.Count; }
        float NutriScore() { return nutri != null ? nutri.score : 50; }
        float CalmScore() { if (calm == null) return 50; return calm.failed ? calm.sync * .35f : Mathf.Clamp(calm.sync * 1.05f, 0, 100); }
        int Performance() { return Mathf.RoundToInt(Mathf.Clamp(PhysScore() * .35f + NutriScore() * .35f + CalmScore() * .3f, 0, 100)); }
        static int SubMinute(int perf) { return perf >= 75 ? 90 : perf >= 55 ? 75 : perf >= 35 ? 60 : perf >= 20 ? 45 : 30; }
        static string Outcome(int perf)
        {
            if (perf >= 90) return "Jugaste los 90 minutos rindiendo al 100%";
            if (perf >= 75) return "Jugaste los 90 minutos a buen nivel";
            if (perf >= 55) return "Te cambiaron al minuto 75: se notó el cansancio";
            if (perf >= 35) return "Saliste al minuto 60 con molestias musculares";
            if (perf >= 20) return "El DT te sacó en el entretiempo";
            return "El DT te cambió al minuto 30 por bajo rendimiento";
        }

        IEnumerator GoNextMatch()
        {
            au.PadStop(1.5f);
            yield return Go(Scenes.Partido, () => { env.Set(EnvName.Stadium, StadiumMode.Match); NewStage(); au.CrowdStart(.5f); StartCoroutine(NextMatch()); },
                "UNA SEMANA DESPUÉS", "Tu próximo partido", "Así se nota en la cancha todo el trabajo invisible", Pal.Teal, 2f);
        }

        IEnumerator NextMatch()
        {
            var scr = NewScreen();
            int perf = Performance(), sub = SubMinute(perf); int home = perf >= 75 ? 2 : perf >= 50 ? 1 : 0, away = perf >= 55 ? 0 : 1;
            Hint("Mirá a tu alrededor: estás jugando el próximo partido");
            // Jugadores en juego y "vos" con la pelota
            var walkers = env.StartNextMatch();
            var players = walkers.Count > 0 ? walkers[0].transform.parent : stage;
            var you = PlayerWalker.Make(players, Pal.Teal, Color.white, Pal.Teal, 9); you.name = "Vos (#10)";
            you.areaX = new Vector2(-14, 14); you.areaZ = new Vector2(2, 30); you.wander = true; you.ResetPos(); you.transform.localPosition = new Vector3(3, 0, 6);
            var ballT = Build.Prim(PrimitiveType.Sphere, you.transform, new Vector3(0, .11f, .45f), Vector3.one * .22f, Mat.Lit(Color.white, .5f)).transform;
            var tag = UI.Canvas(you.transform, new Vector3(0, 2.15f, 0), .9f, .2f, Vector3.zero, "Vos");
            UI.Framed(tag, 4, 4, UI.Wd(tag) - 8, UI.Ht(tag) - 8, new Color(.02f, .04f, .09f, .85f), Pal.Teal, 30, 3);
            UI.T(tag, "VOS · #10", UI.Wd(tag) / 2, 54, 40, Pal.Teal, true, UI.Al.C, UI.Wd(tag));
            tag.gameObject.AddComponent<Billboard>();
            au.Whistle(true);
            // Reloj del partido acelerado
            float minute = 0; int shownMin = -1; bool goalShown = false;
            while (minute < sub)
            {
                minute = Mathf.Min(sub, minute + Time.deltaTime * 90f / 14f);
                int m = Mathf.FloorToInt(minute);
                if (m != shownMin)
                {
                    shownMin = m; int h = home > 0 && m >= 30 ? (home > 1 && m >= 70 ? 2 : 1) : 0;
                    if (h > 0 && !goalShown && m >= 30) { goalShown = true; au.CrowdCheer(); }
                    env.NextMatchBoard(m, h, away > 0 && m >= 50 ? 1 : 0, "Piki FC con el #10 en cancha");
                }
                ballT.Rotate(360 * Time.deltaTime, 0, 0, Space.Self);
                if (scr == null) yield break;
                yield return null;
            }
            if (sub < 90)
            {
                au.Whistle(); env.NextMatchBoard(sub, home > 0 && sub >= 30 ? 1 : 0, away > 0 && sub >= 50 ? 1 : 0, "CAMBIO · sale el #10");
                Destroy(ballT.gameObject);
                you.GoTo(new Vector3(-38.5f, 0, -9), 1.2f, false);
                yield return new WaitForSeconds(3f);
            }
            else { au.Whistle(); env.NextMatchBoard(90, home, away, "FINAL · el #10 jugó los 90 minutos"); au.CrowdCheer(); yield return new WaitForSeconds(2f); }
            ShowNextMatchPanel(perf, true);
        }

        void ShowNextMatchPanel(int perf, bool animate)
        {
            NewScreen();
            Hint("Tu rendimiento en el próximo partido depende de cómo te recuperaste");
            Color pc = Color.HSVToRGB(Mathf.Lerp(0, 125, perf / 100f) / 360f, .75f, .95f);
            var p = Panel(new Vector3(0, 1.88f, 2.85f), 2.7f, 1.74f, pc); float W = UI.Wd(p), H = UI.Ht(p);
            UI.T(p, "TU PRÓXIMO PARTIDO · RENDIMIENTO", 56, 64, 19, pc, true);
            UI.Wrap(p, Outcome(perf), 56, 130, W - 112, 54, 46, Color.white, true);
            // Barra de rendimiento con gradiente rojo → verde
            float bx = 56, by = 236, bw = W - 112, bh = 34; int segs = 40;
            UI.Round(p, bx - 4, by - 4, bw + 8, bh + 8, new Color(1, 1, 1, .12f), (bh + 8) / 2);
            for (int k = 0; k < segs; k++)
            {
                float u = (float)k / segs; Color c = Color.HSVToRGB(Mathf.Lerp(0, 125, u) / 360f, .8f, .9f);
                UI.Img(p, bx + u * bw, by, bw / segs - 2, bh, null, Pal.A(c, .28f));
            }
            var fill = UI.Round(p, bx, by, bh, bh, pc, bh / 2);
            var marker = UI.Rect(p, bx, by - 30, 2, 2, "marcador");
            UI.Icon(marker, Spr.Circle, 1, 1, 14, Color.white);
            var pct = UI.T(p, "0%", bx, by + bh + 58, 46, pc, true, UI.Al.C, 200);
            UI.Wrap(p, "El DT te cambió por bajo rendimiento", bx, by + bh + 34, 300, 22, 18, Pal.Red, true);
            UI.Wrap(p, "Jugaste los 90 min rindiendo al 100%", bx + bw - 300, by + bh + 34, 300, 22, 18, Pal.Green, true, false, UI.Al.R);
            // Desglose por etapa
            var parts = new[] { new { n = "Recuperación física", v = PhysScore(), c = Pal.Red }, new { n = "Recuperación nutricional", v = NutriScore(), c = Pal.Yellow }, new { n = "Vuelta a la calma", v = CalmScore(), c = Pal.Purple } };
            float cw = (W - 112 - 32) / 3;
            for (int k = 0; k < 3; k++)
            {
                float x = 56 + k * (cw + 16), y = 372;
                UI.Framed(p, x, y, cw, 110, new Color(.08f, .14f, .22f, 1), Pal.A(parts[k].c, .5f), 18, 2);
                UI.T(p, parts[k].n, x + 20, y + 36, 19, parts[k].c, true);
                UI.T(p, Mathf.RoundToInt(parts[k].v) + "%", x + 20, y + 84, 40, Color.white, true);
                if (k == 2 && calm != null && calm.failed) UI.T(p, "misión fallida", x + cw - 20, y + 84, 18, Pal.Red, true, UI.Al.R, 200);
            }
            UI.T(p, "“Lo que no se ve en la recuperación, se ve en la cancha.”", W / 2, H - 40, 25, Pal.Orange, false, UI.Al.C, W, true);
            var b1 = Btn("JUGAR DE NUEVO", new Vector3(-.6f, .88f, 2.8f), 1.05f, .22f, UI.Style.Primary, () => StartCoroutine(StartMatch()));
            var b2 = Btn("VOLVER AL INICIO", new Vector3(.6f, .88f, 2.8f), 1.05f, .22f, UI.Style.Secondary, () => StartCoroutine(GoHome()));
            UI.Appear(p, 0, .7f); UI.Appear(b1, .5f); UI.Appear(b2, .6f);
            Action<float> setK = k =>
            {
                float v = perf / 100f * k; fill.Size = new Vector2(Mathf.Max(bh, bw * v), bh);
                marker.localPosition = p.Center(bx + bw * v - 1, by - 30, 2, 2); pct.fontSize = 46; pct.text = Mathf.RoundToInt(perf * k) + "%";
                pct.transform.localPosition = new Vector3(bx + bw * v - W / 2, pct.transform.localPosition.y, 0);
            };
            if (animate && Application.isPlaying) { setK(0); Tw.Go(2.2f, setK, Ease.Out); Tw.Later(2.2f, () => { if (perf >= 75) au.Final(); else au.Soft(); }); }
            else setK(1);
        }

        IEnumerator GoHome()
        {
            au.PadStop(1.5f); au.CrowdStop(1);
            yield return Go(Scenes.Inicio, () => { env.Set(EnvName.Hub); NewStage(); ShowWelcome(); }, null, null, null, null, 0, .8f);
        }
    }
}
