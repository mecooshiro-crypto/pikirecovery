// PIKI RECOVERY · LedScroll (componente: cada componente va en su propio archivo para que Unity lo guarde en las escenas)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Piki
{
    // Cartel LED: el texto corre letra por letra
    public class LedScroll : MonoBehaviour
    {
        public string content = ""; public int visible = 40; public float step = .12f; int offset; float t;
        void Update() { t += Time.deltaTime; if (t < step) return; t = 0; offset = (offset + 1) % Mathf.Max(1, content.Length); Apply(); }
        public void Apply()
        {
            var ui = GetComponent<UIText>(); if (ui == null || content.Length == 0) return;
            var sb = new System.Text.StringBuilder(); int i = offset; while (sb.Length < visible) { sb.Append(content[i % content.Length]); i++; }
            ui.text = sb.ToString();
        }
    }
}
