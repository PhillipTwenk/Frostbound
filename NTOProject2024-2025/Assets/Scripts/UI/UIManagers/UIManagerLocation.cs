using System;
using UnityEngine;

public class UIManagerLocation : MonoBehaviour
{
    public static UIManagerLocation Instance { get; private set; }
    public static EntityID WhichPlayerCreate;

    [SerializeField] private PlayerSaveData player1SaveData;
    [SerializeField] private PlayerSaveData player2SaveData;
    [SerializeField] private PlayerSaveData player3SaveData;

    [SerializeField] private EntityID player1;
    [SerializeField] private EntityID player2;
    [SerializeField] private EntityID player3;

    private void Awake()
    {
        Instance = this;
    }

    public PlayerSaveData WhichPlayerDataUse()
    {
        if (WhichPlayerCreate == player1)
        {
            return player1SaveData;
        }else if (WhichPlayerCreate == player2)
        {
            return player2SaveData;
        }else if (WhichPlayerCreate == player3)
        {
            return player3SaveData;
        }

        return player1SaveData;
    }
}
