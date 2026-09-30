// PIKI RECOVERY · UIPanel (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Piki
{
    /* ------------------------------ Panel ------------------------------ */
    public class UIPanel : MonoBehaviour
    {
        public float W, H; public UIPanel root; public int baseOrder, queue;
        [SerializeField] int counter; [SerializeField] float alpha = 1;
        public UIPanel Root { get { return root != null ? root : this; } }
        public int NextOrder() { var r = Root; return r.baseOrder + (r.counter++); }
        public int Queue { get { return Root.queue; } }
        public float AlphaSelf { get { return alpha; } }
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; foreach (var e in GetComponentsInChildren<UIElem>(true)) e.Refresh(); }
        }
        // atajos para tratar al panel como un Transform
        public Vector3 localPosition { get { return transform.localPosition; } set { transform.localPosition = value; } }
        public Quaternion localRotation { get { return transform.localRotation; } set { transform.localRotation = value; } }
        public Vector3 localScale { get { return transform.localScale; } set { transform.localScale = value; } }
        public Vector3 localEulerAngles { get { return transform.localEulerAngles; } set { transform.localEulerAngles = value; } }
        // Posición local (centro) de un rectángulo dado en coordenadas de panel
        public Vector3 Center(float x, float y, float w, float h) { return new Vector3(x + w / 2 - W / 2, H / 2 - y - h / 2, 0); }
    }
}
