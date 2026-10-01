// =====================================================================
//  PIKI RECOVERY · Audio sintetizado (no requiere archivos de sonido).
//  Público, silbato, música del minijuego (3 tempos), ambiente de calma,
//  respiración y efectos de interfaz.
// =====================================================================
using System.Collections;
using UnityEngine;

namespace Piki
{
    public class PikiAudio : MonoBehaviour
    {
        public static PikiAudio I;
        const int SR = 44100;
        AudioSource sfx, crowd, music, pad;
        AudioClip cClick, cHover, cCorrect, cWrong, cSoft, cGood, cBad, cLevel, cBeep, cBeepHi, cWhistle, cWhistleLong, cWhoosh, cCrowd, cPad, cInhale, cExhale, cFinal;
        AudioClip[] cBeat = new AudioClip[4];
        System.Random rng = new System.Random(7);
        public bool Muted { get; private set; }

        void Awake()
        {
            I = this;
            sfx = Src(false, .8f); crowd = Src(true, 0); music = Src(true, 0); pad = Src(true, 0);
            cClick = Tone(.1f, 660, 990, Wave.Tri, .35f);
            cHover = Tone(.05f, 1400, 1400, Wave.Sine, .08f);
            cSoft = Tone(.2f, 300, 240, Wave.Tri, .25f);
            cGood = Mix(Tone(.18f, 520, 1040, Wave.Sine, .45f), Tone(.18f, 1560, 1560, Wave.Sine, .12f, .05f));
            cBad = Tone(.26f, 190, 90, Wave.Square, .18f);
            cBeep = Tone(.18f, 660, 660, Wave.Sine, .4f);
            cBeepHi = Tone(.45f, 1320, 1320, Wave.Sine, .4f);
            cCorrect = Arp(new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, .085f, .45f, Wave.Tri, .32f);
            cLevel = Arp(new[] { 440f, 554f, 659f, 880f }, .07f, .14f, Wave.Square, .14f);
            cFinal = Arp(new[] { 261.63f, 329.63f, 392f, 493.88f, 523.25f, 659.25f }, .12f, 3.2f, Wave.Tri, .16f);
            cWrong = Mix(Tone(.4f, 220, 130, Wave.Saw, .16f), Tone(.4f, 233, 138, Wave.Saw, .11f));
            cWhistle = Whistle(new[] { .24f, .24f, 1.05f });
            cWhistleLong = Whistle(new[] { .7f });
            cWhoosh = NoiseSweep(.7f, 400, 2400, .35f, true);
            cCrowd = Crowd(6f);
            cPad = Pad(8f);
            cInhale = NoiseSweep(4f, 350, 1100, .16f, false);
            cExhale = NoiseSweep(6f, 1100, 300, .16f, false);
            for (int l = 1; l <= 3; l++) cBeat[l] = Beat(l);
        }
        AudioSource Src(bool loop, float vol) { var s = gameObject.AddComponent<AudioSource>(); s.loop = loop; s.volume = vol; s.playOnAwake = false; s.spatialBlend = 0; return s; }

        /* ------------------------------ API ------------------------------ */
        public void Click() { sfx.PlayOneShot(cClick); }
        public void Hover() { sfx.PlayOneShot(cHover); }
        public void Correct() { sfx.PlayOneShot(cCorrect); }
        public void Wrong() { sfx.PlayOneShot(cWrong); }
        public void Soft() { sfx.PlayOneShot(cSoft); }
        public void Good() { sfx.PlayOneShot(cGood); }
        public void Bad() { sfx.PlayOneShot(cBad); }
        public void Level() { sfx.PlayOneShot(cLevel); }
        public void Beep(bool hi) { sfx.PlayOneShot(hi ? cBeepHi : cBeep); }
        public void Whistle(bool longOne = false) { sfx.PlayOneShot(longOne ? cWhistleLong : cWhistle, .7f); }
        public void Whoosh() { sfx.PlayOneShot(cWhoosh); }
        // Notas suaves mientras se aplica el tratamiento (suben con el progreso)
        AudioClip[] cTicks;
        public void ApplyTick(float k)
        {
            if (cTicks == null) { float[] f = { 392f, 440f, 523.25f, 587.33f, 659.25f, 783.99f, 880f, 1046.5f }; cTicks = new AudioClip[f.Length]; for (int i = 0; i < f.Length; i++) cTicks[i] = Tone(.9f, f[i], f[i], Wave.Sine, .12f, 0, .04f); }
            sfx.PlayOneShot(cTicks[Mathf.Clamp(Mathf.FloorToInt(k * cTicks.Length), 0, cTicks.Length - 1)], .7f);
        }
        public void Final() { sfx.PlayOneShot(cFinal, .9f); }
        public void Breath(bool inhale) { sfx.PlayOneShot(inhale ? cInhale : cExhale, .9f); sfx.PlayOneShot(Tone(1.6f, inhale ? 392 : 329.63f, inhale ? 392 : 329.63f, Wave.Sine, .12f, 0, .3f), .6f); }

