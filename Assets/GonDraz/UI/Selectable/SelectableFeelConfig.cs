using System;
using Ami.BroAudio;
using PrimeTween;
using UnityEngine;

namespace GonDraz.UI.Selectable
{
    [CreateAssetMenu(fileName = "SelectableFeelConfig", menuName = "GonDraz/Selectable Feel Config", order = 0)]
    public class SelectableFeelConfig : ScriptableObject
    {
        [SerializeField] public SoundID buttonClickSound;
        public Effect pointerDown;
        public Effect pointerUp;

        [Serializable]
        public class Effect
        {
            [Range(0f, 1f)] public float duration = 0.125f;

            public Vector3 rotation = new(0f, 0f, 0f);
            public Vector3 scale = new(1f, 1f, 1f);

            [Range(0f, 1f)] public float alpha = 1f;
            public Ease ease = Ease.Default;
        }
    }
}