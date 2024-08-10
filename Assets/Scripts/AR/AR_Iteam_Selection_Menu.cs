using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR_Iteam_Selection_Menu : MonoBehaviour
{
    public NetworkObjectManager nOB;

    public void IteamSelectionMenu(int index)
    {
        switch (index)
        {
            case 0:
                nOB.SpawnNetworkObject(0);
                break;
            case 1:
                nOB.SpawnNetworkObject(1);
                break;
            case 2:
                nOB.SpawnNetworkObject(2);
                break;
            case 3:
                nOB.SpawnNetworkObject(3);
                break;
            case 4:
                nOB.SpawnNetworkObject(4);
                break;
            case 5:
                nOB.SpawnNetworkObject(5);
                break;
        }


    }
}
