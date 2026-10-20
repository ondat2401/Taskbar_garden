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
        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button==PointerEventData.InputButton.Left && garden && garden.hud) garden.hud.OpenPot(plotIndex);
        }

        public void Render(PlotState state)
        {
            var stages = state.Crop.stages;
            cropRenderer.enabled = state.Planted && stages != null && stages.Length > 0;
            if (cropRenderer.enabled)
                cropRenderer.sprite = stages[Mathf.Min(stages.Length - 1, Mathf.FloorToInt(state.Growth * stages.Length))];
            cropRenderer.color = state.Crop.theme ? state.Crop.theme.cropTint : Color.white;
            growthFill.localScale = new Vector3(Mathf.Max(.001f, state.Growth), 1, 1);
            waterFill.localScale = new Vector3(Mathf.Max(.001f, Mathf.Clamp01(state.Water / state.Crop.waterCapacity)), 1, 1);
            soilRenderer.color = Color.Lerp(new Color(.62f, .42f, .32f), Color.white, Mathf.Clamp01(state.Water / state.Crop.waterCapacity));
        }
    }
}
