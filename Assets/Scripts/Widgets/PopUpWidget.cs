using System;
using System.Collections;
using System.Collections.Generic;
using eeGames.Widget;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpWidget : Widget
{
    // Start is called before the first frame update
    public TMP_Text title, body;
    public Button CloseBtn;

    private void Start()
    {
        
        CloseBtn.onClick.AddListener(delegate
        {
            UIManager.Instance.ClosePopUp();
        });
    }

    private void OnDestroy()
    {
        CloseBtn.onClick.RemoveAllListeners();
    }
}
