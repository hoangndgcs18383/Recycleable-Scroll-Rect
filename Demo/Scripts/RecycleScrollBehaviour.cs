using System;
using System.Collections.Generic;
using Hzeff.Demo;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hzeff.UI
{
    public class RecycleScrollBehaviour : MonoBehaviour, IRecycleScrollProvider
    {
        [SerializeField] private RecycleScrollRect recyclableScrollRect;
        [SerializeField] private int itemCount;

        private List<ScrollData> _scrollDatas = new List<ScrollData>();
        private ScrollDemoItem _scrollDemoItem;

        private void Start()
        {
            Initialize();

            recyclableScrollRect.Provider = this;
        }

        private void Initialize()
        {
            _scrollDatas.Clear();

            for (int i = 0; i < itemCount; i++)
            {
                _scrollDatas.Add(new ScrollData
                {
                    Color = Random.ColorHSV(),
                    Text = $"Item {i}"
                });
            }
        }

        public int GetItemCount()
        {
            return _scrollDatas.Count;
        }

        public void SetData(IScrollData data, int index)
        {
            _scrollDemoItem = (ScrollDemoItem)data;
            _scrollDemoItem.SetData(_scrollDatas[index], index);
        }
    }
}