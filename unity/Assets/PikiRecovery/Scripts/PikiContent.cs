// =====================================================================
//  PIKI RECOVERY · Contenido: situaciones corporales, tratamientos,
//  alimentos y estadísticas del partido.
// =====================================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Piki
{
    public enum Kind { Dolor, Molestia, Fatiga, Rigidez }
    public enum Treat { Frio, Calor, Masaje }

    public class Zone
    {
        public string key, name, desc, art, golpe; public bool lado; public Vector3 pos; public Kind[] kinds;
    }

    public class Situation
    {
        public Zone zone; public string side; public Kind kind; public string intensity; public int level; public Treat correct;
        public string text; public string[] signals; public Vector3 pos; public int attempts; public bool firstTry, done;
        public List<Treat> wrongs = new List<Treat>();
    }

    public class MatchStats
    {
        public int minutes, sprints, fcmax, kcal, home = 2, away = 1; public float km, sweat;
        public static MatchStats New()
        {
            return new MatchStats { minutes = 90 + Random.Range(2, 6), km = Random.Range(9.6f, 11.9f), sprints = Random.Range(26, 42), fcmax = Random.Range(181, 196), sweat = Random.Range(1.3f, 2.3f), kcal = Random.Range(1250, 1651) };
        }
    }

    public class Food
    {
        public string name, tip; public bool good; public int E, H, R; public FoodShape shape; public Color c1, c2;
        public Food(string n, bool g, int e, int h, int r, string tip, FoodShape s, string c1, string c2) { name = n; good = g; E = e; H = h; R = r; this.tip = tip; shape = s; this.c1 = Pal.Hex(c1); this.c2 = Pal.Hex(c2); }
    }
    public enum FoodShape { Bottle, Box, Sphere, Banana, Bowl, Bread, Drumstick, Egg, Fish, Glass, Nuts, Can, Burger, Fries, Donut, Candy, Hotdog, Slice }

    public static class Content
    {
        public static string F1(float v) { return v.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ','); }
        public static string Thousands(int v) { return v.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.'); }
        public static string Cap(string s) { return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1); }

        // Cuerpo mirando al usuario (su frente apunta a -Z). Derecha del atleta = -X.
        public static readonly Zone[] Zones =
        {
            new Zone { key = "isquio", name = "Isquiotibiales", desc = "Parte posterior del muslo", lado = true, pos = new Vector3(.1f, .74f, .1f), art = "los isquiotibiales", kinds = new[] { Kind.Molestia, Kind.Fatiga, Kind.Rigidez } },
            new Zone { key = "cuadri", name = "Cuádriceps", desc = "Parte anterior del muslo", lado = true, pos = new Vector3(.1f, .76f, -.1f), art = "el cuádriceps", kinds = new[] { Kind.Dolor, Kind.Fatiga, Kind.Molestia }, golpe = "un rodillazo en el muslo en una disputa (un \"tortazo\")" },
            new Zone { key = "gemelos", name = "Gemelos", desc = "Pantorrilla", lado = true, pos = new Vector3(.1f, .36f, .09f), art = "los gemelos", kinds = new[] { Kind.Fatiga, Kind.Rigidez, Kind.Molestia, Kind.Dolor }, golpe = "una patada en la pantorrilla en una barrida" },
            new Zone { key = "rodilla", name = "Rodilla", desc = "Articulación de la rodilla", lado = true, pos = new Vector3(.1f, .54f, -.09f), art = "la rodilla", kinds = new[] { Kind.Dolor, Kind.Molestia }, golpe = "un golpe en la rodilla al chocar con un rival" },
            new Zone { key = "tobillo", name = "Tobillo", desc = "Articulación del tobillo", lado = true, pos = new Vector3(.1f, .12f, -.05f), art = "el tobillo", kinds = new[] { Kind.Dolor, Kind.Molestia }, golpe = "una patada en el tobillo en una entrada fuerte" },
            new Zone { key = "aductores", name = "Aductores", desc = "Zona interna del muslo · ingle", lado = true, pos = new Vector3(.05f, .86f, -.06f), art = "los aductores", kinds = new[] { Kind.Molestia, Kind.Fatiga, Kind.Rigidez } },
            new Zone { key = "lumbar", name = "Zona lumbar", desc = "Espalda baja", lado = false, pos = new Vector3(0, 1.07f, .15f), art = "la zona lumbar", kinds = new[] { Kind.Rigidez, Kind.Fatiga, Kind.Molestia } },
            new Zone { key = "trapecio", name = "Trapecio", desc = "Cuello y parte alta de la espalda", lado = true, pos = new Vector3(.1f, 1.47f, .08f), art = "el trapecio", kinds = new[] { Kind.Rigidez, Kind.Dolor }, golpe = "un choque de hombros en un cabezazo" },
        };

        public static string KindLabel(Kind k) { return k == Kind.Dolor ? "DOLOR" : k == Kind.Molestia ? "MOLESTIA" : k == Kind.Fatiga ? "FATIGA" : "RIGIDEZ"; }
        public static Color KindColor(Kind k) { return k == Kind.Dolor ? Pal.Red : k == Kind.Molestia ? Pal.Orange : k == Kind.Fatiga ? Pal.Yellow : Pal.Purple; }

        public static string TLabel(Treat t) { return t == Treat.Frio ? "FRÍO" : t == Treat.Calor ? "CALOR" : "MASAJES"; }
        public static Color TColor(Treat t) { return t == Treat.Frio ? Pal.Blue : t == Treat.Calor ? Pal.Orange : Pal.Green; }
        public static Sprite TIcon(Treat t) { return t == Treat.Frio ? Spr.Snow : t == Treat.Calor ? Spr.Flame : Spr.Hands; }
        public static int TMins(Treat t) { return t == Treat.Frio ? 15 : t == Treat.Calor ? 20 : 10; }
        public static string TVerb(Treat t) { return t == Treat.Frio ? "Aplicando frío" : t == Treat.Calor ? "Aplicando calor" : "Masaje de descarga"; }
        public static string THow(Treat t) { return t == Treat.Frio ? "Hielo envuelto en un paño · 15 minutos" : t == Treat.Calor ? "Compresa tibia · 20 minutos" : "Masaje suave de descarga · 10 minutos"; }
        public static string TExpl(Treat t)
        {
            return t == Treat.Frio ? "El frío reduce la inflamación y calma el dolor de un golpe o de una lesión reciente. Siempre con un paño entre el hielo y la piel."
                : t == Treat.Calor ? "El calor aumenta la circulación y relaja el tejido rígido. Se usa solo cuando no hay golpe ni inflamación."
                : "El masaje de descarga relaja el músculo cargado, alivia la sensación de fatiga y devuelve movilidad.";
        }
        public static string Hint(Treat correct, Treat chosen)
        {
            if (correct == Treat.Frio) return chosen == Treat.Calor ? "El calor aumenta la inflamación de una zona recién golpeada o hinchada." : "Masajear una zona golpeada o inflamada puede empeorar el daño.";
            if (correct == Treat.Calor) return chosen == Treat.Frio ? "El frío contrae todavía más el músculo: una zona rígida necesita relajarse y recibir circulación." : "Masajear un tejido rígido y frío es incómodo y poco efectivo: primero necesita soltarse.";
            return chosen == Treat.Frio ? "No hubo golpe ni hay inflamación: el frío no es prioritario. El músculo cargado necesita descarga." : "El calor no es lo más efectivo acá: la carga muscular se alivia mejor con una descarga manual.";
        }

        static bool Plural(Zone z) { return z.art.StartsWith("los "); }
        static bool Fem(Zone z) { return z.art.StartsWith("la "); }
        static string Agr(Zone z, string b) { return Plural(z) ? b + "s" : Fem(z) ? b.Substring(0, b.Length - 1) + "a" : b; }
        static string Vb(Zone z, string s, string p) { return Plural(z) ? p : s; }
        static T Pick<T>(T[] a) { return a[Random.Range(0, a.Length)]; }

        public static Situation Make(Zone z, Kind kind, MatchStats m)
        {
            var s = new Situation { zone = z, kind = kind };
            s.side = z.lado ? Pick(new[] { "derecha", "izquierda" }) : null;
            s.intensity = kind == Kind.Dolor ? Pick(new[] { "moderada", "alta" }) : kind == Kind.Rigidez ? Pick(new[] { "leve", "moderada" }) : Pick(new[] { "leve", "moderada", "alta" });
            s.level = s.intensity == "leve" ? Random.Range(2, 4) : s.intensity == "moderada" ? Random.Range(4, 7) : Random.Range(7, 9);
            int min = Random.Range(52, 89); string lado = s.side != null ? (z.key == "trapecio" ? " del lado " + s.side : " de la pierna " + s.side) : "";
            switch (kind)
            {
                case Kind.Dolor:
                    s.correct = Treat.Frio; s.text = "Minuto " + min + ": recibiste " + z.golpe + ". Ahora la zona está hinchada, caliente y duele al tocarla.";
                    s.signals = new[] { "Golpe reciente", "Hinchazón", "Calor en la zona" }; break;
                case Kind.Molestia:
                    if (s.intensity == "alta") { s.correct = Treat.Frio; s.text = "En un sprint del minuto " + min + " sentiste un pinchazo en " + z.art + lado + ". Se nota una leve hinchazón y molesta al apoyar."; s.signals = new[] { "Pinchazo", "Hinchazón leve", "Aparición reciente" }; }
                    else { s.correct = Treat.Masaje; s.text = "Los cambios de ritmo y los arranques cargaron " + z.art + ". Sentís tensión al estirar, pero no hubo golpe ni hay hinchazón."; s.signals = new[] { "Sobrecarga", "Sin golpe", "Sin hinchazón" }; }
                    break;
                case Kind.Fatiga:
                    s.correct = Treat.Masaje; s.text = "Después de " + F1(m.km) + " km y " + m.sprints + " sprints, " + z.art + " " + Vb(z, "se siente", "se sienten") + " " + Agr(z, "pesado") + " y sin fuerza. No hay un dolor puntual.";
                    s.signals = new[] { "Cansancio muscular", "Pesadez", "Sin dolor puntual" }; break;
                default:
                    s.correct = Treat.Calor; s.text = Cap(z.art) + " " + Vb(z, "está", "están") + " " + Agr(z, "duro") + " y cuesta moverl" + (Plural(z) ? "os" : Fem(z) ? "a" : "o") + ". " + Vb(z, "Se endureció", "Se endurecieron") + " al enfriarte después del partido. No hubo golpe ni hinchazón.";
                    s.signals = new[] { "Poca movilidad", "Tensión", "Sin inflamación" }; break;
            }
            s.pos = z.pos; if (s.side == "derecha") s.pos.x = -s.pos.x;
            return s;
        }
        public static List<Situation> Generate(MatchStats m)
        {
            for (int tries = 0; tries < 80; tries++)
            {
                var zones = Zones.OrderBy(x => Random.value).Take(3).ToList();
                var sits = zones.Select(z => Make(z, Pick(z.kinds), m)).ToList();
                if (sits.Select(x => x.correct).Distinct().Count() >= 2 && sits.Select(x => x.kind).Distinct().Count() >= 2) return sits;
            }
            return new List<Situation> { Make(Zones[1], Kind.Dolor, m), Make(Zones[0], Kind.Fatiga, m), Make(Zones[6], Kind.Rigidez, m) };
        }

        public static readonly Food[] Foods =
        {
            new Food("Agua", true, 0, 16, 0, "Hidratación pura", FoodShape.Bottle, "#4fc3ff", "#e8f7ff"),
            new Food("Bebida isotónica", true, 7, 13, 0, "Líquido + sales + azúcares", FoodShape.Bottle, "#19e3b1", "#ffffff"),
            new Food("Sandía", true, 5, 11, 0, "Fruta muy hidratante", FoodShape.Slice, "#ff4d5e", "#3ddc97"),
            new Food("Naranja", true, 7, 8, 0, "Agua, azúcares y vitamina C", FoodShape.Sphere, "#ff8a3d", "#3ddc97"),
            new Food("Banana", true, 13, 2, 1, "Carbohidratos rápidos y potasio", FoodShape.Banana, "#ffd93d", "#7a5a1a"),
            new Food("Pasta", true, 17, 0, 3, "Recarga el glucógeno", FoodShape.Bowl, "#f4c95d", "#ffffff"),
            new Food("Arroz", true, 16, 0, 2, "Carbohidrato de fácil digestión", FoodShape.Bowl, "#f7f4ea", "#ffffff"),
            new Food("Pan integral", true, 12, 0, 3, "Energía de absorción gradual", FoodShape.Bread, "#b5773b", "#d9a066"),
            new Food("Papa hervida", true, 13, 1, 1, "Carbohidrato y potasio", FoodShape.Egg, "#d9b36c", "#c49a52"),
            new Food("Pollo", true, 0, 0, 17, "Proteína para reparar el músculo", FoodShape.Drumstick, "#d9924a", "#fff4e0"),
            new Food("Huevo", true, 1, 0, 14, "Proteína de alta calidad", FoodShape.Egg, "#fff8ec", "#ffd93d"),
            new Food("Pescado", true, 0, 1, 15, "Proteína y grasas buenas", FoodShape.Fish, "#8fb8d8", "#5f86a8"),
            new Food("Leche chocolatada", true, 8, 6, 9, "Carbohidratos + proteína + líquido", FoodShape.Glass, "#7a4a2a", "#ffffff"),
            new Food("Frutos secos", true, 5, 0, 6, "Proteína y energía", FoodShape.Nuts, "#b07a45", "#8a5a30"),
            new Food("Gaseosa", false, 3, -6, 0, "Azúcar y gas: no hidrata bien", FoodShape.Can, "#d63447", "#ffffff"),
            new Food("Cerveza", false, 0, -14, -4, "El alcohol deshidrata y frena la recuperación", FoodShape.Glass, "#f2b01e", "#ffffff"),
            new Food("Papas fritas", false, 3, -7, 0, "Grasa y sal: digestión lenta", FoodShape.Fries, "#ffd24a", "#d63447"),
            new Food("Hamburguesa", false, 3, -3, 2, "Mucha grasa: no es prioritaria ahora", FoodShape.Burger, "#c98a3e", "#5a2e14"),
            new Food("Dona", false, 3, -3, -2, "Azúcar y grasa, pocos nutrientes", FoodShape.Donut, "#f29bc0", "#c98a3e"),
            new Food("Energizante", false, 2, -9, 0, "Cafeína: no repone líquidos", FoodShape.Can, "#2b2b2b", "#b6ff3b"),
            new Food("Golosinas", false, 4, -2, -2, "Azúcar vacío: pico y bajón", FoodShape.Candy, "#b388ff", "#ff6bd6"),
            new Food("Pancho", false, 3, -4, 1, "Ultraprocesado y salado", FoodShape.Hotdog, "#e0a060", "#b0412a"),
        };
    }
}
