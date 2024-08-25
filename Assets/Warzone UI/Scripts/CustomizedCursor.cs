using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace solidocean { 
public class CustomizedCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;

    private void Start()
    {

        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
}
}
