using System.Collections.Generic;
using Dialogues;
using UnityEngine;
using TMPro;
using UI;

public class DescriptionQuestPanelUpdateConditionText : MonoBehaviour
{
    public TextMeshProUGUI DescriptionText;
    private void Start()
    {
        DescriptionPanelController.OnUpdateTextConditionsUpgradeBase += UpdateDT;
    }

    private void OnDestroy()
    {
        DescriptionPanelController.OnUpdateTextConditionsUpgradeBase -= UpdateDT;
    }

    private async void UpdateDT()
    {
        Debug.Log("Попытка обновить панель описания квеста");
        PlayerResources playerResources =
            await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);
        List<string> ImprovementReport = await BaseUpgradeConditionManager.Instance.CanUpgradeMobileBase(playerResources, 1);
        
        DescriptionText.text = $"";
        foreach (var report in ImprovementReport)
        {
            DescriptionText.text += $"\n- {report} ";
        }
        Debug.Log("Завершено");
    }
}
