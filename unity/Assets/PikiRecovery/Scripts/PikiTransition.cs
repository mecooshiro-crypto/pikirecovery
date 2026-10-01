// =====================================================================
//  PIKI RECOVERY · Escena de fundido (cartel negro entre etapas).
//  Los textos se editan directo en la escena (objetos Kicker, Título y
//  Subtítulo). Desde el Inspector se cambian la escena siguiente y los
//  tiempos.
// =====================================================================
using System.Collections;
using UnityEngine;

namespace Piki
{
    public class PikiTransition : MonoBehaviour
    {
        [Tooltip("Escena que se carga al terminar el fundido")] public string nextScene = "01_Cancha";
        [Tooltip("Segundos que tarda en aparecer el cartel")] public float fadeIn = .6f;
        [Tooltip("Segundos que el cartel queda en pantalla")] public float hold = 1.8f;
        [Tooltip("Segundos que tarda en desaparecer el cartel")] public float fadeOut = .5f;
        public UIPanel card;

        IEnumerator Start()
        {
            if (card != null) card.Alpha = 0;
            yield return null;
            yield return Tw.Co(fadeIn, k => { if (card != null) card.Alpha = k; });
            yield return new WaitForSeconds(hold);
            yield return Tw.Co(fadeOut, k => { if (card != null) card.Alpha = 1 - k; });
            if (!string.IsNullOrEmpty(nextScene)) UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }

        // Arma la escena de fundido (lo usa el constructor de escenas)
        public static PikiTransition Bake(string kicker, string title, string sub, Color color, string next, float hold)
        {
            RenderSettings.skybox = null; RenderSettings.fog = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = Color.black;
            var rig = PikiRig.Create(null);
            rig.Cam.backgroundColor = Color.black; rig.Cam.clearFlags = CameraClearFlags.SolidColor;
            var go = new GameObject("Fundido (cartel)"); var t = go.AddComponent<PikiTransition>();
            t.nextScene = next; t.hold = hold;
            var card = UI.Canvas(go.transform, new Vector3(0, 1.6f, 1.2f), 1.6f, .6f, Vector3.zero, "Cartel");
            float W = UI.Wd(card), H = UI.Ht(card);
            UI.T(card, kicker, W / 2, H * .3f, 26, color, true, UI.Al.C, W).name = "Kicker";
            UI.T(card, title, W / 2, H * .6f, 70, Color.white, true, UI.Al.C, W).name = "Título";
            UI.T(card, sub, W / 2, H * .84f, 26, Pal.Muted, false, UI.Al.C, W).name = "Subtítulo";
            UI.Round(card, W * .3f, H * .68f, W * .4f, 3, Pal.A(color, .7f), 1.5f).name = "Línea";
            t.card = card;
            return t;
        }
    }
}
