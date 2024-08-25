using UnityEngine;
using UnityEngine.UI;


namespace solidocean { 
[RequireComponent(typeof(Button))]

    public class CanvasSwitcher : MonoBehaviour
    {
        [Tooltip("Assign which canvas you want to go when the button is clicked.")]
        [Header("Settings")]
        public CanvasName ChangeCanvasTo;

        CanvasManager canvasManager;
        Button menuButton;

        private void Start()
        {
            menuButton = GetComponent<Button>();
            menuButton.onClick.AddListener(OnButtonClicked);
            canvasManager = CanvasManager.GetInstance();
        }

        public void OnButtonClicked()
        {

            StartCoroutine(canvasManager.PlayNextCanvasAnimation(ChangeCanvasTo));




        }
    }
}