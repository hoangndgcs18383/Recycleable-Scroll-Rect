using System;
using System.Collections;
using UnityEngine;

namespace Hzeff.UI
{
    public abstract class RecycleScrolling
    {
        public IRecycleScrollProvider Provider;

        protected RectTransform Viewport, Content;
        protected RectTransform Item;

        protected bool IsGrid;

        protected float MinPoolCoverage = 1.5f; // The recyclable pool must cover (viewPort * _poolCoverage) area.
        protected int MinPoolSize = 10; // Cell pool must have a min size
        protected float RecyclingThreshold = .2f; //Threshold for recycling above and below viewport

        public abstract IEnumerator IEInitialize(Action onInitialized = null);
        public abstract Vector2 OnValueChangedListener(Vector2 direction);
    }
}