        public void CrowdStart(float level) { if (!crowd.isPlaying) { crowd.clip = cCrowd; crowd.volume = 0; crowd.Play(); } FadeTo(crowd, level, 2.5f); }
        public void CrowdLevel(float v, float t) { FadeTo(crowd, v, t); }
        public void CrowdCheer() { StartCoroutine(Cheer()); }
        IEnumerator Cheer() { float b = crowd.volume; yield return Tw.Co(.5f, k => crowd.volume = Mathf.Lerp(b, Mathf.Min(1, b * 2.3f), k)); yield return new WaitForSeconds(1f); yield return Tw.Co(3f, k => crowd.volume = Mathf.Lerp(Mathf.Min(1, b * 2.3f), b, k)); }
        public void CrowdStop(float t) { FadeTo(crowd, 0, t, true); }

        public void BeatStart(int level) { music.clip = cBeat[level]; music.volume = 0; music.Play(); FadeTo(music, .55f, 1.2f); }
        public void BeatLevel(int level) { if (!music.isPlaying) return; float pos = music.time / music.clip.length; music.clip = cBeat[level]; music.time = pos * music.clip.length; music.Play(); }
        public void BeatStop(float t) { FadeTo(music, 0, t, true); }

        public void PadStart(float v) { if (!pad.isPlaying) { pad.clip = cPad; pad.volume = 0; pad.Play(); } FadeTo(pad, v, 4); }
        public void PadLevel(float v, float t) { FadeTo(pad, v, t); }
        public void PadStop(float t) { FadeTo(pad, 0, t, true); }

        public void SetMuted(bool m) { Muted = m; AudioListener.volume = m ? 0 : 1; }

        readonly System.Collections.Generic.Dictionary<AudioSource, Coroutine> fades = new System.Collections.Generic.Dictionary<AudioSource, Coroutine>();
        void FadeTo(AudioSource s, float v, float t, bool stopAtEnd = false)
        {
            Coroutine c; if (fades.TryGetValue(s, out c) && c != null) StopCoroutine(c);
            fades[s] = StartCoroutine(FadeCo(s, v, t, stopAtEnd));
        }
        IEnumerator FadeCo(AudioSource s, float v, float t, bool stop)
        {
            float a = s.volume; yield return Tw.Co(Mathf.Max(.01f, t), k => s.volume = Mathf.Lerp(a, v, k), Ease.Lin);
            if (stop && v <= 0) s.Stop();
        }

