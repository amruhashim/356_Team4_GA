using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace solidocean {
    public class CanvasManager : Singleton<CanvasManager>

    {
        [Tooltip("You need to add all of your states in here.")]
        [Header("List of States")]
        [SerializeField]
        List<GameObject> States = new List<GameObject>();


        [Tooltip("Assign starting state in here.")]
        public CanvasName FirstCanvas;

        [HideInInspector]
        public Animator CanvasAnimator;


        List<CanvasController> canvasControllerList;

        [HideInInspector]
        public CanvasController ActiveCanvas;
        [HideInInspector]
        public CanvasController PreviousCanvas;

        private InspectManager inspectManager;




        protected override void Awake()
        {

            foreach (GameObject states in States)
            {
                states.SetActive(true);
            }


            base.Awake();

            inspectManager = FindFirstObjectByType<InspectManager>();




            canvasControllerList = GetComponentsInChildren<CanvasController>().ToList();
            canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
            StartCoroutine(PlayNextCanvasAnimation(FirstCanvas));





        }

        private void OnEnable()
        {
            StartCoroutine(PlayNextCanvasAnimation(FirstCanvas));
        }




        public void GoToNextCanvas(CanvasName _name)
        {
            if (ActiveCanvas != null)
            {
                ActiveCanvas.gameObject.SetActive(false);
            }

            inspectManager.DeactiveInspector();



            CanvasController NextCanvas = canvasControllerList.Find(x => x.canvasName == _name);
            if (NextCanvas != null)
            {

                PreviousCanvas = ActiveCanvas;
                NextCanvas.gameObject.SetActive(true);
                ActiveCanvas = NextCanvas;
                NextCanvas.GetComponent<CanvasController>().StartSelectable.Select();
            }
            else { Debug.LogWarning("The next canvas was not found!"); }


        }

        public void GoToPreviousCanvas()
        {



            if (ActiveCanvas != null)
            {
                ActiveCanvas.gameObject.SetActive(false);
            }

            inspectManager.DeactiveInspector();

            CanvasController NextCanvas = canvasControllerList.Find(x => x.canvasName == ActiveCanvas.previousCanvas);



            if (ActiveCanvas.canvasName.canGoPreviousCanvas == true)
            {
                PreviousCanvas = ActiveCanvas;
                NextCanvas.gameObject.SetActive(true);
                ActiveCanvas = NextCanvas;
                NextCanvas.GetComponent<CanvasController>().StartSelectable.Select();

                //Debug.Log("Can go previous canvas.");

            }
            else
            {
                //Debug.Log("Can't go previous canvas.");
            }
            //Debug.Log("Go Back Performed");
        }



        public IEnumerator PlayNextCanvasAnimation(CanvasName _type)
        {



            CanvasAnimator.Play("out_canvas");
            yield return new WaitForSeconds(0.1f);
            GoToNextCanvas(_type);



            CanvasAnimator.Play("in_canvas");

        }

        public IEnumerator PlayPreviousCanvasAnimation()
        {

            CanvasAnimator.Play("out_canvas");
            yield return new WaitForSeconds(0.1f);
            GoToPreviousCanvas();
            CanvasAnimator.Play("in_canvas");

        }

        public void LeaveGame()
        {
            Application.Quit();
            Debug.Log("When you build, your game will close when submit this button.");
        }




    }

}
