    using System;
    using SingletonTest;
    using UnityEngine;

    public class GameManager: MonoBehaviour
    {
        private void Start()
        {
            CharacterPopup.Instance.ShowCharacterStatus();
        }
    }
