using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Steamworks;
using UnityEngine;

public class CursorState : MonoBehaviour
{ 
    void Update()
    {
        Cursor.lockState = GameManager.instance.gameState == GameManager.GameState.InGame ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = GameManager.instance.gameState == GameManager.GameState.InGame ? false : true;
    }
    
}
