// =====================================================================
//  PIKI RECOVERY · Herramienta de recuperación que se agarra y se pasa
//  por la zona afectada (bolsa de hielo, compresa tibia, pelota de masaje).
// =====================================================================
using UnityEngine;

namespace Piki
{
    public class PikiTool : MonoBehaviour
    {
        public Treat kind; public Vector3 home; public GameObject badge;
        public static Transform Focus; // zona del cuerpo sobre la que se arrastra
        public bool Disabled { get; private set; }
        public bool Held { get; private set; }
        public float Speed { get; private set; } // velocidad (m/s), para el masaje
        Vector3 target, lastPos; float baseScale = 1;

        void Start() { baseScale = transform.localScale.x; lastPos = transform.position; }

        public void Grab()
        {
            if (Disabled) return; Held = true; target = transform.position;
            var c = GetComponent<Collider>(); if (c != null) c.enabled = false;
            if (PikiAudio.I != null) PikiAudio.I.Soft();
        }
        public void Release()
        {
            Held = false; var c = GetComponent<Collider>(); if (c != null) c.enabled = !Disabled;
        }
        // Mueve la herramienta sobre un plano que pasa por la zona y mira a la cámara
        public void DragTo(Ray ray, Vector3 camPos)
        {
            Vector3 fp = Focus != null ? Focus.position : ray.GetPoint(2);
            Vector3 n = camPos - fp; n.y = 0; if (n.sqrMagnitude < 1e-4f) n = -ray.direction; n.Normalize();
            var plane = new Plane(n, fp + n * .07f); float d;
            target = plane.Raycast(ray, out d) && d > .3f && d < 6 ? ray.GetPoint(d) : ray.GetPoint(1.6f);
        }
        public void SetDisabled(bool d)
        {
            Disabled = d; if (d && Held && PikiRig.I != null) PikiRig.I.ForceRelease();
            var c = GetComponent<Collider>(); if (c != null) c.enabled = !d;
            if (badge != null) badge.SetActive(d);
        }
        void Update()
        {
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            if (Held)
            {
                transform.position = Vector3.Lerp(transform.position, target, 1 - Mathf.Exp(-dt * 22));
                if (PikiRig.CamT != null) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position - PikiRig.CamT.position), 1 - Mathf.Exp(-dt * 10));
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, home + Vector3.up * Mathf.Sin(Time.time * 2 + home.x) * .015f, 1 - Mathf.Exp(-dt * 8));
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, Time.time * 25, 0), 1 - Mathf.Exp(-dt * 4));
            }
            float s = Disabled ? .7f : 1f; transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * baseScale * s, 1 - Mathf.Exp(-dt * 10));
            Speed = Vector3.Distance(transform.position, lastPos) / dt; lastPos = transform.position;
        }
    }
}
