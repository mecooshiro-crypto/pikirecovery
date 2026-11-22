// PIKI GAME · UIText (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
    /* ------------------------------ Texto ------------------------------
       Usa Poppins (Manual de marca). Los titulares (negrita y grandes) van en
       Poppins ExtraBold cursiva; el resto en Poppins Bold o Medium.            */
    public class UIText : UIElem
    {
        public const int FontPx = 64; const float Unit = .1f; // TextMesh: 10 px de fuente = 1 unidad
        [SerializeField] TextMesh tm; [SerializeField] float x, baseline, size;
        [SerializeField] bool bold, italic; [SerializeField] float fscale = 1, top = .95f, lineFix = 1, spacing = 1;
        public string text { get { return tm.text; } set { tm.text = value; } }
        public Color color { get { return baseColor; } set { baseColor = value; Refresh(); } }
        public float fontSize { get { return size; } set { size = value; Layout(); } }
        // Interlineado relativo al de la fuente de sistema (1 = 1,15 veces el tamaño)
        public float lineSpacing { get { return spacing; } set { spacing = value; if (tm != null) tm.lineSpacing = spacing * lineFix; } }
        public void Init(UIPanel p, string s, float x, float baseline, float size, Color c, bool bold, bool italic, UI.Al al)
        {
            panel = p; this.x = x; this.baseline = baseline; this.size = size; baseColor = c; this.bold = bold; this.italic = italic;
            var f = Brand.FontFor(bold, italic, size); bool pop = Brand.IsPoppins(f);
            tm = gameObject.AddComponent<TextMesh>(); tm.font = f; tm.fontSize = FontPx; tm.richText = true; tm.text = s;
            tm.fontStyle = pop ? FontStyle.Normal : bold ? (italic ? FontStyle.BoldAndItalic : FontStyle.Bold) : (italic ? FontStyle.Italic : FontStyle.Normal);
            tm.anchor = al == UI.Al.L ? TextAnchor.UpperLeft : al == UI.Al.C ? TextAnchor.UpperCenter : TextAnchor.UpperRight;
            tm.alignment = al == UI.Al.L ? TextAlignment.Left : al == UI.Al.C ? TextAlignment.Center : TextAlignment.Right;
            fscale = pop ? Brand.PoppinsScale : 1f;
            top = pop ? Brand.Ascent(f) + .045f : .95f;                 // de la línea base al borde superior de la línea
            lineFix = 1.15f / (Brand.LineHeight(f) * fscale);            // mantiene el interlineado del diseño
            lineSpacing = 1;
            var mr = GetComponent<MeshRenderer>(); mr.sharedMaterial = UI.TextMat(f, p.Queue); mr.sortingOrder = p.NextOrder();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            Layout(); Refresh();
        }
        void Layout()
        {
            tm.characterSize = size * fscale / (FontPx * Unit);
            float t = baseline - size * fscale * top;
            transform.localPosition = new Vector3(x - panel.W / 2, panel.H / 2 - t, 0);
        }
        // Achica el texto si no entra en el ancho indicado
        public void FitWidth(float maxW)
        {
            float wNow = UI.TextWidth(tm.text, size, tm.font, bold);
            if (wNow > maxW && wNow > 0) fontSize = size * maxW / wNow;
        }
        public override void Refresh() { if (tm == null) return; var c = baseColor; c.a *= ChainAlpha(); tm.color = c; }
    }
}
