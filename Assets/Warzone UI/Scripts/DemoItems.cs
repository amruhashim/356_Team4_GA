using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace solidocean
{
    public class DemoItems : MonoBehaviour
    {

        public HorizontalSelector colorSelector;
        public Slider slider;
        public Image box;

        float sliderValue;

        private void Update()
        {

            sliderValue = slider.value / 10;

            //This is for Horizontal Selector to control color of the Image.
            switch (colorSelector.value)
            {
                case "RED":
                    box.color = new Color(1, 0.224f, 0.224f, sliderValue);
                    break;
                case "ORANGE":
                    box.color = new Color(1, 0.639f, 0.149f, sliderValue);
                    break;
                case "YELLOW":
                    box.color = new Color(1, 0.973f, 0.192f, sliderValue);
                    break;
                case "GREEN":
                    box.color = new Color(0.153f, 0.788f, 0.173f, sliderValue);
                    break;
                case "BLUE":
                    box.color = new Color(0.184f, 0.686f, 1, sliderValue);
                    break;
                case "PURPLE":
                    box.color = new Color(0.804f, 0.271f, 1, sliderValue);
                    break;
                case "BLACK":
                    box.color = new Color(0, 0, 0, sliderValue);
                    break;
                case "WHITE":
                    box.color = new Color(1, 1, 1, sliderValue);
                    break;
            }
        }

        //This is for toggle to active or deactive Image.
        public void ActiveDeactiveObject()
        {
            if (box.gameObject.activeSelf == true)
            {
                box.gameObject.SetActive(false);
            }
            else
            {
                box.gameObject.SetActive(true);
            }
        }
    }

}