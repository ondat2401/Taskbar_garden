using System;
using System.Runtime.InteropServices;
using Kirurobo;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IdleGarden
{
    public sealed class DesktopWindow : MonoBehaviour
    {
        public GardenProfile profile;
        public UniWindowController window;
        public Camera targetCamera;
        public bool locked;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private bool dragging;
        private bool resizing;
        private Vector2 startCursor, startPosition, startSize;
#endif
        public bool Topmost { get; private set; } = true;
        private Vector2 collapsedSize;
        private bool panelExpanded;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [StructLayout(LayoutKind.Sequential)] private struct Rect { public int left, top, right, bottom; }
        [DllImport("user32.dll")] private static extern bool SystemParametersInfo(uint action, uint param, out Rect area, uint update);
#endif
        private System.Collections.IEnumerator Start()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            // Let the plugin attach to the native window in its first Update.
            yield return null;
            window.SetCamera(targetCamera);
            window.isHitTestEnabled = true;
            window.hitTestType = UniWindowController.HitTestType.Opacity;
            window.isTopmost = Topmost;
            window.isTransparent = true;
            var size = profile.windowSize;
            window.windowSize = new Vector2(Mathf.Max(size.x, profile.minimumWindowSize.x), Mathf.Max(size.y, profile.minimumWindowSize.y));
            if (SystemParametersInfo(0x30, 0, out Rect area, 0))
                window.windowPosition = new Vector2(area.right - window.windowSize.x - 12, Display.main.systemHeight - area.bottom + 8);
#endif
            yield break;
        }

        public void BeginDrag() { Begin(false); }
        public void BeginResize() { Begin(true); }
        private void Begin(bool resize)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (locked) return;
            startCursor = UniWindowController.GetCursorPosition();
            startPosition = window.windowPosition; startSize = window.windowSize;
            resizing = resize; dragging = !resize;
            window.isHitTestEnabled = false; window.isClickThrough = false;
#endif
        }

        private void Update()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!dragging && !resizing) return;
            if (Mouse.current == null || !Mouse.current.leftButton.isPressed) { EndInteraction(); return; }
            Vector2 delta = UniWindowController.GetCursorPosition() - startCursor;
            if (dragging) window.windowPosition = startPosition + delta;
            else
            {
                Vector2 size = new Vector2(Mathf.Max(profile.minimumWindowSize.x, startSize.x + delta.x), Mathf.Max(profile.minimumWindowSize.y, startSize.y - delta.y));
                window.windowSize = size;
                window.windowPosition = new Vector2(startPosition.x, startPosition.y + startSize.y - size.y);
            }
#endif
        }

        public void EndInteraction()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            dragging = resizing = false;
            window.isHitTestEnabled = true;
#endif
        }
        public void ToggleTopmost()
        {
            Topmost = !Topmost;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            window.isTopmost = Topmost;
#endif
        }
        public void ToggleLock() { locked = !locked; EndInteraction(); }
        public void SetPanelExpanded(bool expanded)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (panelExpanded == expanded) return;
            if (expanded)
            {
                collapsedSize = window.windowSize;
                window.windowSize = new Vector2(Mathf.Max(620, collapsedSize.x), Mathf.Max(540, collapsedSize.y));
            }
            else window.windowSize = collapsedSize;
#endif
            panelExpanded = expanded;
        }
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
