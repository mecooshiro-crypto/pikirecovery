// PIKI RECOVERY · PikiBody (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System.Collections.Generic;
using UnityEngine;

namespace Piki
{
    public class PikiBody : MonoBehaviour
    {
        Transform figure, marker, markerRing, scan, fxRoot; Material mMarker, mGlow, mRing;
        public bool spin = true; float t; ParticleSystem fx; readonly List<Transform> fxRings = new List<Transform>(); Transform pack; Treat? fxKind;

        public static PikiBody Create(Transform parent, Vector3 localPos)
        {
            var root = Build.Group("Holograma", parent, localPos);
            Vector3 flat = new Vector3(localPos.x, 0, localPos.z);
            root.localRotation = Quaternion.LookRotation(flat.normalized); // el frente del cuerpo (-Z) mira al usuario
            var b = root.gameObject.AddComponent<PikiBody>(); b.Make(); return b;
        }
        void Make()
        {
            var baseM = Mat.Lit(Pal.Hex("#0b1b2b"), .8f, .6f);
            Build.Prim(PrimitiveType.Cylinder, transform, new Vector3(0, .04f, 0), new Vector3(1.0f, .04f, 1.0f), baseM);
            var ring = Build.MeshObj("aro", transform, Build.Floor(1.12f, 1.12f), Mat.Unlit(Pal.Cyan, Spr.Ring.texture, 3000)); ring.transform.localPosition = new Vector3(0, .085f, 0);
            Build.Prim(PrimitiveType.Cylinder, transform, new Vector3(0, 1.05f, 0), new Vector3(.95f, 1f, .95f), Mat.Unlit(Pal.A(Pal.Cyan, .06f), null, 2990));
            var sc = Build.MeshObj("escaneo", transform, Build.Floor(.9f, .9f), Mat.Unlit(Pal.A(Pal.Hex("#9ff0ff"), .9f), Spr.Ring.texture, 3001)); scan = sc.transform;

            figure = Build.Group("Figura", transform, new Vector3(0, .08f, 0));
            var m = Mat.Unlit(new Color(.22f, .84f, 1f, .42f), null, 3000);
            System.Action<PrimitiveType, Vector3, Vector3, Vector3> P = (pt, p, s, e) => Build.Prim(pt, figure, p, s, m, e);
            P(PrimitiveType.Sphere, new Vector3(0, 1.62f, 0), Vector3.one * .22f, Vector3.zero);
            P(PrimitiveType.Cylinder, new Vector3(0, 1.49f, 0), new Vector3(.1f, .05f, .1f), Vector3.zero);
            P(PrimitiveType.Capsule, new Vector3(0, 1.24f, 0), new Vector3(.39f, .33f, .22f), Vector3.zero);
            P(PrimitiveType.Sphere, new Vector3(0, .96f, 0), new Vector3(.36f, .22f, .24f), Vector3.zero);
            foreach (int s in new[] { -1, 1 })
            {
                P(PrimitiveType.Capsule, new Vector3(s * .25f, 1.3f, 0), new Vector3(.1f, .17f, .1f), new Vector3(0, 0, s * 6));
                P(PrimitiveType.Capsule, new Vector3(s * .285f, 1.02f, -.02f), new Vector3(.086f, .155f, .086f), new Vector3(0, 0, s * 3));
                P(PrimitiveType.Sphere, new Vector3(s * .3f, .84f, -.03f), Vector3.one * .1f, Vector3.zero);
                P(PrimitiveType.Capsule, new Vector3(s * .1f, .7f, 0), new Vector3(.15f, .225f, .15f), Vector3.zero);
                P(PrimitiveType.Capsule, new Vector3(s * .1f, .32f, 0), new Vector3(.116f, .208f, .116f), Vector3.zero);
                P(PrimitiveType.Cube, new Vector3(s * .1f, .03f, -.05f), new Vector3(.09f, .06f, .22f), Vector3.zero);
            }
            marker = Build.Group("Marcador", figure);
            mMarker = Mat.Unlit(Pal.Red, null, 3100); mGlow = Mat.Unlit(Pal.A(Pal.Red, .9f), Tex.Glow, 3101); mRing = Mat.Unlit(Pal.Red, Spr.Ring.texture, 3102);
            Build.Prim(PrimitiveType.Sphere, marker, Vector3.zero, Vector3.one * .12f, mMarker);
            var g = Build.MeshObj("brillo", marker, Build.Wall(1, 1), mGlow); g.transform.localScale = Vector3.one * .5f; g.AddComponent<Billboard>();
            var r = Build.MeshObj("pulso", marker, Build.Wall(1, 1), mRing); r.AddComponent<Billboard>(); markerRing = r.transform;
            fxRoot = Build.Group("FX", marker);
            marker.gameObject.SetActive(false);
            transform.localScale = Vector3.one * .001f; StartCoroutine(Tw.Co(.9f, k => transform.localScale = Vector3.one * Mathf.Max(.001f, k), Ease.Out));
        }
        public void SetMarker(Vector3 p) { marker.localPosition = p; marker.gameObject.SetActive(true); SetColor(Pal.Red); }
        public void HideMarker() { marker.gameObject.SetActive(false); }
        public void SetColor(Color c) { mMarker.color = c; mGlow.color = Pal.A(c, .9f); mRing.color = c; }
        public Coroutine FaceZone(Vector3 p)
        {
            spin = false; float from = figure.localEulerAngles.y, to = p.z > .03f ? 180 : 0;
            float delta = Mathf.DeltaAngle(from, to);
            return StartCoroutine(Tw.Co(.9f, k => figure.localEulerAngles = new Vector3(0, from + delta * k, 0)));
        }
        void Update()
        {
            t += Time.deltaTime;
            if (spin) figure.Rotate(0, 32 * Time.deltaTime, 0);
            scan.localPosition = new Vector3(0, .1f + (Mathf.Sin(t * 1.3f) * .5f + .5f) * 1.75f, 0);
            float p = 1 + Mathf.Sin(t * 6) * .18f; marker.GetChild(0).localScale = Vector3.one * .12f * p;
            float k = (t * .8f) % 1; markerRing.localScale = Vector3.one * (.24f + k * .38f); mRing.color = Pal.A(mRing.color, .8f * (1 - k));
            if (fxKind.HasValue)
            {
                if (pack != null) pack.localPosition = Vector3.Lerp(pack.localPosition, new Vector3(0, 0, 0), 1 - Mathf.Exp(-Time.deltaTime * 3));
                for (int i = 0; i < fxRings.Count; i++) { float q = (t * .9f + i / 3f) % 1; fxRings[i].localScale = Vector3.one * (.16f + q * .5f); fxRings[i].GetComponent<Renderer>().sharedMaterial.color = Pal.A(Pal.Green, 1 - q); }
            }
        }
        public void StartFX(Treat kind)
        {
            fxKind = kind; Color c = kind == Treat.Frio ? Pal.Hex("#7fd8ff") : kind == Treat.Calor ? Pal.Orange : Pal.Green;
            fx = Build.Particles(fxRoot, Vector3.zero, c, .035f, 200);
            var main = fx.main; main.startLifetime = 1.4f; main.startSpeed = 0; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSize = new ParticleSystem.MinMaxCurve(.02f, .045f); main.gravityModifier = kind == Treat.Frio ? .02f : kind == Treat.Calor ? -.03f : 0;
            var em = fx.emission; em.enabled = true; em.rateOverTime = 40;
            var sh = fx.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = .2f;
            fx.GetComponent<Renderer>().sharedMaterial.renderQueue = 3200;
            fx.Play();
            if (kind == Treat.Masaje)
                for (int i = 0; i < 3; i++) { var r = Build.MeshObj("onda", fxRoot, Build.Wall(1, 1), Mat.Unlit(Pal.Green, Spr.Ring.texture, 3150)); r.AddComponent<Billboard>(); fxRings.Add(r.transform); }
            else
            {
                var pk = Build.Prim(PrimitiveType.Cube, fxRoot, new Vector3(0, 0, -.45f), new Vector3(.2f, .14f, .05f), Mat.Unlit(kind == Treat.Frio ? new Color(.75f, .94f, 1, .9f) : new Color(1, .6f, .35f, .9f), null, 3150));
                pk.AddComponent<Billboard>(); pack = pk.transform;
            }
            Build.Glow(fxRoot, Vector3.zero, .45f, Pal.A(c, .7f)).GetComponent<Renderer>().sharedMaterial.renderQueue = 3099;
        }
        public void StopFX()
        {
            fxKind = null; fxRings.Clear(); pack = null;
            foreach (Transform ch in fxRoot) Destroy(ch.gameObject);
        }
    }
}
