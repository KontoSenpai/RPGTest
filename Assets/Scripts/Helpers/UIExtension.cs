using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace RPGTest.Helpers
{
    public static class UIExtension
    {
        public static void ExplicitNavigation(this Button button, Button Left = null, Button Right = null, Button Up = null, Button Down = null) 
        {
            Navigation newNavigation = new Navigation();
            newNavigation.mode = Navigation.Mode.Explicit;
            newNavigation.selectOnLeft = Left;
            newNavigation.selectOnRight = Right;
            newNavigation.selectOnUp = Up;
            newNavigation.selectOnDown = Down;
            button.navigation = newNavigation;
        }
    }
}
