using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.UI.Widgets
{
    public class UI_DualStatsWidget : MonoBehaviour
    {
        [SerializeField] private UI_StatWidget Stats1;
        [SerializeField] private UI_StatWidget Stats2;

        public void Refresh(KeyValuePair<string, int> stats1, KeyValuePair<string, int> stats2)
        {
            Stats1.Refresh(stats1.Key, stats1.Value);
            Stats2.Refresh(stats2.Key, stats2.Value);
        }

        public void Clean()
        {
            Stats1.Clean();
            Stats2.Clean();
        }
    }
}
