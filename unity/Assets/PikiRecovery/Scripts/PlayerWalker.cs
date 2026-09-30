// PIKI RECOVERY · PlayerWalker (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    /* ------------------------------ Jugador animado ------------------------------ */
    public class PlayerWalker : MonoBehaviour
    {
        public Transform[] legs = new Transform[2], arms = new Transform[2];
        public Vector3 target = new Vector3(-40.5f, 0, 0);
        float phase, speed, idle;
        public static PlayerWalker Make(Transform parent, Color jersey, Color shorts, Color socks, int seed)
        {
            var g = Build.Group("Jugador " + (seed + 1), parent); var w = g.gameObject.AddComponent<PlayerWalker>();
            Color[] skins = { Pal.Hex("#f1c7a5"), Pal.Hex("#d9a47f"), Pal.Hex("#a86f4c"), Pal.Hex("#6d4430") };
            Material mJ = Mat.Lit(jersey, .3f), mS = Mat.Lit(shorts, .3f), mK = Mat.Lit(skins[seed % skins.Length], .3f), mSo = Mat.Lit(socks, .3f), mB = Mat.Lit(new Color(.07f, .07f, .07f), .5f);
            Build.Prim(PrimitiveType.Capsule, g, new Vector3(0, 1.24f, 0), new Vector3(.38f, .36f, .24f), mJ);
            Build.Prim(PrimitiveType.Cylinder, g, new Vector3(0, .9f, 0), new Vector3(.4f, .12f, .36f), mS);
            Build.Prim(PrimitiveType.Sphere, g, new Vector3(0, 1.67f, 0), Vector3.one * .22f, mK);
            Build.Prim(PrimitiveType.Sphere, g, new Vector3(0, 1.7f, -.01f), new Vector3(.23f, .17f, .23f), Mat.Lit(new Color(.1f, .07f, .05f), .2f));
            for (int i = 0; i < 2; i++)
            {
                float s = i == 0 ? -1 : 1;
                var hip = Build.Group("cadera", g, new Vector3(s * .1f, .86f, 0));
                Build.Prim(PrimitiveType.Capsule, hip, new Vector3(0, -.38f, 0), new Vector3(.13f, .4f, .13f), mK);
                Build.Prim(PrimitiveType.Cylinder, hip, new Vector3(0, -.6f, 0), new Vector3(.14f, .17f, .14f), mSo);
                Build.Prim(PrimitiveType.Cube, hip, new Vector3(0, -.82f, .05f), new Vector3(.1f, .08f, .26f), mB);
                w.legs[i] = hip;
                var sho = Build.Group("hombro", g, new Vector3(s * .25f, 1.44f, 0));
                Build.Prim(PrimitiveType.Capsule, sho, new Vector3(0, -.3f, 0), new Vector3(.09f, .3f, .09f), mK);
                Build.Prim(PrimitiveType.Cylinder, sho, new Vector3(0, -.07f, 0), new Vector3(.13f, .09f, .13f), mJ);
                w.arms[i] = sho;
            }
            // posición de ejemplo en la cancha (se sortea de nuevo al dar Play)
            float a = seed * 2.4f; g.localPosition = new Vector3(-8 + Mathf.Cos(a) * 14, 0, 10 + Mathf.Sin(a) * 22);
            g.localRotation = Quaternion.LookRotation(new Vector3(-40.5f, 0, 0) - g.localPosition);
            return w;
        }
        void Start() { ResetPos(); }
        public void ResetPos()
        {
            gameObject.SetActive(true);
            var p = new Vector3(Random.Range(-28f, 25f), 0, Random.Range(-40f, 40f)); if (Vector3.Distance(p, new Vector3(0, 0, -6)) < 9) p.x -= 13;
            transform.localPosition = p; speed = Random.Range(.9f, 1.5f); idle = Random.Range(0f, 5f); phase = Random.value * 6;
        }
        void Update()
        {
            float dt = Time.deltaTime;
            if (idle > 0) { idle -= dt; foreach (var l in legs) l.localRotation = Quaternion.Slerp(l.localRotation, Quaternion.identity, dt * 5); return; }
            Vector3 d = target - transform.localPosition; d.y = 0;
            if (d.magnitude < 1.2f) { gameObject.SetActive(false); return; }
            transform.localPosition += d.normalized * speed * dt; transform.localRotation = Quaternion.LookRotation(d.normalized);
            phase += dt * speed * 5.5f; float s = Mathf.Sin(phase) * 26;
            legs[0].localEulerAngles = new Vector3(s, 0, 0); legs[1].localEulerAngles = new Vector3(-s, 0, 0);
            arms[0].localEulerAngles = new Vector3(-s * .8f, 0, 0); arms[1].localEulerAngles = new Vector3(s * .8f, 0, 0);
        }
    }
}
