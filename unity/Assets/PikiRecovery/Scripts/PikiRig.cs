// =====================================================================
//  PIKI RECOVERY · Cámara 360°, entrada y puntero.
//  · PC: arrastrar con el mouse para mirar, clic para elegir.
//  · Celular: giroscopio o arrastrar con el dedo, tocar para elegir.
//  · VR (OpenXR / Meta Quest): la cabeza mueve la cámara; se apunta con
//    el control (gatillo) o con la mirada (mantener la vista 1,4 s).
//  Funciona con el Input System nuevo o con el Input Manager clásico.
// =====================================================================
using System.Collections.Generic;
using UnityEngine;
using UXR = UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Piki
{
    public static class PIn
    {
#if ENABLE_INPUT_SYSTEM
        public static bool MouseHeld { get { return Mouse.current != null && Mouse.current.leftButton.isPressed; } }
        public static bool MouseDown { get { return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame; } }
        public static bool MouseUp { get { return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame; } }
        public static bool HasMouse { get { return Mouse.current != null; } }
        public static Vector2 MousePos { get { return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero; } }
        public static bool TouchHeld { get { return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed; } }
        public static bool TouchDown { get { return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame; } }
        public static bool TouchUp { get { return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame; } }
        public static Vector2 TouchPos { get { return Touchscreen.current != null ? Touchscreen.current.primaryTouch.position.ReadValue() : Vector2.zero; } }
        public static bool Space { get { return Keyboard.current != null && Keyboard.current.spaceKey.isPressed; } }
        public static bool KeyM { get { return Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame; } }
        public static bool KeyR { get { return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame; } }
        public static bool KeyEnter { get { return Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame); } }
        public static Vector2 Arrows { get { if (Keyboard.current == null) return Vector2.zero; var k = Keyboard.current; return new Vector2((k.rightArrowKey.isPressed ? 1 : 0) - (k.leftArrowKey.isPressed ? 1 : 0), (k.upArrowKey.isPressed ? 1 : 0) - (k.downArrowKey.isPressed ? 1 : 0)); } }
        public static bool GyroAvailable { get { return AttitudeSensor.current != null; } }
        public static void EnableGyro() { if (AttitudeSensor.current != null) InputSystem.EnableDevice(AttitudeSensor.current); }
        public static Quaternion GyroRaw { get { return AttitudeSensor.current != null ? AttitudeSensor.current.attitude.ReadValue() : Quaternion.identity; } }
#else
        public static bool MouseHeld { get { return Input.mousePresent && Input.GetMouseButton(0) && Input.touchCount == 0; } }
        public static bool MouseDown { get { return Input.mousePresent && Input.GetMouseButtonDown(0) && Input.touchCount == 0; } }
        public static bool MouseUp { get { return Input.mousePresent && Input.GetMouseButtonUp(0) && Input.touchCount == 0; } }
        public static bool HasMouse { get { return Input.mousePresent; } }
        public static Vector2 MousePos { get { return Input.mousePosition; } }
        public static bool TouchHeld { get { return Input.touchCount > 0; } }
        public static bool TouchDown { get { return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began; } }
        public static bool TouchUp { get { return Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled); } }
        public static Vector2 TouchPos { get { return Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero; } }
        public static bool Space { get { return Input.GetKey(KeyCode.Space); } }
        public static bool KeyM { get { return Input.GetKeyDown(KeyCode.M); } }
        public static bool KeyR { get { return Input.GetKeyDown(KeyCode.R); } }
        public static bool KeyEnter { get { return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter); } }
        public static Vector2 Arrows { get { return new Vector2((Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0), (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0)); } }
        public static bool GyroAvailable { get { return SystemInfo.supportsGyroscope; } }
        public static void EnableGyro() { if (SystemInfo.supportsGyroscope) Input.gyro.enabled = true; }
        public static Quaternion GyroRaw { get { return Input.gyro.attitude; } }
