// PIKI RECOVERY · PikiButton (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
    /* ------------------------------ Botón ------------------------------ */
    public class PikiButton : MonoBehaviour
    {
        public Action onClick; public Action<bool> onHover; public float hoverScale = 1.06f;
        public bool Disabled { get; private set; }
        public bool Hovered { get; private set; }
        public UIPanel Panel { get { return GetComponent<UIPanel>(); } }
        static float lastClick;
        Vector3 baseScale; float target = 1;
        void Awake() { baseScale = transform.localScale; }
        public void SetHover(bool h)
        {
            if (Disabled) h = false; if (h == Hovered) return; Hovered = h; target = h ? hoverScale : 1;
            if (onHover != null) onHover(h); if (h && PikiAudio.I != null) PikiAudio.I.Hover();
        }
        public void Click()
        {
            if (Disabled || !isActiveAndEnabled) return;
            if (Time.unscaledTime - lastClick < .35f) return; lastClick = Time.unscaledTime;
            if (PikiAudio.I != null) PikiAudio.I.Click();
            if (onClick != null) onClick();
        }
        public void SetDisabled(bool d, float alpha = .35f)
        {
            Disabled = d; if (d) SetHover(false);
            var col = GetComponent<Collider>(); if (col != null) col.enabled = !d;
            var p = Panel; if (p != null && alpha >= 0) p.Alpha = d ? alpha : 1;
        }
        public void Shake()
        {
            float x0 = transform.localPosition.x;
            StartCoroutine(Tw.Co(.4f, k => { var p = transform.localPosition; p.x = x0 + Mathf.Sin(k * Mathf.PI * 6) * .022f * (1 - k); transform.localPosition = p; }, Ease.Lin));
        }
        void Update() { transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, 1 - Mathf.Exp(-Time.deltaTime * 16)); }
    }
}
