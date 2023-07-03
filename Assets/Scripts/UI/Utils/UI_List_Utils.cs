using RPGTest.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Utils
{
    public static class UI_List_Utils
    {
        /// <summary>
        /// Orders objects alphabetically and put the given list GameObject as their parent
        /// </summary>
        /// <param name="list">GameObject to use as parent</param>
        /// <param name="objects">GameObjects to order alphabetically</param>
        public static void RefreshHierarchy(GameObject list, IEnumerable<GameObject> objects)
        {
            var objectList = objects.OrderBy(x => x.name).ToList();

            for (int i = 0; i < objectList.Count; i++)
            {
                objectList[i].transform.SetParent(list.transform);
                objectList[i].transform.SetSiblingIndex(i);
                objectList[i].transform.localScale = new Vector3(1, 1, 1);
            }
        }

        public static void SetVerticalNavigation(List<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                var index = objects.IndexOf(obj);

                obj.GetComponent<Button>().ExplicitNavigation(
                    Up: index > 0 ? objects[index - 1].GetComponent<Button>() : null,
                    Down: index < objects.Count - 1 ? objects[index + 1].GetComponent<Button>() : null);
            }
        }
    }
}
