using System;
using UnityEngine;

public class ClickAnyButtonForStartGame : MonoBehaviour
{
    public GameObject StartGameButtons;
    public Animator StartGameButtonsAnimator;
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            StartGameButtons.SetActive(true);
            StartGameButtonsAnimator.SetBool("ActivateButtons", true);
            gameObject.SetActive(false);
        }
    }
}
