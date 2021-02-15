using RPGTest.Models;
using RPGTest.Models.Banks;
using RPGTest.Models.Entity;
using System.Linq;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class PlayableCharactersCollector : ICollector
    {
        static public string CharactersAsset  = ((TextAsset)Resources.Load("Configs/PlayableCharacters")).text;
        static public string GrowthTableAsset = ((TextAsset)Resources.Load("Configs/GrowthTable")).text;

        private static PlayableCharacterBank _characterbank;
        public static PlayableCharacterBank CharacterBank => _characterbank ?? (_characterbank = Collect<PlayableCharacterBank>(CharactersAsset));

        private static GrowthTableBank _growthBank;
        public static GrowthTableBank GrowthTableBank => _growthBank ?? (_growthBank = Collect<GrowthTableBank>(GrowthTableAsset));

        public static PlayableCharacter TryGetPlayableCharacter(string ID)
        {
            if (CharacterBank != null && CharacterBank.PlayableCharacters != null)
            {
                var playableCharacter = CharacterBank.PlayableCharacters.SingleOrDefault(x => x.Id == ID);
                playableCharacter.Initialize(TryGetCharacterGrowthTable(ID));
                return playableCharacter;
            }
            return null;
        }

        public static GrowthTable TryGetCharacterGrowthTable(string ID)
        {
            if (GrowthTableBank != null && CharacterBank.PlayableCharacters != null)
            {
                var growthTable = GrowthTableBank.GrowthTables.SingleOrDefault(x => x.CharacterId == ID);
                return growthTable;
            }
            return null;
        }
    }
}
