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

            // Panel oscuro detrás del holograma para que contraste con la cancha
            var back = UI.Canvas(transform, new Vector3(0, 1.12f, .62f), 1.3f, 2.2f, Vector3.zero, "Pantalla del holograma");
            back.baseOrder = -500; float BW = UI.Wd(back), BH = UI.Ht(back);
            UI.Round(back, 0, 0, BW, BH, Pal.A(Pal.Cyan, .55f), 40);
            UI.Round(back, 4, 4, BW - 8, BH - 8, new Color(.01f, .03f, .07f, .9f), 36);
            for (float gx = 60; gx < BW - 20; gx += 60) UI.Img(back, gx, 20, 1.5f, BH - 40, null, Pal.A(Pal.Cyan, .1f));
            for (float gy = 60; gy < BH - 20; gy += 60) UI.Img(back, 20, gy, BW - 40, 1.5f, null, Pal.A(Pal.Cyan, .1f));
            UI.Img(back, BW * .1f, BH * .08f, BW * .8f, BH * .8f, Spr.Glow, Pal.A(Pal.Cyan, .14f));
            UI.T(back, "ANÁLISIS MUSCULAR", BW / 2, 50, 26, Pal.Cyan, true, UI.Al.C, BW);

            figure = Build.Group("Figura", transform, new Vector3(0, .08f, 0));
            var m = Mat.Hologram(new Color(.3f, .92f, 1f, 1f), new Color(.02f, .13f, .24f, .5f));
            var mJoint = Mat.Hologram(new Color(.55f, 1f, 1f, 1f), new Color(.05f, .22f, .32f, .65f));
            // E = elipsoide (esfera escalada)
            System.Action<string, Vector3, Vector3, Vector3, Material> E = (n, p, size, e, mat) => { var part = Build.Prim(PrimitiveType.Sphere, figure, p, size, mat, e); part.name = n; };
            // Cabeza, cuello y tronco
            E("Cabeza", new Vector3(0, 1.64f, 0), new Vector3(.19f, .24f, .22f), Vector3.zero, m);
            E("Mandíbula", new Vector3(0, 1.57f, -.03f), new Vector3(.13f, .1f, .13f), Vector3.zero, m);
            E("Cuello", new Vector3(0, 1.5f, 0), new Vector3(.11f, .14f, .11f), Vector3.zero, m);
            E("Trapecio", new Vector3(0, 1.45f, .04f), new Vector3(.36f, .11f, .15f), Vector3.zero, m);
            E("Caja torácica", new Vector3(0, 1.28f, .01f), new Vector3(.34f, .36f, .21f), Vector3.zero, m);
            E("Dorsal y espalda", new Vector3(0, 1.22f, .07f), new Vector3(.36f, .3f, .1f), Vector3.zero, m);
            E("Pelvis", new Vector3(0, .95f, 0), new Vector3(.32f, .17f, .2f), Vector3.zero, m);
            float[] absY = { 1.19f, 1.12f, 1.05f };
            foreach (float y in absY) foreach (int sx in new[] { -1, 1 }) E("Abdominal", new Vector3(sx * .042f, y, -.095f), new Vector3(.075f, .062f, .04f), Vector3.zero, m);
            foreach (int s in new[] { -1, 1 })
            {
                string lado = s < 0 ? " der." : " izq.";
                E("Pectoral" + lado, new Vector3(s * .085f, 1.34f, -.085f), new Vector3(.17f, .12f, .08f), new Vector3(0, 0, s * 12), m);
                E("Oblicuo" + lado, new Vector3(s * .13f, 1.1f, -.03f), new Vector3(.08f, .17f, .12f), Vector3.zero, m);
                E("Dorsal" + lado, new Vector3(s * .11f, 1.27f, .075f), new Vector3(.14f, .24f, .07f), new Vector3(0, 0, -s * 8), m);
                E("Lumbar" + lado, new Vector3(s * .04f, 1.06f, .085f), new Vector3(.06f, .17f, .05f), Vector3.zero, m);
                E("Glúteo" + lado, new Vector3(s * .08f, .92f, .075f), new Vector3(.15f, .16f, .12f), Vector3.zero, m);
                // Brazos
                E("Deltoides" + lado, new Vector3(s * .215f, 1.4f, 0), new Vector3(.13f, .14f, .14f), Vector3.zero, m);
                E("Bíceps" + lado, new Vector3(s * .255f, 1.23f, -.025f), new Vector3(.075f, .2f, .075f), new Vector3(0, 0, s * 4), m);
                E("Tríceps" + lado, new Vector3(s * .26f, 1.25f, .03f), new Vector3(.075f, .2f, .075f), new Vector3(0, 0, s * 4), m);
                E("Codo" + lado, new Vector3(s * .275f, 1.11f, 0), Vector3.one * .065f, Vector3.zero, mJoint);
                E("Antebrazo" + lado, new Vector3(s * .29f, .99f, -.01f), new Vector3(.068f, .22f, .068f), new Vector3(0, 0, s * 3), m);
                E("Mano" + lado, new Vector3(s * .3f, .82f, -.01f), new Vector3(.06f, .11f, .035f), Vector3.zero, m);
                // Piernas
                E("Cuádriceps (recto)" + lado, new Vector3(s * .1f, .71f, -.05f), new Vector3(.08f, .3f, .07f), Vector3.zero, m);
                E("Vasto externo" + lado, new Vector3(s * .145f, .68f, -.01f), new Vector3(.075f, .28f, .08f), new Vector3(0, 0, s * 3), m);
                E("Vasto interno" + lado, new Vector3(s * .065f, .6f, -.04f), new Vector3(.07f, .15f, .07f), Vector3.zero, m);
                E("Aductor" + lado, new Vector3(s * .055f, .76f, 0), new Vector3(.06f, .22f, .07f), Vector3.zero, m);
                E("Isquiotibial" + lado, new Vector3(s * .1f, .72f, .05f), new Vector3(.11f, .3f, .08f), Vector3.zero, m);
                E("Rodilla" + lado, new Vector3(s * .1f, .52f, -.02f), Vector3.one * .085f, Vector3.zero, mJoint);
                E("Gemelo interno" + lado, new Vector3(s * .075f, .37f, .04f), new Vector3(.06f, .18f, .07f), Vector3.zero, m);
                E("Gemelo externo" + lado, new Vector3(s * .125f, .38f, .035f), new Vector3(.055f, .16f, .065f), Vector3.zero, m);
                E("Tibial" + lado, new Vector3(s * .1f, .3f, -.025f), new Vector3(.055f, .3f, .055f), Vector3.zero, m);
                E("Tobillo" + lado, new Vector3(s * .1f, .1f, 0), Vector3.one * .06f, Vector3.zero, mJoint);
                E("Pie" + lado, new Vector3(s * .1f, .04f, -.05f), new Vector3(.09f, .06f, .22f), Vector3.zero, m);
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
