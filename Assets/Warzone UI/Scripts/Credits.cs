using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace solidocean
{
    public class Credits : MonoBehaviour
    {

        public float speed = 100f;
        public float beginPosition = -825f;
        public float endPosition = 825f;

        RectTransform creditsRect;

        [SerializeField]
        bool canLoop = false;

        void OnEnable()
        {
            creditsRect = GetComponent<RectTransform>();
            StartCoroutine(AutoScroll());
        }

        void OnDisable()
        {
            creditsRect.localPosition = new Vector3(creditsRect.localPosition.x, beginPosition, creditsRect.localPosition.z);
        }

        IEnumerator AutoScroll()
        {
            while (creditsRect.localPosition.y < endPosition)
            {
                creditsRect.Translate(Vector3.up * speed * Time.deltaTime);
                if (creditsRect.localPosition.y > endPosition)
                {
                    if (canLoop)
                    {
                        creditsRect.localPosition = Vector3.up * beginPosition;
                    }
                    else
                    {
                        break;
                    }
                }

                yield return null;
            }

        }
    }
}
