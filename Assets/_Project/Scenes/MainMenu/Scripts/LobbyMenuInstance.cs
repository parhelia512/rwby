using Fusion;
using rwby.ui.mainmenu;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Selectable = rwby.ui.Selectable;

namespace rwby
{
    public class LobbyMenuInstance : MonoBehaviour
    {
        [System.Serializable]
        public struct CSSConnection
        {
            public Selectable cssSelectable;
            public ModObjectReference characterReference;
        }
        
        public int playerID;
        
        public Canvas canvas;

        public GameObject defaultSelectedObject;
        
        private LobbyMenuHandler lobbyMenuHandler;

        [Header("Content")] 
        public Selectable readyButton;
        public Selectable profileButton;
        public Selectable spectateButton;
        public Selectable exitButton;
        public Selectable topBar;
        public Transform characterContentTransform;
        public GameObject characterContentPrefab;
        public GameObject characterSelectMenu;
        public GameObject characterSelectBigCharacter;
        public CSSConnection[] cssConnections;

        public void Initialize(LobbyMenuHandler menuHandler)
        {
            this.lobbyMenuHandler = menuHandler;
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text = lobbyMenuHandler.sessionManagerGamemode.Runner.IsServer ? "Start Match" : "Ready";
            if (lobbyMenuHandler.sessionManagerGamemode.Runner.IsServer)
            {
                readyButton.GetComponent<Selectable>().onSubmit.AddListener(() => _ = lobbyMenuHandler.StartMatch());
            }
            exitButton.onSubmit.AddListener(menuHandler.ExitLobby);

            for (int i = 0; i < cssConnections.Length; i++)
            {
                cssConnections[i].cssSelectable.onSubmit.AddListener(() => { SetCharacter(cssConnections[i].characterReference); });
            }
            
            ResetCharacterList();
        }

        public void ResetCharacterList()
        {
            // TODO
            /*
            foreach(Transform child in characterContentTransform)
            {
                Destroy(child.gameObject);
            }

            PlayerRef localPlayerRef = lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            ClientManager cm = lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();
            for (int i = 0; i < cm.ClientPlayers[playerID].characterReferences.Count; i++)
            {
                int selectIndex = i;
                GameObject chara = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
                chara.GetComponentInChildren<TextMeshProUGUI>().text = "?";
                chara.GetComponent<Selectable>().onSubmit.AddListener(() => {OpenCharacterSelect(selectIndex);});
            }

            if (cm.ClientPlayers[playerID].characterReferences.Count == lobbyMenuHandler.sessionManagerGamemode.settings
                    .GetTeamDefinition(cm.ClientPlayers[playerID].team).maxCharactersPerPlayer) return;
            GameObject cAdd = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
            cAdd.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            cAdd.GetComponent<Selectable>().onSubmit.AddListener(TryAddCharacter);*/
        }

        void TryAddCharacter()
        {
            PlayerRef localPlayerRef = lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            ClientManager cm = lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();
            // TODO
            //cm.CLIENT_SetPlayerCharacterCount(playerID, cm.ClientPlayers[playerID].characterReferences.Count+1);
        }
        
        public int currentSelectingCharacter = 0;
        public void OpenCharacterSelect(int playerCharacterIndex)
        {
            currentSelectingCharacter = playerCharacterIndex;
            characterSelectMenu.SetActive(true);
        }

        public void OpenCustomCharacterSelect()
        {
            
        }
        
        public void SetCharacter(ModObjectReference characterReference)
        {
            characterSelectMenu.SetActive(false);
        }
        
        public void Cleanup()
        {
            
        }
    }
}