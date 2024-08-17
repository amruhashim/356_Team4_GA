using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] playerStats; //[0] - Health, [1] - ammo, [2] - weapon
    public float[] playerPositionAndRotation;

    //public float[] inventoryContent;

    public PlayerData(float[] _playerStats, float[] _playerPosAndRot)
    {
        playerStats = _playerStats;
        playerPositionAndRotation = _playerPosAndRot;
    }

}
