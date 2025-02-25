using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarShopControl : MonoBehaviour
{
    [SerializeField] private GameObject yourScrollbar;
    private void Start()
    {
        Scrollbar scrollbar = yourScrollbar.GetComponent<Scrollbar>();
        scrollbar.size = 0.1f;
    }
}
