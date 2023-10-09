using UnityEngine;

namespace RPGTest.UI
{
    public partial class UI_PauseButtonAnimatorController : MonoBehaviour
    {
        public int MaxIndex;
        public int Index = 0;

        public void ChangeIndex(int variation)
        {
            if (variation < 0)
            {
                Index = MaxIndex;
            }
            else if (variation > MaxIndex)
            {
                Index = 0;
            }
            else
            {
                Index = variation;
            }
        }
    }
}
