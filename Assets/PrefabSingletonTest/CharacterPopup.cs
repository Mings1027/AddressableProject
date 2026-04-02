using UnityEngine;

namespace SingletonTest
{
    public class CharacterPopup: PrefabSingleton<CharacterPopup>
    {
        public void ShowCharacterStatus()
        {
            Debug.Log("ShowCharacterStatus");
        }
    }
}