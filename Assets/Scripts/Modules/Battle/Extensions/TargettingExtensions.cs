using RPGTest.Models.Entity;
using System.Collections.Generic;

namespace RPGTest.Modules.Battle.Extensions
{
    public static class TargettingExtensions
    {
        /// <summary>
        /// Returns the index of the first enemy that is alive in given array
        /// </summary>
        /// <param name="enemies">List of <see cref="Enemy"/> for selection</param>
        /// <returns>Index of the first matching element</returns>
        public static int GetIndexOfFirstAliveEnemy(this List<Enemy> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].IsAlive)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the last enemy that is alive in given array
        /// </summary>
        /// <param name="enemies">List of <see cref="Enemy"/> for selection</param>
        /// <returns>Index of the last matching element</returns>
        public static int GetIndexOfLastAliveEnemy(this List<Enemy> entities)
        {
            for (int i = entities.Count - 1; i > 0; i--)
            {
                if (entities[i].IsAlive)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the matching playable character in given array
        /// </summary>
        /// <param name="playableCharacters">List of <see cref="PlayableCharacter"/> for selection</param>
        /// <param name="ally"><see cref="PlayableCharacter"/> to find index of</param>
        /// <returns>Index of matching element</returns>
        public static int GetIndexOfAlly(this List<PlayableCharacter> playableCharacters, PlayableCharacter ally)
        {
            for (int i = 0; i < playableCharacters.Count; i++)
            {
                if (playableCharacters[i].Id == ally.Id)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}