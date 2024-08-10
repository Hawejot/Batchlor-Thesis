using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AR_Main_Menu : MonoBehaviour
{
    public ARTutorial arTutorial;

    public RadialSelection rsIteam;

    public SimpleMatchmaking simpleMatchmaking;

    public void MainMenuLogic(int action)
    {
        switch(action)
        {

            case 0:
            //Open Leanrnig Station Wheel
                Debug.Log("Open Learning Station");
                rsIteam.SpawnRadialPart();
                break;
            case 1:
               SceneManager.LoadScene("MainMenu");
                
            break;
            case 2:
                Application.Quit();
            break;
            case 3:
                Debug.Log("Starting Multiplayer");
                simpleMatchmaking.StartLobbyButtonStart();
            //Multi Player Controller
            break;
            case 4:
                arTutorial.OpenTutorial();
            break;
        }
    }
}