        /* ------------------------------ Síntesis ------------------------------ */
        enum Wave { Sine, Tri, Square, Saw }
        static float Osc(Wave w, float ph)
        {
            float p = ph - Mathf.Floor(ph);
            switch (w)
            {
                case Wave.Tri: return 4 * Mathf.Abs(p - .5f) - 1;
                case Wave.Square: return p < .5f ? .8f : -.8f;
                case Wave.Saw: return 2 * p - 1;
                default: return Mathf.Sin(ph * 2 * Mathf.PI);
            }
        }
        float Noise() { return (float)(rng.NextDouble() * 2 - 1); }
        static AudioClip Clip(string n, float[] d, bool loop = false) { var c = AudioClip.Create(n, d.Length, 1, SR, false); c.SetData(d, 0); return c; }
        static void AddTone(float[] b, float t0, float dur, float f0, float f1, Wave w, float vol, float attack = .006f)
        {
            int s0 = (int)(t0 * SR), n = (int)(dur * SR); double ph = 0;
            for (int i = 0; i < n && s0 + i < b.Length; i++)
            {
                float t = (float)i / SR, k = t / dur;
                float f = f0 * Mathf.Pow(f1 / f0, k); ph += f / SR;
                float env = t < attack ? t / attack : Mathf.Exp(-(t - attack) / (dur * .28f));
                b[s0 + i] += Osc(w, (float)ph) * vol * env;
            }
        }
        AudioClip Tone(float dur, float f0, float f1, Wave w, float vol, float delay = 0, float attack = .006f)
        { var b = new float[(int)((dur + delay + .05f) * SR)]; AddTone(b, delay, dur, f0, f1, w, vol, attack); return Clip("tone", b); }
        AudioClip Arp(float[] fs, float step, float dur, Wave w, float vol)
        { var b = new float[(int)((fs.Length * step + dur + .05f) * SR)]; for (int i = 0; i < fs.Length; i++) AddTone(b, i * step, dur, fs[i], fs[i], w, vol); return Clip("arp", b); }
        static AudioClip Mix(AudioClip a, AudioClip b)
        {
            var da = new float[a.samples]; a.GetData(da, 0); var db = new float[b.samples]; b.GetData(db, 0);
            var o = new float[Mathf.Max(da.Length, db.Length)]; for (int i = 0; i < o.Length; i++) o[i] = (i < da.Length ? da[i] : 0) + (i < db.Length ? db[i] : 0);
            return Clip("mix", o);
        }
        AudioClip Whistle(float[] pattern)
        {
            float total = 0; foreach (var d in pattern) total += d + .13f;
            var b = new float[(int)((total + .1f) * SR)]; float t0 = .02f;
            foreach (var d in pattern)
            {
                int s0 = (int)(t0 * SR), n = (int)(d * SR); double ph = 0;
                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / SR; float f = 2780 + 170 * Mathf.Sin(2 * Mathf.PI * 36 * t); ph += f / SR;
                    float env = Mathf.Clamp01(t / .02f) * Mathf.Clamp01((d - t) / .04f);
                    b[s0 + i] += Mathf.Sin((float)ph * 2 * Mathf.PI) * .28f * env + Noise() * .015f * env;
                }
                t0 += d + .13f;
            }
            return Clip("whistle", b);
        }
        AudioClip NoiseSweep(float dur, float f0, float f1, float vol, bool fast)
        {
            var b = new float[(int)(dur * SR)]; float lp1 = 0, lp2 = 0;
            for (int i = 0; i < b.Length; i++)
            {
                float k = (float)i / b.Length, fc = Mathf.Lerp(f0, f1, k);
                float a1 = 1 - Mathf.Exp(-2 * Mathf.PI * fc * 1.6f / SR), a2 = 1 - Mathf.Exp(-2 * Mathf.PI * fc * .5f / SR);
                float n = Noise(); lp1 += a1 * (n - lp1); lp2 += a2 * (lp1 - lp2);
                float band = (lp1 - lp2) * 3f;
                float env = fast ? Mathf.Sin(k * Mathf.PI) : Mathf.Clamp01(k / .35f) * Mathf.Clamp01((1 - k) / .45f);
                b[i] = band * vol * env;
            }
            return Clip("sweep", b);
        }
        // Loop de público: ruido filtrado con oleadas (sin cortes en el loop)
        AudioClip Crowd(float dur)
        {
            int n = (int)(dur * SR), fadeN = SR / 2; var raw = new float[n + fadeN]; float lp = 0, lp2 = 0, hp = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                float x = Noise(); lp += .09f * (x - lp); lp2 += .012f * (lp - lp2); hp = lp - lp2; raw[i] = hp * 2.4f;
            }
            var b = new float[n];
            for (int i = 0; i < n; i++)
            {
                float v = raw[i]; if (i < fadeN) { float k = (float)i / fadeN; v = raw[i] * k + raw[n + i] * (1 - k); }
                float t = (float)i / SR; float swell = .75f + .25f * Mathf.Sin(2 * Mathf.PI * t / dur) + .1f * Mathf.Sin(2 * Mathf.PI * 3 * t / dur + 1);
                b[i] = v * swell * .6f;
            }
            return Clip("crowd", b);
        }
        // Pad de calma: frecuencias múltiplo de 1/8 Hz para que el loop de 8 s sea perfecto
        AudioClip Pad(float dur)
        {
            float[] fs = { 110f, 164.75f, 220f, 247f, 261.625f, 329.625f }; float[] vs = { .1f, .08f, .05f, .04f, .04f, .03f };
            int n = (int)(dur * SR); var b = new float[n];
            for (int k = 0; k < fs.Length; k++) for (int d = -1; d <= 1; d += 2)
                {
                    float f = fs[k] + d * .25f; Wave w = k < 2 ? Wave.Sine : Wave.Tri;
                    for (int i = 0; i < n; i++) { float t = (float)i / SR; b[i] += Osc(w, f * t) * vs[k]; }
                }
            float lp = 0; for (int i = 0; i < n; i++) { float t = (float)i / SR; lp += .05f * (b[i] - lp); b[i] = lp * (.75f + .25f * Mathf.Sin(2 * Mathf.PI * t / dur)) * 1.6f; }
            return Clip("pad", b);
        }
        // 2 compases de música electrónica simple para cada nivel
        AudioClip Beat(int level)
        {
            float bpm = level == 1 ? 100 : level == 2 ? 118 : 136; float sp = 60f / bpm / 4; int steps = 32;
            int n = (int)(steps * sp * SR); var b = new float[n];
            float[] bass = { 55, 55, 65.41f, 49 }; float[] arp = { 440, 523.25f, 659.25f, 783.99f, 659.25f, 523.25f };
            for (int s = 0; s < steps; s++)
            {
                int i16 = s % 16; int s0 = (int)(s * sp * SR);
                if (i16 % 4 == 0) { double ph = 0; for (int i = 0; i < (int)(.28f * SR) && s0 + i < n; i++) { float t = (float)i / SR; float f = 42 + 108 * Mathf.Exp(-t / .03f); ph += f / SR; b[s0 + i] += Mathf.Sin((float)ph * 2 * Mathf.PI) * .55f * Mathf.Exp(-t / .09f); } }
                if (i16 == 4 || i16 == 12) { float hpp = 0, prev = 0; for (int i = 0; i < (int)(.16f * SR) && s0 + i < n; i++) { float t = (float)i / SR; float x = Noise(); hpp = .8f * (hpp + x - prev); prev = x; b[s0 + i] += hpp * .22f * Mathf.Exp(-t / .045f); } }
                if (i16 % 2 == 1 || level >= 3) { float hpp = 0, prev = 0, v = i16 % 4 == 2 ? .07f : .045f; for (int i = 0; i < (int)(.04f * SR) && s0 + i < n; i++) { float t = (float)i / SR; float x = Noise(); hpp = .3f * (hpp + x - prev); prev = x; b[s0 + i] += hpp * v * 2 * Mathf.Exp(-t / .012f); } }
                if (i16 % 4 == 0 || i16 == 6 || i16 == 14)
                {
                    float f = bass[(s / 8) % 4]; float lp = 0; double ph = 0; int len = (int)(sp * 1.8f * SR);
                    for (int i = 0; i < len && s0 + i < n; i++) { float t = (float)i / SR; ph += f / SR; float x = Osc(Wave.Saw, (float)ph); lp += .05f * (x - lp); b[s0 + i] += lp * .3f * Mathf.Exp(-t / (sp * .8f)); }
                }
                if (level >= 2 && i16 % 2 == 0)
                {
                    float f = arp[(s / 2) % arp.Length] * (level >= 3 ? 1 : .5f); double ph = 0; int len = (int)(sp * 1.5f * SR);
                    for (int i = 0; i < len && s0 + i < n; i++) { float t = (float)i / SR; ph += f / SR; b[s0 + i] += Osc(Wave.Tri, (float)ph) * .06f * Mathf.Exp(-t / (sp * .5f)); }
                }
            }
            return Clip("beat" + level, b);
        }
    }
}
