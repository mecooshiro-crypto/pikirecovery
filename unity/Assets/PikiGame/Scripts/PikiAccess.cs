// =====================================================================
//  PIKI GAME · Opciones de accesibilidad.
//  Se eligen en la pantalla de inicio (botón "Accesibilidad") y quedan
//  guardadas en el equipo (PlayerPrefs) para las próximas partidas.
//
//  Visual ........ Texto grande · Alto contraste · Reducir movimiento
//  Auditiva ...... Subtítulos de sonidos · Volumen de la música
//                  (además: todo sonido importante tiene su señal visual
//                   y vibración en los controles VR)
//  Motriz ........ Agarre fácil (un toque, sin mantener ni mover en círculos)
//  Cognitiva ..... Ritmo tranquilo (más tiempo y más margen de error)
//  Física ........ La interfaz se acomoda a la altura de la cabeza (sentado
//                  o parado) y todo se puede jugar con un solo botón o con
//                  la mirada.
// =====================================================================
using System;
using UnityEngine;

namespace Piki
{
    public static class Access
    {
        public static Action Changed;
        const string P = "Piki.Acceso.";
        // Se guardan en memoria para no leer PlayerPrefs en cada cuadro
        static readonly System.Collections.Generic.Dictionary<string, bool> cache = new System.Collections.Generic.Dictionary<string, bool>();
        static bool Get(string k, bool d)
        {
            bool v; if (cache.TryGetValue(k, out v)) return v;
            v = PlayerPrefs.GetInt(P + k, d ? 1 : 0) == 1; cache[k] = v; return v;
        }
        static void Set(string k, bool v) { cache[k] = v; PlayerPrefs.SetInt(P + k, v ? 1 : 0); PlayerPrefs.Save(); if (Changed != null) Changed(); }

        public static bool BigText { get { return Get("TextoGrande", false); } set { Set("TextoGrande", value); } }
        public static bool HighContrast { get { return Get("AltoContraste", false); } set { Set("AltoContraste", value); } }
        public static bool ReduceMotion { get { return Get("ReducirMovimiento", false); } set { Set("ReducirMovimiento", value); } }
        public static bool Captions { get { return Get("Subtitulos", true); } set { Set("Subtitulos", value); } }
        public static bool EasyGrab { get { return Get("AgarreFacil", false); } set { Set("AgarreFacil", value); } }
        public static bool CalmPace { get { return Get("RitmoTranquilo", false); } set { Set("RitmoTranquilo", value); } }

        static readonly float[] musicSteps = { 1f, .6f, .3f, 0f };
        static readonly string[] musicNames = { "100 %", "60 %", "30 %", "Sin música" };
        public static int MusicLevel
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(P + "Musica", 0), 0, musicSteps.Length - 1); }
            set { PlayerPrefs.SetInt(P + "Musica", (value % musicSteps.Length + musicSteps.Length) % musicSteps.Length); PlayerPrefs.Save(); if (Changed != null) Changed(); }
        }
        public static float MusicVolume { get { return musicSteps[MusicLevel]; } }
        public static string MusicName { get { return musicNames[MusicLevel]; } }

        // Escala de los paneles principales con "Texto grande"
        public static float UIScale { get { return BigText ? 1.15f : 1f; } }

        public static void ResetAll()
        {
            foreach (var k in new[] { "TextoGrande", "AltoContraste", "ReducirMovimiento", "Subtitulos", "AgarreFacil", "RitmoTranquilo", "Musica" }) PlayerPrefs.DeleteKey(P + k);
            cache.Clear(); PlayerPrefs.Save(); if (Changed != null) Changed();
        }
    }
}
