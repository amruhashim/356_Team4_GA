using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace solidocean
{

    public class InspectManager : MonoBehaviour
    {
        [Header("Inspect Type")]
        public Types types;

        public enum Types { Text, TextAndImage };




        [Header("Texts")]
        public string Title;
        [TextArea]
        public string Description;
        [Header("Images")]
        public Sprite image;

        [Header("References")]
        public GameObject Inspector;
        [HideInInspector] public TextMeshProUGUI _Title;
        [HideInInspector] public TextMeshProUGUI _Description;
        [HideInInspector] public Image _image;



        public void ActiveInspector()
        {
            Inspector.SetActive(true);

            _Title.text = Title;
            _Description.text = Description;
            _image.sprite = image;

            if (types == Types.Text)
            {

                _Title.gameObject.SetActive(true);
                _Description.gameObject.SetActive(true);
                _image.gameObject.SetActive(false);


            }
            else if (types == Types.TextAndImage)
            {

                _Title.gameObject.SetActive(true);
                _Description.gameObject.SetActive(true);
                _image.gameObject.SetActive(true);

            }
        }

        public void DeactiveInspector()
        {
            Inspector.SetActive(false);
        }


    }
}

