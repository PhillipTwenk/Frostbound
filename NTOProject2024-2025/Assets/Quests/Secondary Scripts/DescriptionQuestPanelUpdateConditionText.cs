using System.Collections.Generic;
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

    public async void UpdateDT()
    {
        PlayerResources playerResources =
            await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);
        List<string> ImprovementReport = await BaseUpgradeConditionManager.Instance.CanUpgradeMobileBase(playerResources);
        
        DescriptionText.text = $"";
        foreach (var report in ImprovementReport)
        {
            DescriptionText.text += $"\n- {report} ";
        }
    }
}
