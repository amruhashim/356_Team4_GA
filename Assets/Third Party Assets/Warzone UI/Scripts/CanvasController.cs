using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace solidocean
{
    public class CanvasController : MonoBehaviour

    {
        [Header("Settings")]
        [Tooltip("Assign this canvases name.")]
        public CanvasName canvasName;
        [Tooltip("Assign any canvas you want to go when any back button is clicked.")]
        public CanvasName previousCanvas;

        [Header("UI Settings")]
        [Tooltip("Assign this canvases starting selectable. (Button, Slider etc.)")]
        public Button StartSelectable;



        CanvasManager canvasManager;




        private void OnEnable()
        {
            canvasManager = CanvasManager.GetInstance();





        }


    }
}
