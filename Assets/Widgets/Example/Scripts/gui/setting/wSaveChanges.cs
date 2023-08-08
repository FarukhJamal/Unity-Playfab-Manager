using UnityEngine;
using System.Collections;
using eeGames.Widget;
using UnityEngine.UI;

public class wSaveChanges : Widget 
{
    [SerializeField]
    private Button m_yesButton;
    [SerializeField]
    private Button m_noButton;

    #region UNity Events
    protected override void Awake() 
    {
        m_noButton.onClick.AddListener(OnNoButtonClick);
        m_yesButton.onClick.AddListener(OnYesButtonClick);
    }

    void OnDestroy()
    {
        m_noButton.onClick.RemoveListener(OnNoButtonClick);
        m_yesButton.onClick.RemoveListener(OnYesButtonClick);
        DestroyWidget();
    }
    #endregion

    private void OnYesButtonClick() 
    {
        WidgetManager.Instance.Pop(WidgetName.SaveSetting, false);
    }
    private void OnNoButtonClick()
    {
        WidgetManager.Instance.Pop(WidgetName.SaveSetting, false);
    }
}
