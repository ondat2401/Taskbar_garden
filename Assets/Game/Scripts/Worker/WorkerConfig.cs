using UnityEngine;

namespace IdleGarden
{
    [CreateAssetMenu(menuName = "Idle Garden/Worker")]
    public sealed class WorkerConfig : ScriptableObject
    {
        [Min(.1f)] public float moveSpeed = 2;
        [Min(.05f)] public float plantSeconds = .7f;
        [Min(.05f)] public float waterSeconds = .7f;
        [Min(.05f)] public float harvestSeconds = .8f;
        [Min(.05f)] public float refillSeconds = 1;
        [Min(1)] public float tankCapacity = 24;
        [Min(.1f)] public float waterPerAction = 10;
        public Sprite[] idleFrames;
        public Sprite[] walkFrames;
        public Sprite[] workFrames;
        [Range(1, 16)] public float animationFps = 5;
    }
}

