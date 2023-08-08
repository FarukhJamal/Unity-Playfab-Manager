using UnityEngine;
using System.Collections;
using eeGames.Widget;
using UnityEngine.UI;

public class wMiniGame : Widget
{

    [SerializeField] private Button m_closeButton;
    [SerializeField] private Text m_text;
    private void OnCloseButtonClick()
    {
        WidgetManager.Instance.Pop();
    }
    protected override void Awake()
    {
        base.Awake();
        m_closeButton.onClick.AddListener(OnCloseButtonClick);
    }

    void OnDestroy()
    {
        m_closeButton.onClick.RemoveListener(OnCloseButtonClick);
        base.DestroyWidget();
    }

   
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            var widget = WidgetManager.Instance.GetWidget(WidgetName.MainMenu) as wMainMenu;
            if (widget != null) m_text.text = "Your Lucky # " + widget.GetRandomGeneratedNumber();

            // Function Below Get called From Widget Manager that's cool isn't it ? 
            WidgetManager.Instance.UpdateWidget(WidgetName.MiniGame);
        }
    }

    public override void UpdateWidget()
    {
        Debug.Log("Mini Game UpdateWidget Get Called From Widget Manager");
    }
	
}
