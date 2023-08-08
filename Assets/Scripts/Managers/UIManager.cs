using System;
using eeGames.Widget;
using UnityEngine;

namespace Managers
{
    public class UIManager 
    {
        public static UIManager Instance;
        private WidgetManager _widgetManager;

        public UIManager()
        {
            Instance = this;
            _widgetManager = WidgetManager.Instance;
        }
        public void SpawnUI(WidgetName widgetName)
        {
            _widgetManager.Push(widgetName);
        }

        public void SpawnUI(WidgetName widgetName, bool playHideTween = false, bool lastActive = false,
            bool lastInteractive = false, bool firstChild = false)
        {
            _widgetManager.Push(widgetName,playHideTween,lastActive,lastInteractive,firstChild);
        }

        public void ShowErrorMessage(string title, string message)
        {
           _widgetManager.Push(WidgetName.PopUp);
            PopUpWidget popUp = _widgetManager.GetWidget(WidgetName.PopUp).GetComponent<PopUpWidget>();
            if (popUp != null)
            {
                popUp.title.text = title;
                popUp.body.text = message;
            }
        }

        public void ClosePopUp()
        {
            _widgetManager.Pop(WidgetName.PopUp);
        }
    }
}