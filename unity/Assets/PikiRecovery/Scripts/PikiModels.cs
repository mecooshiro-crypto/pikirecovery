// =====================================================================
//  PIKI RECOVERY · Modelos procedurales: holograma corporal (etapa 1),
//  efectos de tratamiento y alimentos 3D (etapa 2).
// =====================================================================
using System.Collections.Generic;
using UnityEngine;

namespace Piki
{

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