#endif
    }

    public class PikiRig : MonoBehaviour
    {
        public static PikiRig I; public static Transform CamT;
        [SerializeField] Camera cam; [SerializeField] Transform head;
        public Camera Cam { get { return cam; } }
        public Transform Head { get { return head; } }
        public bool XR { get; private set; }
        public bool Holding { get; private set; }
        public bool HoldUsed { get; set; }
        public float FuseTime = 1.4f;
        public System.Action onMute, onRecenter;

        float yaw, pitch, gyroYawRef; bool gyro, gyroRefSet;
        bool pressing; float dragDist;
        PikiButton hovered; float fuse;
        [SerializeField] Transform reticle, reticleFill; [SerializeField] LineRenderer laser;
        readonly List<UXR.InputDevice> devs = new List<UXR.InputDevice>();
        bool trigPrev;

        public static PikiRig Create(Camera existing)
        {
            var rig = new GameObject("PikiRig").AddComponent<PikiRig>();
            var head = new GameObject("Head").transform; head.SetParent(rig.transform, false); head.localPosition = new Vector3(0, 1.6f, 0);
            Camera cam = existing;
            if (cam == null) { cam = new GameObject("Main Camera").AddComponent<Camera>(); cam.gameObject.AddComponent<AudioListener>(); }
            if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
            cam.gameObject.tag = "MainCamera";
            cam.transform.SetParent(head, false); cam.transform.localPosition = Vector3.zero; cam.transform.localRotation = Quaternion.identity;
            cam.nearClipPlane = .05f; cam.farClipPlane = 1600; cam.fieldOfView = 70;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.02f, .04f, .08f);
            rig.cam = cam; rig.head = head; CamT = cam.transform; I = rig;
            rig.BuildReticle();
            return rig;
        }

        void BuildReticle()
        {
            var c = UI.Canvas(Cam.transform, new Vector3(0, 0, 1.2f), .06f, .06f, default(Vector3), "Reticle");
            UI.Icon(c, Spr.Ring, 12, 12, 10, Pal.A(Pal.Teal, .9f));
            var fill = UI.Icon(c, Spr.Circle, 12, 12, 6, Pal.Teal);
            reticleFill = fill.transform; reticle = c;
            c.GetComponent<Canvas>().sortingOrder = 100;
            var lg = new GameObject("Laser"); lg.transform.SetParent(transform, false);
            laser = lg.AddComponent<LineRenderer>(); laser.sharedMaterial = Mat.Unlit(Pal.A(Pal.Teal, .7f)); laser.widthMultiplier = .006f; laser.positionCount = 2; laser.enabled = false; laser.useWorldSpace = true;
        }

        void Awake() { I = this; if (cam != null) CamT = cam.transform; }
        void Start()
        {
            if (!XRActive() && PIn.GyroAvailable && Application.isMobilePlatform) { PIn.EnableGyro(); gyro = true; }
            SetFloorOrigin();
            Application.onBeforeRender += OnBeforeRender;
        }
        void OnDestroy() { Application.onBeforeRender -= OnBeforeRender; }

        static bool XRActive() { return UXR.XRSettings.enabled && UXR.XRSettings.isDeviceActive; }
        void SetFloorOrigin()
        {
            var subs = new List<UXR.XRInputSubsystem>(); SubsystemManager.GetSubsystems(subs);
            foreach (var s in subs) s.TrySetTrackingOriginMode(UXR.TrackingOriginModeFlags.Floor);
        }

        /* Mirada hacia adelante (solo en Y) y posición de la cabeza en el mundo */
        public float Yaw { get { return Cam.transform.eulerAngles.y; } }
        public Vector3 HeadPos { get { return Cam.transform.position; } }

        public void ResetView() { yaw = 0; pitch = 0; gyroRefSet = false; if (!XR && Head != null) Head.localRotation = Quaternion.identity; }
        public void PlaceAt(Vector3 pos, float yawDeg = 0) { transform.position = pos; transform.rotation = Quaternion.Euler(0, yawDeg, 0); ResetView(); }

        void OnBeforeRender() { if (XR) ApplyHeadPose(); }
        void ApplyHeadPose()
        {
            var d = UXR.InputDevices.GetDeviceAtXRNode(UXR.XRNode.CenterEye); if (!d.isValid) d = UXR.InputDevices.GetDeviceAtXRNode(UXR.XRNode.Head);
            Vector3 p; Quaternion q;
            if (d.TryGetFeatureValue(UXR.CommonUsages.centerEyePosition, out p) || d.TryGetFeatureValue(UXR.CommonUsages.devicePosition, out p)) Head.localPosition = p;
            if (d.TryGetFeatureValue(UXR.CommonUsages.centerEyeRotation, out q) || d.TryGetFeatureValue(UXR.CommonUsages.deviceRotation, out q)) Head.localRotation = q;
            Cam.transform.localPosition = Vector3.zero; Cam.transform.localRotation = Quaternion.identity;
        }

        void Update()
        {
            bool xrNow = XRActive();
            if (xrNow != XR) { XR = xrNow; if (!XR) Head.localPosition = new Vector3(0, 1.6f, 0); SetFloorOrigin(); }
            if (PIn.KeyM && onMute != null) onMute();
            if (PIn.KeyR && onRecenter != null) onRecenter();

            // ---- mirar alrededor ----
            bool pointerHeld = PIn.TouchHeld || PIn.MouseHeld;
            Vector2 pos = PIn.TouchHeld || PIn.TouchUp ? PIn.TouchPos : PIn.MousePos;
            if (PIn.TouchDown || PIn.MouseDown) { pressing = true; dragDist = 0; lastPos = pos; }
            if (XR) ApplyHeadPose();
            else
            {
                if (pressing && pointerHeld)
                {
                    Vector2 d = pos - lastPos; dragDist += d.magnitude; lastPos = pos;
                    float k = 90f / Mathf.Max(400, Screen.height);
                    yaw -= d.x * k; if (!gyro) pitch = Mathf.Clamp(pitch + d.y * k, -80, 80);
                }
                Vector2 ar = PIn.Arrows; yaw += ar.x * 70 * Time.deltaTime; pitch = Mathf.Clamp(pitch - ar.y * 50 * Time.deltaTime, -80, 80);
                if (gyro)
                {
                    Quaternion g = PIn.GyroRaw; Quaternion att = Quaternion.Euler(90, 0, 0) * new Quaternion(g.x, g.y, -g.z, -g.w);
                    if (!gyroRefSet && att != Quaternion.identity) { gyroYawRef = att.eulerAngles.y; gyroRefSet = true; }
                    Head.localRotation = Quaternion.Euler(0, yaw - gyroYawRef, 0) * att;
                }
                else Head.localRotation = Quaternion.Euler(pitch, yaw, 0);
            }

            // ---- mantener presionado (respiración) ----
            bool trig = TriggerHeld();
            bool hold = PIn.Space || trig || (pointerHeld && pressing);
            if (hold && !Holding) HoldUsed = true;
            Holding = hold;

            // ---- puntero ----
            Ray ray; bool gazeMode = false; bool clickNow = false;
            Vector3 ctrlPos; Quaternion ctrlRot;
            if (XR && ControllerPose(out ctrlPos, out ctrlRot))
            {
                Vector3 o = transform.TransformPoint(ctrlPos); Vector3 dir = transform.rotation * (ctrlRot * Vector3.forward);
                ray = new Ray(o, dir); clickNow = trig && !trigPrev;
            }
            else if (XR || gyro && !pointerHeld && !PIn.TouchUp) { ray = new Ray(Cam.transform.position, Cam.transform.forward); gazeMode = XR; clickNow = trig && !trigPrev; }
            else { ray = Cam.ScreenPointToRay(pos); clickNow = (PIn.MouseUp || PIn.TouchUp) && pressing && dragDist < 14; }
            trigPrev = trig;
            if (PIn.MouseUp || PIn.TouchUp) pressing = false;

            PikiButton hit = null; RaycastHit rh; float hitDist = 6;
            if (Physics.Raycast(ray, out rh, 60f)) { hit = rh.collider.GetComponentInParent<PikiButton>(); hitDist = rh.distance; if (hit != null && hit.Disabled) hit = null; }
            if (hit != hovered) { if (hovered != null) hovered.SetHover(false); hovered = hit; if (hovered != null) hovered.SetHover(true); fuse = 0; }
            if (hovered != null && clickNow) { hovered.Click(); fuse = 0; }
            // Fusible por mirada (VR sin controles)
            if (gazeMode && hovered != null) { fuse += Time.deltaTime; if (fuse >= FuseTime) { hovered.Click(); fuse = -1.2f; } }
            else if (!gazeMode) fuse = 0;

            reticle.gameObject.SetActive(XR || gyro);
            float fk = gazeMode && hovered != null ? Mathf.Clamp01(fuse / FuseTime) : 0;
            reticleFill.localScale = Vector3.one * Mathf.Lerp(.35f, 1.6f, fk);
            laser.enabled = XR && !gazeMode;
            if (laser.enabled) { laser.SetPosition(0, ray.origin); laser.SetPosition(1, ray.origin + ray.direction * hitDist); }
        }
        Vector2 lastPos;

        bool TriggerHeld()
        {
            devs.Clear(); UXR.InputDevices.GetDevicesWithCharacteristics(UXR.InputDeviceCharacteristics.Controller, devs);
            foreach (var d in devs) { bool b; if ((d.TryGetFeatureValue(UXR.CommonUsages.triggerButton, out b) && b) || (d.TryGetFeatureValue(UXR.CommonUsages.gripButton, out b) && b)) return true; }
            return false;
        }
        bool ControllerPose(out Vector3 pos, out Quaternion rot)
        {
            pos = Vector3.zero; rot = Quaternion.identity;
            var d = UXR.InputDevices.GetDeviceAtXRNode(UXR.XRNode.RightHand); if (!d.isValid) d = UXR.InputDevices.GetDeviceAtXRNode(UXR.XRNode.LeftHand);
            if (!d.isValid) return false;
            bool tracked; if (d.TryGetFeatureValue(UXR.CommonUsages.isTracked, out tracked) && !tracked) return false;
            return d.TryGetFeatureValue(UXR.CommonUsages.devicePosition, out pos) && d.TryGetFeatureValue(UXR.CommonUsages.deviceRotation, out rot);
        }
        public void Haptic()
        {
            foreach (var n in new[] { UXR.XRNode.LeftHand, UXR.XRNode.RightHand }) { var d = UXR.InputDevices.GetDeviceAtXRNode(n); if (d.isValid) d.SendHapticImpulse(0, .3f, .06f); }
        }
    }
}
