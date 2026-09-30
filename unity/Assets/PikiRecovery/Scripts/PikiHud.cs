// PIKI RECOVERY · PikiHud (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
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
