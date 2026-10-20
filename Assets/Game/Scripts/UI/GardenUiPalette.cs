using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public static class GardenUiPalette
    {
        // Shared by HUD styling and the pixel-art placeholder palette.
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
        public static readonly Color LeafLight = new Color32(124, 160, 94, 255);
        public static readonly Color LeafBright = new Color32(171, 193, 122, 255);
        public static readonly Color SoilDeep = new Color32(69, 54, 46, 255);
        public static readonly Color Soil = new Color32(117, 81, 57, 255);
        public static readonly Color SoilLight = new Color32(160, 123, 79, 255);
        public static readonly Color Water = new Color32(112, 152, 160, 255);
        public static readonly Color Produce = new Color32(205, 125, 61, 255);
        public static readonly Color Radish = new Color32(190, 108, 122, 255);
        public static readonly Color Flower = new Color32(220, 208, 176, 255);
        public static readonly Color32[] SharedColors =
        {
            Panel, Header, Surface, Row, Button, ButtonHover, ButtonPressed,
            ButtonSelected, Disabled, Border, TextPrimary, TextMuted, Accent,
            LeafLight, LeafBright, SoilDeep, Soil, SoilLight, Water, Produce, Radish, Flower
        };

        public static Color32 SnapToSharedColor(Color32 color)
        {
            int bestDistance = int.MaxValue;
            Color32 best = SharedColors[0];
            foreach (Color32 candidate in SharedColors)
            {
                int red = color.r - candidate.r;
                int green = color.g - candidate.g;
                int blue = color.b - candidate.b;
                int distance = red * red + green * green + blue * blue;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = candidate;
            }
            return best;
        }

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
