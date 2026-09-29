// =====================================================================
//  PIKI RECOVERY · Modelos procedurales: holograma corporal (etapa 1),
//  efectos de tratamiento y alimentos 3D (etapa 2).
// =====================================================================
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

    /* ------------------------------ Alimentos 3D ------------------------------ */
    public static class FoodModel
    {
        public static Transform Make(Food f, Transform parent)
        {
            var g = Build.Group(f.name, parent); var a = Mat.Lit(f.c1, .45f); var b = Mat.Lit(f.c2, .45f);
            System.Func<PrimitiveType, Vector3, Vector3, Material, Vector3, GameObject> P = (t, p, s, m, e) => Build.Prim(t, g, p, s, m, e);
            Vector3 z = Vector3.zero;
            switch (f.shape)
            {
                case FoodShape.Bottle:
                    P(PrimitiveType.Cylinder, z, new Vector3(.13f, .14f, .13f), a, z); P(PrimitiveType.Cylinder, new Vector3(0, .02f, 0), new Vector3(.135f, .04f, .135f), b, z);
                    P(PrimitiveType.Cylinder, new Vector3(0, .17f, 0), new Vector3(.06f, .03f, .06f), Mat.Lit(Color.white, .5f), z); break;
                case FoodShape.Can:
                    P(PrimitiveType.Cylinder, z, new Vector3(.14f, .12f, .14f), a, z); P(PrimitiveType.Cylinder, new Vector3(0, .02f, 0), new Vector3(.145f, .03f, .145f), b, z);
                    P(PrimitiveType.Cylinder, new Vector3(0, .125f, 0), new Vector3(.12f, .006f, .12f), Mat.Lit(new Color(.75f, .75f, .78f), .8f, .8f), z); break;
                case FoodShape.Sphere:
                    P(PrimitiveType.Sphere, z, Vector3.one * .25f, a, z); P(PrimitiveType.Cube, new Vector3(.02f, .13f, 0), new Vector3(.06f, .02f, .03f), b, new Vector3(0, 0, 25)); break;
                case FoodShape.Slice:
                    P(PrimitiveType.Cylinder, z, new Vector3(.32f, .03f, .32f), a, new Vector3(90, 0, 0)); P(PrimitiveType.Cube, new Vector3(0, -.14f, 0), new Vector3(.3f, .05f, .065f), b, z);
                    for (int i = -1; i <= 1; i++) P(PrimitiveType.Sphere, new Vector3(i * .07f, .03f, -.035f), new Vector3(.02f, .03f, .01f), Mat.Lit(new Color(.1f, .1f, .1f)), z); break;
                case FoodShape.Banana:
                    for (int i = -2; i <= 2; i++) P(PrimitiveType.Capsule, new Vector3(i * .06f, -Mathf.Abs(i) * -.025f - .03f, 0), new Vector3(.07f, .05f, .07f), a, new Vector3(0, 0, 90 + i * 18));
                    P(PrimitiveType.Sphere, new Vector3(-.15f, .04f, 0), Vector3.one * .03f, b, z); break;
                case FoodShape.Bowl:
                    P(PrimitiveType.Cylinder, new Vector3(0, -.04f, 0), new Vector3(.3f, .05f, .3f), Mat.Lit(Color.white, .6f), z); P(PrimitiveType.Sphere, new Vector3(0, .02f, 0), new Vector3(.27f, .1f, .27f), a, z); break;
                case FoodShape.Bread:
                    P(PrimitiveType.Cube, z, new Vector3(.3f, .12f, .16f), a, z); P(PrimitiveType.Capsule, new Vector3(0, .06f, 0), new Vector3(.16f, .15f, .16f), b, new Vector3(0, 0, 90)); break;
                case FoodShape.Drumstick:
                    P(PrimitiveType.Sphere, new Vector3(-.04f, 0, 0), new Vector3(.2f, .15f, .15f), a, z); P(PrimitiveType.Cylinder, new Vector3(.1f, 0, 0), new Vector3(.04f, .07f, .04f), b, new Vector3(0, 0, 90));
                    P(PrimitiveType.Sphere, new Vector3(.17f, .02f, 0), Vector3.one * .045f, b, z); P(PrimitiveType.Sphere, new Vector3(.17f, -.02f, 0), Vector3.one * .045f, b, z); break;
                case FoodShape.Egg:
                    P(PrimitiveType.Sphere, z, new Vector3(.17f, .22f, .17f), a, z); break;
                case FoodShape.Fish:
                    P(PrimitiveType.Sphere, z, new Vector3(.3f, .13f, .08f), a, z); P(PrimitiveType.Cube, new Vector3(.17f, 0, 0), new Vector3(.08f, .08f, .02f), b, new Vector3(0, 0, 45));
                    P(PrimitiveType.Sphere, new Vector3(-.1f, .02f, -.035f), Vector3.one * .025f, Mat.Lit(new Color(.05f, .05f, .05f)), z); break;
                case FoodShape.Glass:
                    P(PrimitiveType.Cylinder, z, new Vector3(.14f, .12f, .14f), a, z); P(PrimitiveType.Cylinder, new Vector3(0, .12f, 0), new Vector3(.142f, .015f, .142f), b, z); break;
                case FoodShape.Nuts:
                    for (int i = 0; i < 7; i++) { float an = i * 2.4f; P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(an) * .07f * (i % 3), (i % 2) * .04f, Mathf.Sin(an) * .05f), new Vector3(.08f, .06f, .06f), i % 2 == 0 ? a : b, new Vector3(0, i * 40, 20)); }
                    break;
                case FoodShape.Burger:
                    P(PrimitiveType.Cylinder, new Vector3(0, -.06f, 0), new Vector3(.28f, .025f, .28f), a, z); P(PrimitiveType.Cylinder, new Vector3(0, -.02f, 0), new Vector3(.3f, .025f, .3f), b, z);
                    P(PrimitiveType.Cube, new Vector3(0, .01f, 0), new Vector3(.27f, .01f, .27f), Mat.Lit(Pal.Yellow, .4f), new Vector3(0, 45, 0));
                    P(PrimitiveType.Cylinder, new Vector3(0, .02f, 0), new Vector3(.3f, .008f, .3f), Mat.Lit(Pal.Hex("#5fbf4a"), .4f), z);
                    P(PrimitiveType.Sphere, new Vector3(0, .05f, 0), new Vector3(.28f, .13f, .28f), a, z); break;
                case FoodShape.Fries:
                    P(PrimitiveType.Cube, new Vector3(0, -.05f, 0), new Vector3(.17f, .14f, .09f), b, z);
                    for (int i = 0; i < 7; i++) P(PrimitiveType.Cube, new Vector3(-.06f + i * .02f, .05f + (i % 3) * .015f, (i % 2) * .02f - .01f), new Vector3(.015f, .14f, .015f), a, new Vector3(0, 0, (i - 3) * 5)); break;
                case FoodShape.Donut:
                    for (int i = 0; i < 12; i++) { float an = i * Mathf.PI * 2 / 12; P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(an) * .1f, Mathf.Sin(an) * .1f, 0), Vector3.one * .085f, b, z); P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(an) * .1f, Mathf.Sin(an) * .1f, -.02f), new Vector3(.075f, .075f, .06f), a, z); }
                    break;
                case FoodShape.Candy:
                    P(PrimitiveType.Sphere, z, Vector3.one * .15f, a, z); P(PrimitiveType.Cube, new Vector3(-.11f, 0, 0), new Vector3(.07f, .09f, .02f), b, new Vector3(0, 0, 45)); P(PrimitiveType.Cube, new Vector3(.11f, 0, 0), new Vector3(.07f, .09f, .02f), b, new Vector3(0, 0, 45)); break;
                case FoodShape.Hotdog:
                    P(PrimitiveType.Capsule, new Vector3(0, -.02f, 0), new Vector3(.12f, .15f, .1f), a, new Vector3(0, 0, 90)); P(PrimitiveType.Capsule, new Vector3(0, .03f, 0), new Vector3(.06f, .19f, .06f), b, new Vector3(0, 0, 90));
                    P(PrimitiveType.Cube, new Vector3(0, .065f, 0), new Vector3(.22f, .01f, .02f), Mat.Lit(Pal.Yellow, .4f), new Vector3(0, 0, 0)); break;
            }
            return g;
        }
    }
}
