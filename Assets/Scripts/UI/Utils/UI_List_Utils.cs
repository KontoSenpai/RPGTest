using RPGTest.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Utils
{
    public static class UI_List_Utils
    {
        public static int GetAmountOfObjectsPerGridRow(int objectCount, GridLayoutGroup.Constraint constraintType, int constraint)
        {
            switch (constraintType)
            {
                case GridLayoutGroup.Constraint.FixedRowCount:
                    return Mathf.CeilToInt((objectCount - 1) / constraint);
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    return constraint;
                case GridLayoutGroup.Constraint.Flexible:
                    Debug.LogError("Unsupported constraint type for method");
                    break;
            }
            return -1;
        }

        /// <summary>
        /// Orders objects alphabetically and put the given list GameObject as their parent
        /// </summary>
        /// <param name="list">GameObject to use as parent</param>
        /// <param name="objects">GameObjects to order alphabetically</param>
        public static List<GameObject> RefreshHierarchy(GameObject list, IEnumerable<GameObject> objects)
        {
            var objectList = objects.OrderBy(x => x.name).ToList();

            for (int i = 0; i < objectList.Count; i++)
            {
                objectList[i].transform.SetParent(list.transform);
                objectList[i].transform.SetSiblingIndex(i);
                objectList[i].transform.localScale = new Vector3(1, 1, 1);
            }

            return objectList;
        }

        /// <summary>
        /// Explicit the navigation within a vertical layout group
        /// </summary>
        /// <param name="objects">Objects contained in the Layout Group</param>
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

        /// <summary>
        /// Explicit the navigation within a grid layout group.
        /// Objects are added horizontally regardless of the constraint type.
        /// </summary>
        /// <param name="objects">Objects contained in the Layout Group</param>
        /// <param name="constraint"></param>
        public static void SetGridNavigation(List<GameObject> objects, int itemsPerRow)
        {
            for(int i = 0; i < objects.Count; i++)
            {
                var adjustedIndex = i + 1;
                var beginningOfRow = adjustedIndex == 1 || (adjustedIndex > itemsPerRow && adjustedIndex % itemsPerRow == 1);
                var endOfRow = adjustedIndex == objects.Count || adjustedIndex % itemsPerRow == 0;

                var isFirstRow = adjustedIndex - itemsPerRow <= 0;
                var isLastRow = adjustedIndex + itemsPerRow >= objects.Count;
                objects[i].GetComponent<Button>().ExplicitNavigation(
                    Left: beginningOfRow ? null : objects[i-1].GetComponent<Button>(),
                    Right: endOfRow ? null : objects[i+1].GetComponent<Button>(),
                    Up: isFirstRow ? null : objects[i - itemsPerRow].GetComponent<Button>(),
                    Down: isLastRow ? null : objects[i + itemsPerRow].GetComponent<Button>());
            }
        }
    }
}
