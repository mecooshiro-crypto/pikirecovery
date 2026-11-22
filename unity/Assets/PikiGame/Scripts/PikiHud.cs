// PIKI GAME · PikiHud (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
    /* ------------------------------ Textos en pantalla (PC / celular) ------------------------------
       Barra de instrucciones fija abajo (qué hacer en cada momento) y la marca arriba.
       En VR no se ve: ahí la barra de instrucciones flota frente a la vista (PikiGame).            */
    public class PikiHud : MonoBehaviour
    {
        public string hint = ""; public bool hidden;
        GUIStyle big, small, hintSt; Texture2D bg; bool hc;
        void OnGUI()
        {
            if (hidden) return;
            float s = Mathf.Max(.6f, Screen.height / 720f) * (Access.BigText ? 1.25f : 1f);
            if (big == null || hc != Access.HighContrast)
            {
                hc = Access.HighContrast;
                bg = new Texture2D(1, 1); bg.SetPixel(0, 0, hc ? new Color(0, 0, 0, .95f) : new Color(0, .2f, .243f, .85f)); bg.Apply();
                big = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, font = Brand.FontFor(true, false, 20) }; big.normal.textColor = Pal.Lima;
                small = new GUIStyle(GUI.skin.label) { font = Brand.FontFor(false, false, 20) }; small.normal.textColor = Pal.Muted;
                hintSt = new GUIStyle(GUI.skin.box) { font = Brand.FontFor(true, false, 20), alignment = TextAnchor.MiddleCenter, wordWrap = true }; hintSt.normal.textColor = Pal.Text; hintSt.normal.background = bg;
            }
            big.fontSize = Mathf.RoundToInt(15 * s); small.fontSize = Mathf.RoundToInt(12 * s); hintSt.fontSize = Mathf.RoundToInt(14 * s);
            GUI.Label(new Rect(16 * s, 10 * s, 400 * s, 24 * s), "PIKI GAME", big);
            GUI.Label(new Rect(16 * s, 30 * s, 400 * s, 20 * s), "Fútbol · " + Brand.Claim, small);
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
