using System;

namespace Watermelon
{
    public class UIMonthlyBuy : UIIAPOffer
    {
        private static Action callback;
        public static void Show(Action action)
        {
            callback = action;
            UIController.ShowPage<UIMonthlyBuy>();
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();
            callback?.Invoke();
        }
        public override void PlayShowAnimation()
        {
            base.PlayShowAnimation();
        }
    }
}