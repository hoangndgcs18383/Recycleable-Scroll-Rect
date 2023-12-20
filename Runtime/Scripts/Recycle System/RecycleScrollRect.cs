using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hzeff.UI
{
    public class RecycleScrollRect : ScrollRect
    {
        public IRecycleScrollProvider Provider;

        public bool IsGrid;
        public RectTransform PrefabItem;
        public bool SelfInitialize = true;

        public enum DirectionType
        {
            Vertical,
            Horizontal
        }

        public DirectionType Direction;

        public int Segments
        {
            set { _segments = Math.Max(value, 2); }
            get { return _segments; }
        }

        [SerializeField] private int _segments;

        private RecycleScrolling _recyclingSystem;
        private Vector2 _prevAnchoredPos;

        protected override void Start()
        {
            vertical = true;
            horizontal = false;

            if (!Application.isPlaying) return;

            if (SelfInitialize) Initialize();
        }

        private void Initialize()
        {
            if (Direction == DirectionType.Vertical)
            {
                _recyclingSystem = new VerticalScrolling(PrefabItem, viewport, content, Provider, IsGrid, Segments);
            }
            else if (Direction == DirectionType.Horizontal)
            {
                _recyclingSystem = new HorizontalScrolling(PrefabItem, viewport, content, Provider, IsGrid, Segments);
            }

            vertical = Direction == DirectionType.Vertical;
            horizontal = Direction == DirectionType.Horizontal;

            _prevAnchoredPos = content.anchoredPosition;
            onValueChanged.RemoveListener(OnValueChangedListener);
            StartCoroutine(_recyclingSystem.IEInitialize(() =>
                onValueChanged.AddListener(OnValueChangedListener)
            ));
        }

        public void Initialize(IRecycleScrollProvider provider)
        {
            Provider = provider;
            Initialize();
        }

        public void OnValueChangedListener(Vector2 normalizedPos)
        {
            Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
            m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(dir);
            _prevAnchoredPos = content.anchoredPosition;
        }

        public void ReloadData()
        {
            ReloadData(Provider);
        }

        public void ReloadData(IRecycleScrollProvider provider)
        {
            if (_recyclingSystem != null)
            {
                StopMovement();
                onValueChanged.RemoveListener(OnValueChangedListener);
                _recyclingSystem.Provider = provider;
                StartCoroutine(_recyclingSystem.IEInitialize(() =>
                    onValueChanged.AddListener(OnValueChangedListener)
                ));
                _prevAnchoredPos = content.anchoredPosition;
            }
        }
    }
}