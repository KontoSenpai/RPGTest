using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace RPGTest.Helpers
{
    public enum NavigationSlot
    {
        Left,
        Right,
        Up,
        Down,
    }
    public static class UIExtension
    {
        public static void UpdateExplicitNavigation(this Button button, Dictionary<NavigationSlot, Button> updatedSlots)
        {
            var oldNavigation = button.navigation;
            button.navigation = new Navigation
            {
                mode = oldNavigation.mode,
                selectOnLeft = updatedSlots.TryGetValue(NavigationSlot.Left, out var left) ? left : oldNavigation.selectOnLeft,
                selectOnRight = updatedSlots.TryGetValue(NavigationSlot.Right, out var right) ? right : oldNavigation.selectOnRight,
                selectOnUp = updatedSlots.TryGetValue(NavigationSlot.Up, out var up) ? up : oldNavigation.selectOnUp,
                selectOnDown = updatedSlots.TryGetValue(NavigationSlot.Down, out var down) ? down : oldNavigation.selectOnDown,
            };
        }

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
