using System;
using TMPro;
using UnityEngine;

public class Tablet : MonoBehaviour
{
    [SerializeField] private Outline outline;
    public TabletSO TabletInfo;
    public bool isCollected;
    public GameEvent OpenTabletMenuEvent;

    public void InitializeTablet()
    {
        if (BaseUpgradeConditionManager.CurrentBaseLevel > int.Parse(TabletInfo.tablet_id))
        {
            gameObject.SetActive(false);
            BaseUpgradeConditionManager.Instance.FindNote[int.Parse(TabletInfo.tablet_id) - 1] = true;
        }
    }

    public void OnMouseDown() {
        if(!isCollected && Time.timeScale != 0f){
            Debug.Log("Получена заметка");
            isCollected = true;

            //Отмечаем в скрипте контроля уровня базы
            BaseUpgradeConditionManager.Instance.FindNote[int.Parse(TabletInfo.tablet_id) - 1] = true;
            
            //UIManager.currentTablet = TabletInfo;
            OpenTabletMenuEvent.TriggerEvent();
            
            gameObject.SetActive(false);
        }
    }

    private void OnMouseEnter() {
        if(Time.timeScale != 0f && !isCollected){
            outline.enabled = true;
        }
    }
    private void OnMouseExit() {
        outline.enabled = false;
    }
}
