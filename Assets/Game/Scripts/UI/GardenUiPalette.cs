using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public static class GardenUiPalette
    {
        // UI colors plus the 8x8 art palette documented in PLACEHOLDER_ART.md.
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
            LeafLight, LeafBright, SoilDeep, Soil, SoilLight, Water, Produce, Radish, Flower,
            new Color32(255, 113, 74, 255), new Color32(247, 181, 227, 255),
            new Color32(143, 46, 72, 255), new Color32(184, 59, 82, 255),
            new Color32(217, 77, 93, 255), new Color32(242, 107, 98, 255),
            new Color32(255, 138, 112, 255), new Color32(244, 81, 139, 255),
            new Color32(233, 60, 111, 255), new Color32(255, 178, 196, 255),
            new Color32(168, 76, 39, 255), new Color32(213, 93, 38, 255),
            new Color32(239, 116, 40, 255), new Color32(255, 143, 46, 255),
            new Color32(255, 170, 54, 255), new Color32(255, 195, 75, 255),
            new Color32(255, 219, 92, 255), new Color32(255, 240, 160, 255),
            new Color32(50, 101, 46, 255), new Color32(69, 136, 51, 255),
            new Color32(90, 174, 60, 255), new Color32(115, 201, 75, 255),
            new Color32(140, 222, 87, 255), new Color32(171, 233, 108, 255),
            new Color32(200, 245, 138, 255), new Color32(230, 255, 184, 255),
            new Color32(18, 106, 104, 255), new Color32(22, 141, 122, 255),
            new Color32(32, 174, 144, 255), new Color32(57, 211, 176, 255),
            new Color32(91, 203, 227, 255), new Color32(58, 157, 223, 255),
            new Color32(70, 120, 212, 255), new Color32(160, 216, 255, 255),
            new Color32(94, 57, 139, 255), new Color32(118, 76, 168, 255),
            new Color32(146, 92, 199, 255), new Color32(175, 108, 222, 255),
            new Color32(202, 131, 239, 255), new Color32(226, 160, 255, 255),
            new Color32(235, 117, 197, 255), new Color32(255, 255, 255, 255)
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
            return new Color32(best.r, best.g, best.b, 255);
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
