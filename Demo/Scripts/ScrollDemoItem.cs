using Hzeff.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hzeff.Demo
{
    public struct ScrollData
    {
        public Color Color;
        public string Text;
    }

    public class ScrollDemoItem : MonoBehaviour, IScrollData
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private TMP_Text indexTxt;
        [SerializeField] private Image image;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }


        private ScrollData _data;
        private int _index;
        
        public void SetData(ScrollData data, int index)
        {
            _data = data;
            _index = index;
            
            text.text = data.Text;
            indexTxt.text = index.ToString();
            image.color = data.Color;
        }
        
        public void OnClick()
        {
            Debug.Log($"Index: {_index}, Text: {_data.Text}");
        }
    }
}