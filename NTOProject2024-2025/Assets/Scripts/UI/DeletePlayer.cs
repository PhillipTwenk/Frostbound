using TMPro;
using UnityEngine;

public class DeletePlayer : MonoBehaviour
{
    [SerializeField] private string DefaultPlayername;
    [SerializeField] private EntityID playerID;
    [SerializeField] private TextMeshProUGUI textNewCharacter;
    [SerializeField] private TextMeshProUGUI textPlayerName;

    /// <summary>
    /// Нажатие на кнопку удаления игрока
    /// </summary>
    public async void ClickDeletePlayerButton()
    {
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
        
        textNewCharacter.gameObject.SetActive(true);
        textNewCharacter.text = DefaultPlayername;
        textPlayerName.gameObject.SetActive(false);

        await playerID.DefaultRevert();
        
        playerID.Name = "None";
        
        gameObject.SetActive(false);
        
        JSONSerializeManager.Instance.JSONSave();
        
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }
}
