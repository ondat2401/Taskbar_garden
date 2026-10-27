using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleGarden
{
    public sealed class PlotView : MonoBehaviour, IPointerClickHandler
    {
        public SpriteRenderer cropRenderer;
        public Transform growthFill;
        public Transform waterFill;
        public SpriteRenderer soilRenderer;
        public GardenController garden;
        public int plotIndex;
        private Color potCosmeticTint = Color.white;
        private Color cropCosmeticTint = Color.white;

        public void SetCosmeticTints(Color potTint, Color cropTint)
        {
            potCosmeticTint = potTint;
            cropCosmeticTint = cropTint;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button==PointerEventData.InputButton.Left && garden && garden.hud) garden.hud.OpenPot(plotIndex);
        }

        public void Render(PlotState state, float animationTime)
        {
            var stages = state.Crop.stages;
            cropRenderer.enabled = state.Planted && stages != null && stages.Length > 0;
            if (cropRenderer.enabled)
            {
                int stage = Mathf.Min(stages.Length - 1, Mathf.FloorToInt(state.Growth * stages.Length));
                var sway = state.Crop.swayStages;
                bool alternate = sway != null && sway.Length == stages.Length && sway[stage] && (Mathf.FloorToInt(animationTime * state.Crop.animationFps) & 1) != 0;
                cropRenderer.sprite = alternate ? sway[stage] : stages[stage];
            }
            cropRenderer.color = (state.Crop.theme ? state.Crop.theme.cropTint : Color.white) * cropCosmeticTint;
            growthFill.localScale = new Vector3(Mathf.Max(.001f, state.Growth), 1, 1);
            waterFill.localScale = new Vector3(Mathf.Max(.001f, Mathf.Clamp01(state.Water / state.Crop.waterCapacity)), 1, 1);
            soilRenderer.color = Color.Lerp(new Color(.62f, .42f, .32f), Color.white, Mathf.Clamp01(state.Water / state.Crop.waterCapacity)) * potCosmeticTint;
        }
    }
}
