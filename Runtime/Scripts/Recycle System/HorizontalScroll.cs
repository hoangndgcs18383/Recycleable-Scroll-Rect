using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hzeff.UI
{
    public class HorizontalScrolling : RecycleScrolling
    {
        private int _rows;
        private float _cellHeight, _cellWidth;

        private List<RectTransform> _poolItem;
        private List<IScrollData> _cachedData;
        private Bounds _recyclableViewBounds;

        private readonly Vector3[] _corners = new Vector3[4];
        private bool _isRecycling;

        private int _currentItemCount;
        private int _leftMostCellIndex, _rightMostCellIndex;
        private int _leftMostCellRow, _rightMostCellRow;

        private Vector2 _zeroVector = Vector2.zero;

        public HorizontalScrolling(RectTransform item, RectTransform viewport, RectTransform content,
            IRecycleScrollProvider provider, bool isGrid, int rows)
        {
            Item = item;
            Viewport = viewport;
            Content = content;
            Provider = provider;
            IsGrid = isGrid;
            _rows = isGrid ? rows : 1;
            _recyclableViewBounds = new Bounds();
        }

        public override IEnumerator IEInitialize(Action onInitialized = null)
        {
            SetLeftAnchor(Item);
            Content.anchoredPosition = Vector3.zero;
            yield return null;
            SetRecyclingBounds();

            CreatePool();

            _currentItemCount = _poolItem.Count;
            _leftMostCellIndex = 0;
            _rightMostCellIndex = _poolItem.Count - 1;
            
            int columns = Mathf.CeilToInt((float)_poolItem.Count / _rows);
            float contentXSize = columns * _cellWidth;
            Content.sizeDelta = new Vector2(contentXSize, Content.sizeDelta.y);
            SetLeftAnchor(Content);

            onInitialized?.Invoke();
        }

        public override Vector2 OnValueChangedListener(Vector2 direction)
        {
            if (_isRecycling || _poolItem == null || _poolItem.Count == 0) return _zeroVector;
            
            SetRecyclingBounds();

            if (direction.x < 0 && _poolItem[_rightMostCellIndex].MinX() < _recyclableViewBounds.max.x)
            {
                return RecycleLeftToRight();
            }
            else if (direction.x > 0 && _poolItem[_leftMostCellIndex].MaxX() > _recyclableViewBounds.min.x)
            {
                return RecycleRightToLeft();
            }

            return _zeroVector;
        }

        private Vector2 RecycleLeftToRight()
        {
            _isRecycling = true;

            int n = 0;
            float posX = IsGrid ? _poolItem[_rightMostCellIndex].anchoredPosition.x : 0;
            float posY = 0;


            int additionaColumns = 0;

            while (_poolItem[_leftMostCellIndex].MaxX() < _recyclableViewBounds.min.x &&
                   _currentItemCount < Provider.GetItemCount())
            {
                if (IsGrid)
                {
                    if (++_rightMostCellRow >= _rows)
                    {
                        n++;
                        _rightMostCellRow = 0;
                        posX = _poolItem[_rightMostCellIndex].anchoredPosition.x + _cellWidth;
                        additionaColumns++;
                    }
                    
                    posY = -_rightMostCellRow * _cellHeight;
                    _poolItem[_leftMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (++_leftMostCellRow >= _rows)
                    {
                        _leftMostCellRow = 0;
                        additionaColumns--;
                    }
                }
                else
                {
                    posX = _poolItem[_rightMostCellIndex].anchoredPosition.x +
                           _poolItem[_rightMostCellIndex].sizeDelta.x;
                    _poolItem[_leftMostCellIndex].anchoredPosition =
                        new Vector2(posX, _poolItem[_leftMostCellIndex].anchoredPosition.y);
                }
                
                Provider.SetData(_cachedData[_leftMostCellIndex], _currentItemCount);
                
                _rightMostCellIndex = _leftMostCellIndex;
                _leftMostCellIndex = (_leftMostCellIndex + 1) % _poolItem.Count;

                _currentItemCount++;
                if (!IsGrid) n++;
            }
            
            if (IsGrid)
            {
                Content.sizeDelta += additionaColumns * Vector2.right * _cellWidth;
                if (additionaColumns > 0)
                {
                    n -= additionaColumns;
                }
            }
            
            _poolItem.ForEach((cell) =>
                cell.anchoredPosition -= n * Vector2.right * _poolItem[_leftMostCellIndex].sizeDelta.x);
            Content.anchoredPosition += n * Vector2.right * _poolItem[_leftMostCellIndex].sizeDelta.x;
            _isRecycling = false;

            return n * Vector2.right * _poolItem[_leftMostCellIndex].sizeDelta.x;
        }

        private Vector2 RecycleRightToLeft()
        {
            _isRecycling = true;

            int n = 0;
            float posX = IsGrid ? _poolItem[_leftMostCellIndex].anchoredPosition.x : 0;
            float posY = 0;
            
            int additionalColoums = 0;
            while (_poolItem[_rightMostCellIndex].MinX() > _recyclableViewBounds.max.x && _currentItemCount > _poolItem.Count)
            {
                if (IsGrid)
                {
                    if (--_leftMostCellRow < 0)
                    {
                        n++;
                        _leftMostCellRow = _rows - 1;
                        posX = _poolItem[_leftMostCellIndex].anchoredPosition.x - _cellWidth;
                        additionalColoums++;
                    }
                    
                    posY = -_leftMostCellRow * _cellHeight;
                    _poolItem[_rightMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (--_rightMostCellRow < 0)
                    {
                        _rightMostCellRow = _rows - 1;
                        additionalColoums--;
                    }
                }
                else
                {
                    posX = _poolItem[_leftMostCellIndex].anchoredPosition.x - _poolItem[_leftMostCellIndex].sizeDelta.x;
                    _poolItem[_rightMostCellIndex].anchoredPosition = new Vector2(posX, _poolItem[_rightMostCellIndex].anchoredPosition.y);
                    n++;
                }

                _currentItemCount--;
                
                Provider.SetData(_cachedData[_rightMostCellIndex], _currentItemCount - _poolItem.Count);
                
                _leftMostCellIndex = _rightMostCellIndex;
                _rightMostCellIndex = (_rightMostCellIndex - 1 + _poolItem.Count) % _poolItem.Count;
            }
            
            if (IsGrid)
            {
                Content.sizeDelta += additionalColoums * Vector2.right * _cellWidth;
                if (additionalColoums > 0)
                {
                    n -= additionalColoums;
                }
            }
            
            _poolItem.ForEach((cell) => cell.anchoredPosition += n * Vector2.right * _poolItem[_leftMostCellIndex].sizeDelta.x);
            Content.anchoredPosition -= n * Vector2.right * _poolItem[_leftMostCellIndex].sizeDelta.x;
            _isRecycling = false;
            return -n * Vector2.right * _poolItem[_leftMostCellIndex].sizeDelta.x;
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
            SetLeftAnchor(Item);

            _cellHeight = Content.rect.height / _rows;
            var sizeDelta = Item.sizeDelta;
            _cellWidth = sizeDelta.x / sizeDelta.y * _cellHeight;

            ResetMostItemIndex();

            float currentPoolCoverage = 0;
            int poolSize = 0;
            float posX = 0;
            float posY = 0;

            float requiredCoverage = MinPoolCoverage * Viewport.rect.width;
            int minPoolSize = Math.Min(MinPoolSize, Provider.GetItemCount());

            while ((poolSize < minPoolSize || currentPoolCoverage < requiredCoverage) &&
                   poolSize < Provider.GetItemCount())
            {
                RectTransform item = Object.Instantiate(Item.gameObject).GetComponent<RectTransform>();
                item.sizeDelta = new Vector2(_cellWidth, _cellHeight);
                _poolItem.Add(item);
                item.SetParent(Content, false);

                if (IsGrid)
                {
                    posY = -_rightMostCellRow * _cellHeight;
                    item.anchoredPosition = new Vector2(posX, posY);
                    if (++_rightMostCellRow >= _rows)
                    {
                        _rightMostCellRow = 0;
                        posX += _cellWidth;
                        currentPoolCoverage += item.rect.width;
                    }
                }
                else
                {
                    item.anchoredPosition = new Vector2(posX, 0);
                    posX = item.anchoredPosition.x + item.rect.width;
                    currentPoolCoverage += item.rect.width;
                }

                _cachedData.Add(item.GetComponent<IScrollData>());
                Provider.SetData(_cachedData[^1], poolSize);
                
                poolSize++;
            }

            if (IsGrid)
            {
                _rightMostCellRow = (_rightMostCellRow - 1 + _rows) % _rows;
            }

            if (Item.gameObject.scene.IsValid())
            {
                Item.gameObject.SetActive(false);
            }
        }

        private void ResetMostItemIndex()
        {
            _leftMostCellIndex = _rightMostCellIndex = 0;
        }

        private void SetRecyclingBounds()
        {
            Viewport.GetWorldCorners(_corners);
            float threshHold = RecyclingThreshold * (_corners[2].x - _corners[0].x);
            _recyclableViewBounds.min = new Vector3(_corners[0].x - threshHold, _corners[0].y);
            _recyclableViewBounds.max = new Vector3(_corners[2].x + threshHold, _corners[2].y);
        }

        private void SetLeftAnchor(RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height;

            Vector2 pos = IsGrid ? new Vector2(0, 1) : new Vector2(0, 0.5f);

            rectTransform.anchorMin = pos;
            rectTransform.anchorMax = pos;
            rectTransform.pivot = pos;

            rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}