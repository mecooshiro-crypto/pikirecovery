// PIKI RECOVERY · UIElem (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
    public abstract class UIElem : MonoBehaviour
    {
        public UIPanel panel; [SerializeField] protected Color baseColor = Color.white;
        protected float ChainAlpha()
        {
            float a = 1; var t = transform;
            while (t != null) { var p = t.GetComponent<UIPanel>(); if (p != null) a *= p.AlphaSelf; t = t.parent; }
            return a;
        }
        public abstract void Refresh();
    }
}
