using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace solidocean
{
    public class HorizontalSelector : MonoBehaviour
    {
        private TextMeshProUGUI text;


        private int m_Index = 0;

        public int index
        {

            get
            {
                return m_Index;
            }
            set
            {
                m_Index = value;
                text.text = data[m_Index];
            }

        }

        public int defalutValueIndex = 0;

        public List<string> data = new List<string>();

        public string value
        {
            get
            {
                return data[m_Index];
            }
        }

        void Start()
        {
            text = transform.Find("txt_text").GetComponent<TextMeshProUGUI>();

            transform.Find("btn_left").GetComponent<Button>().onClick.AddListener(OnLeftClicked);
            transform.Find("btn_right").GetComponent<Button>().onClick.AddListener(OnRightClicked);

            index = defalutValueIndex;
        }

        void OnLeftClicked()
        {
            if (index == 0)
            {
                index = data.Count - 1; //Index starts from 0
            }
            else
            {
                index--;
            }
        }

        void OnRightClicked()
        {
            if ((index + 1) >= data.Count)
            {
                index = 0;
            }
            else
            {
                index++;
            }


        }
    }
}