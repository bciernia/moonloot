
using EasyTalk.Controller;
using EasyTalk.Display;
using UnityEngine;

namespace EasyTalk.Demo
{
    public class Demo4ShopMenu : DialoguePanel
    {
        [SerializeField]
        private DialogueController controller;

        private void Awake()
        {
            Init();
            HideImmediately();
        }

        public void LeaveShop()
        {
            controller.Continue();
        }

        public void TapeBought()
        {
            controller.SetBoolVariable("has_tape", true);
            controller.Continue();
        }
    }
}
