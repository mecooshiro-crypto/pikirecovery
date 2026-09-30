// PIKI RECOVERY · UIText (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
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
}
