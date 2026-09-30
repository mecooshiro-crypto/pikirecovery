// PIKI RECOVERY · UIImage (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
    /* ------------------------------ Imagen ------------------------------ */
    public class UIImage : UIElem
    {
        [SerializeField] SpriteRenderer sr; [SerializeField] float x, y, w, h, radius; [SerializeField] bool sliced;
        public Color color { get { return baseColor; } set { baseColor = value; Refresh(); } }
        public Vector2 Size { get { return new Vector2(w, h); } set { w = value.x; h = value.y; Layout(); } }
        public void Init(UIPanel p, float x, float y, float w, float h, Sprite s, Color c, float radius)
        {
            panel = p; this.x = x; this.y = y; this.w = w; this.h = h; baseColor = c;
            sr = gameObject.AddComponent<SpriteRenderer>(); sr.sprite = s != null ? s : Spr.White;
            sr.sharedMaterial = UI.SpriteMat(p.Queue); sr.sortingOrder = p.NextOrder();
            sliced = sr.sprite.border.x > 0; this.radius = radius > 0 ? radius : 30;
            Layout(); Refresh();
        }
        void Layout()
        {
            transform.localPosition = panel.Center(x, y, w, h);
            if (sliced)
            {
                float r = Mathf.Min(radius, Mathf.Min(w, h) / 2); float m = Mathf.Max(.001f, r * 2.083f);
                sr.drawMode = SpriteDrawMode.Sliced; transform.localScale = Vector3.one * m; sr.size = new Vector2(Mathf.Max(.01f, w / m), Mathf.Max(.01f, h / m));
            }
            else
            {
                var n = sr.sprite.bounds.size; sr.drawMode = SpriteDrawMode.Simple;
                transform.localScale = new Vector3(Mathf.Max(.0001f, w / n.x), Mathf.Max(.0001f, h / n.y), 1);
            }
        }
        public override void Refresh() { if (sr == null) return; var c = baseColor; c.a *= ChainAlpha(); sr.color = c; }
    }
}
