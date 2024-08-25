using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace solidocean
{

    public class HintManager : MonoBehaviour
    {
        [HeaderAttribute("Settings")]
        [Tooltip("Add your game hints, tips and tricks here to show them on the loading screen.")]
        public string[] hints;

        [HeaderAttribute("References")]
        [SerializeField]
        private TextMeshProUGUI hintText;


        private void Start()
        {
            hintText = GetComponent<TextMeshProUGUI>();
        }


        private void OnEnable()
        {
            string randomHint = hints[Random.Range(0, hints.Length)];

            hintText.text = randomHint;
        }
    }
}
