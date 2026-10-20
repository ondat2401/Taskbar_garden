using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public static class GardenUiPalette
    {
        public static readonly Color Panel = new Color32(23, 36, 30, 248);
        public static readonly Color Header = new Color32(53, 80, 58, 255);
        public static readonly Color Surface = new Color32(28, 45, 35, 255);
        public static readonly Color Row = new Color32(36, 59, 44, 255);
        public static readonly Color Button = new Color32(57, 90, 66, 255);
        public static readonly Color ButtonHover = new Color32(86, 120, 80, 255);
        public static readonly Color ButtonPressed = new Color32(41, 63, 48, 255);
        public static readonly Color ButtonSelected = new Color32(70, 105, 70, 255);
        public static readonly Color Disabled = new Color32(58, 69, 59, 255);
        public static readonly Color Border = new Color32(16, 26, 21, 255);
        public static readonly Color TextPrimary = new Color32(243, 233, 201, 255);
        public static readonly Color TextMuted = new Color32(184, 197, 163, 255);
        public static readonly Color Accent = new Color32(229, 184, 92, 255);

        public static void StyleButton(Button button, bool selected = false)
        {
            if (button.targetGraphic) button.targetGraphic.color = Color.white;
            var colors = button.colors;
            colors.normalColor = selected ? ButtonSelected : Button;
            colors.highlightedColor = ButtonHover;
            colors.pressedColor = ButtonPressed;
            colors.selectedColor = ButtonSelected;
            colors.disabledColor = Disabled;
            colors.colorMultiplier = 1;
            colors.fadeDuration = .08f;
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;
        }

        public static void AddBorder(Image image)
        {
            var outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = Border;
            outline.effectDistance = new Vector2(1, -1);
            outline.useGraphicAlpha = false;
        }
    }
}
