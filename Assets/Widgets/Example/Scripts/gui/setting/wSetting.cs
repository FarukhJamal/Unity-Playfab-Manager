using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using eeGames.Widget;

public class wSetting : Widget
{

    [SerializeField] private Button m_audioButton;
    [SerializeField] private Button m_gameButton;
    [SerializeField] private Button m_saveButton;
    [SerializeField] private Button m_closeButton;


    #region UNity Methods
    protected override void Awake()
    {
        m_audioButton.onClick.AddListener(OnAudioButtonClick);
        m_gameButton.onClick.AddListener(OnGameButtonClick);
        m_saveButton.onClick.AddListener(OnSaveButtonClick);
        m_closeButton.onClick.AddListener(OnCloseButtonClick);
       
    }

    void OnDestroy()
    {
        m_audioButton.onClick.RemoveListener(OnAudioButtonClick);
        m_gameButton.onClick.RemoveListener(OnGameButtonClick);
        m_saveButton.onClick.RemoveListener(OnSaveButtonClick);
        m_closeButton.onClick.RemoveListener(OnCloseButtonClick);
        base.DestroyWidget();
    }

    #endregion

    #region Helper Method
    void OnAudioButtonClick()
    {
        WidgetManager.Instance.Push(WidgetName.AudioSetting, false, true);
    }
    void OnGameButtonClick()
    {
        WidgetManager.Instance.Push(WidgetName.GameSetting, false, true);
    }
    void OnSaveButtonClick()
    {
        WidgetManager.Instance.Push(WidgetName.SaveSetting, false, true);
    }

    private void OnCloseButtonClick()
    {
        WidgetManager.Instance.Pop(false);
    }
    #endregion

}
