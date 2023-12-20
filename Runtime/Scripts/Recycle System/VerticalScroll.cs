using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hzeff.UI
{
    public class VerticalScrolling : RecycleScrolling
    {
        private int _columns;
        private float _cellHeight, _cellWidth;

        private List<RectTransform> _poolItem;
        private List<IScrollData> _cachedData;
        private Bounds _recyclableViewBounds;

        private readonly Vector3[] _corners = new Vector3[4];
        private bool _isRecycling;

        private int _currentItemCount;
        private int _topMostCellIndex, _bottomMostCellIndex;
        private int _topMostCellColumn, _bottomMostCellColumn;

        private Vector2 _zeroVector = Vector2.zero;

        public VerticalScrolling(RectTransform item, RectTransform viewport, RectTransform content,
            IRecycleScrollProvider provider, bool isGrid, int _coloumns)
        {
            Item = item;
            Viewport = viewport;
            Content = content;
            Provider = provider;
            IsGrid = isGrid;
            _columns = isGrid ? _coloumns : 1;
            _recyclableViewBounds = new Bounds();
        }

        public override IEnumerator IEInitialize(Action onInitialized = null)
        {
            SetTopAnchor(Item);
            Content.anchoredPosition = Vector3.zero;
            yield return null;
            SetRecyclingBounds();

            CreatePool();

            _currentItemCount = _poolItem.Count;
            _topMostCellIndex = 0;
            _bottomMostCellIndex = _poolItem.Count - 1;

            int noOfRows = (int)Mathf.Ceil((float)_poolItem.Count / (float)_columns);
            float contentYSize = noOfRows * _cellHeight;
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, contentYSize);
            SetTopAnchor(Content);

            onInitialized?.Invoke();
        }

        public override Vector2 OnValueChangedListener(Vector2 direction)
        {
            if (_isRecycling || _poolItem == null || _poolItem.Count == 0) return _zeroVector;

            SetRecyclingBounds();

            if (direction.y > 0 && _poolItem[_bottomMostCellIndex].MaxY() > _recyclableViewBounds.min.y)
            {
                return RecycleTopToBottom();
            }
            else if (direction.y < 0 && _poolItem[_topMostCellIndex].MinY() < _recyclableViewBounds.max.y)
            {
                return RecycleBottomToTop();
            }

            return _zeroVector;
        }

        private Vector2 RecycleTopToBottom()
        {
            _isRecycling = true;

            int n = 0;
            float posY = IsGrid ? _poolItem[_bottomMostCellIndex].anchoredPosition.y : 0;
            float posX = 0;

            //to determine if content size needs to be updated
            int additionalRows = 0;
            //Recycle until cell at Top is avaiable and current item count smaller than datasource
            while (_poolItem[_topMostCellIndex].MinY() > _recyclableViewBounds.max.y && _currentItemCount < Provider.GetItemCount())
            {
                if (IsGrid)
                {
                    if (++_bottomMostCellColumn >= _columns)
                    {
                        n++;
                        _bottomMostCellColumn = 0;
                        posY = _poolItem[_bottomMostCellIndex].anchoredPosition.y - _cellHeight;
                        additionalRows++;
                    }

                    //Move top cell to bottom
                    posX = _bottomMostCellColumn * _cellWidth;
                    _poolItem[_topMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (++_topMostCellColumn >= _columns)
                    {
                        _topMostCellColumn = 0;
                        additionalRows--;
                    }
                }
                else
                {
                    //Move top cell to bottom
                    posY = _poolItem[_bottomMostCellIndex].anchoredPosition.y - _poolItem[_bottomMostCellIndex].sizeDelta.y;
                    _poolItem[_topMostCellIndex].anchoredPosition = new Vector2(_poolItem[_topMostCellIndex].anchoredPosition.x, posY);
                }

                //Cell for row at
                Provider.SetData(_cachedData[_topMostCellIndex], _currentItemCount);

                //set new indices
                _bottomMostCellIndex = _topMostCellIndex;
                _topMostCellIndex = (_topMostCellIndex + 1) % _poolItem.Count;

                _currentItemCount++;
                if (!IsGrid) n++;
            }

            //Content size adjustment 
            if (IsGrid)
            {
                Content.sizeDelta += additionalRows * Vector2.up * _cellHeight;
                //TODO : check if it is supposed to be done only when > 0
                if (additionalRows > 0)
                {
                    n -= additionalRows;
                }
            }

            //Content anchor position adjustment.
            _poolItem.ForEach((RectTransform cell) => cell.anchoredPosition += n * Vector2.up * _poolItem[_topMostCellIndex].sizeDelta.y);
            Content.anchoredPosition -= n * Vector2.up * _poolItem[_topMostCellIndex].sizeDelta.y;
            _isRecycling = false;
            return -new Vector2(0, n * _poolItem[_topMostCellIndex].sizeDelta.y);
        }

        private Vector2 RecycleBottomToTop()
        {
            _isRecycling = true;

            int n = 0;
            float posY = IsGrid ? _poolItem[_topMostCellIndex].anchoredPosition.y : 0;
            float posX = 0;

            int additionalRows = 0;
            while (_poolItem[_bottomMostCellIndex].MaxY() < _recyclableViewBounds.min.y &&
                   _currentItemCount > _poolItem.Count)
            {
                if (IsGrid)
                {
                    if (--_topMostCellColumn < 0)
                    {
                        n++;
                        _topMostCellColumn = _columns - 1;
                        posY = _poolItem[_topMostCellIndex].anchoredPosition.y + _cellHeight;
                        additionalRows++;
                    }

                    posX = _topMostCellColumn * _cellWidth;
                    _poolItem[_bottomMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (--_bottomMostCellColumn < 0)
                    {
                        _bottomMostCellColumn = _columns - 1;
                        additionalRows--;
                    }
                }
                else
                {
                    posY = _poolItem[_topMostCellIndex].anchoredPosition.y + _poolItem[_topMostCellIndex].sizeDelta.y;
                    _poolItem[_bottomMostCellIndex].anchoredPosition =
                        new Vector2(_poolItem[_bottomMostCellIndex].anchoredPosition.x, posY);
                    n++;
                }

                _currentItemCount--;
                Provider.SetData(_cachedData[_bottomMostCellIndex], _currentItemCount - _poolItem.Count);

                _topMostCellIndex = _bottomMostCellIndex;
                _bottomMostCellIndex = (_bottomMostCellIndex - 1 + _poolItem.Count) % _poolItem.Count;
            }

            if (IsGrid)
            {
                Content.sizeDelta += additionalRows * Vector2.up * _cellHeight;
                if (additionalRows > 0)
                {
                    n -= additionalRows;
                }
            }

            _poolItem.ForEach((cell) =>
                cell.anchoredPosition -= n * Vector2.up * _poolItem[_topMostCellIndex].sizeDelta.y);
            Content.anchoredPosition += n * Vector2.up * _poolItem[_topMostCellIndex].sizeDelta.y;
            _isRecycling = false;
            return new Vector2(0, n * _poolItem[_topMostCellIndex].sizeDelta.y);
        }

        private void CreatePool()
        {
            if (_poolItem != null)
            {
                _poolItem.ForEach((item) => Object.Destroy(item.gameObject));
                _poolItem.Clear();
                _cachedData.Clear();
            }
            else
            {
                _cachedData = new List<IScrollData>();
                _poolItem = new List<RectTransform>();
            }

            Item.gameObject.SetActive(true);

            if (IsGrid)
            {
                SetTopLeftAnchor(Item);
            }
            else
            {
                SetTopAnchor(Item);
            }

            ResetMostItemIndex();

            float currentPoolCoverage = 0;
            int poolSize = 0;
            float posX = 0;
            float posY = 0;

            _cellWidth = Content.rect.width / _columns;
            var sizeDelta = Item.sizeDelta;
            _cellHeight = sizeDelta.y / sizeDelta.x * _cellWidth;

            float requiredCoverage = MinPoolCoverage * Viewport.rect.height;
            int minPoolSize = Math.Min(MinPoolSize, Provider.GetItemCount());

            while ((poolSize < minPoolSize || currentPoolCoverage < requiredCoverage) &&
                   poolSize < Provider.GetItemCount())
            {
                RectTransform item = (UnityEngine.Object.Instantiate(Item.gameObject)).GetComponent<RectTransform>();
                item.sizeDelta = new Vector2(_cellWidth, _cellHeight);
                _poolItem.Add(item);
                item.SetParent(Content, false);

                if (IsGrid)
                {
                    posX = _bottomMostCellColumn * _cellWidth;
                    item.anchoredPosition = new Vector2(posX, posY);
                    if (++_bottomMostCellColumn >= _columns)
                    {
                        _bottomMostCellColumn = 0;
                        posY -= _cellHeight;
                        currentPoolCoverage += item.rect.height;
                    }
                }
                else
                {
                    item.anchoredPosition = new Vector2(0, posY);
                    posY = item.anchoredPosition.y - item.rect.height;
                    currentPoolCoverage += item.rect.height;
                }

                _cachedData.Add(item.GetComponent<IScrollData>());
                Provider.SetData(_cachedData[^1], poolSize);

                //Update the Pool size
                poolSize++;
            }

            if (IsGrid)
            {
                _bottomMostCellColumn = (_bottomMostCellColumn - 1 + _columns) % _columns;
            }

            if (Item.gameObject.scene.IsValid())
            {
                Item.gameObject.SetActive(false);
            }
        }

        private void ResetMostItemIndex()
        {
            _topMostCellIndex = _bottomMostCellIndex = 0;
        }

        private void SetRecyclingBounds()
        {
            Viewport.GetWorldCorners(_corners);
            float threshHold = RecyclingThreshold * (_corners[2].y - _corners[0].y);
            _recyclableViewBounds.min = new Vector3(_corners[0].x, _corners[0].y - threshHold);
            _recyclableViewBounds.max = new Vector3(_corners[2].x, _corners[2].y + threshHold);
        }

        private void SetTopAnchor(RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height;

            Vector2 pos = new Vector2(0, 0.5f);

            rectTransform.anchorMin = pos;
            rectTransform.anchorMax = pos;
            rectTransform.pivot = pos;

            rectTransform.sizeDelta = new Vector2(width, height);
        }

        private void SetTopLeftAnchor(RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height;

            Vector2 pos = new Vector2(0, 1f);

            rectTransform.anchorMin = pos;
            rectTransform.anchorMax = pos;
            rectTransform.pivot = pos;

            rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}