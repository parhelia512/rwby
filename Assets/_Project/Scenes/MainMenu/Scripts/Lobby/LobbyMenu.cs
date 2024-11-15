using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using rwby.ui.mainmenu;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui
{
    public class LobbyMenu : MenuBase
    {
        public enum LobbySubMenuType
        {
            MAIN,
            TEAM_SELECT,
            PROFILE_SELECT,
            LOBBY_SETTINGS
        }
        
        [SerializeField] private List<int> history = new List<int>();
        
        public LobbyMenuInstance lobbyMenuInstance;

        public GameObject defaultSelectedUIItem;
        public CanvasGroup canvasGroup;

        [Header("Content")] 
        public GameObject lobbyOptionsMenu;
        public Selectable readyButton;
        public Selectable characterSelectButton;
        public Selectable selectTeamButton;
        public Selectable profileButton;
        public Selectable configureButton;
        public Selectable settingsButton;
        public Selectable exitButton;
        public Transform characterContentTransform;
        public GameObject characterContentPrefab;
        public CharacterSelectMenu characterSelectMenu;
        public TextMeshProUGUI gamemodeNameText;
        public TextMeshProUGUI mapNameText;

        public GameObject lobbyOptionsContentHolder;

        [Header("Lobby Players")] 
        public GameObject lobbyPlayerTeamHolder;
        public Transform lobbyPlayerContentHolder;
        public GameObject lobbyPlayerHeader;
        public TextMeshProUGUI songText;

        [Header("Team Select")]
        public GameObject teamSelectContentHolder;
        public GameObject teamSelectButtonPrefab;

        [Header("Submenus")] 
        public GameObject lobbyMainMenu;
        public GameObject teamSelectMenu;
        
        private int currentSelectingCharacterIndex = 0;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            
            readyButton.onSubmit.RemoveAllListeners();
            exitButton.onSubmit.RemoveAllListeners();
            characterSelectButton.onSubmit.RemoveAllListeners();
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text = lobbyMenuInstance.lobbyMenuHandler
                .sessionManagerGamemode.Runner.IsServer ? "Start Match" : "Ready";
            if (lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.IsServer)
            {
                readyButton.onSubmit.AddListener(async () => await lobbyMenuInstance.lobbyMenuHandler.StartMatch());
            }
            else
            {
                readyButton.onSubmit.AddListener(ReadyUp);
            }
            exitButton.onSubmit.AddListener(ExitLobby);
            characterSelectButton.onSubmit.AddListener(OpenCharacterSelect);
            
            Refresh();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
        
        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(EventSystem.current, GameManager.singleton.localPlayerManager.localPlayers[lobbyMenuInstance.playerID]))
            {
                if (canvasGroup.interactable && history.Count == 0)
                {
                    EventSystem.current.SetSelectedGameObject(defaultSelectedUIItem);
                }
            }
        }

        public void BUTTON_ChangeTeam()
        {
            history.Add((int)LobbySubMenuType.TEAM_SELECT);
            lobbyMainMenu.SetActive(false);
            teamSelectMenu.SetActive(true);

            foreach (Transform child in teamSelectContentHolder.transform)
            {
                Destroy(child.gameObject);
            }
            
            var smgm = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode;
            for (int i = 0; i < smgm.teamDefinitions.Count; i++)
            {
                int index = i;
                var g = GameObject.Instantiate(teamSelectButtonPrefab, teamSelectContentHolder.transform, false);
                g.GetComponent<rwby.ui.Selectable>().onSubmit.AddListener(() => OnTeamSelectButton(index));
            }

            var backButton = GameObject.Instantiate(teamSelectButtonPrefab, teamSelectContentHolder.transform, false);
            backButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
            backButton.GetComponent<rwby.ui.Selectable>().onSubmit.AddListener(() =>
            {
                history.Clear();
                lobbyMainMenu.SetActive(true);
                teamSelectMenu.SetActive(false);
            });
        }

        private void OnTeamSelectButton(int index)
        {
            SetTeam((byte)index);
            history.Clear();
            lobbyMainMenu.SetActive(true);
            teamSelectMenu.SetActive(false);
        }

        public void BUTTON_ChangeProfile()
        {
            history.Add((int)LobbySubMenuType.PROFILE_SELECT);
            lobbyMainMenu.SetActive(false);
            teamSelectMenu.SetActive(true);
            
            foreach (Transform child in teamSelectContentHolder.transform)
            {
                Destroy(child.gameObject);
            }

            var pManager = GameManager.singleton.profilesManager;
            for (int i = 0; i < pManager.Profiles.Count; i++)
            {
                int index = i;
                var g = GameObject.Instantiate(teamSelectButtonPrefab, teamSelectContentHolder.transform, false);
                g.GetComponentInChildren<TextMeshProUGUI>().text = pManager.Profiles[i].profileName;
                g.GetComponent<rwby.ui.Selectable>().onSubmit.AddListener(() => OnProfileSelectButton(index));
            }
            
            var backButton = GameObject.Instantiate(teamSelectButtonPrefab, teamSelectContentHolder.transform, false);
            backButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
            backButton.GetComponent<rwby.ui.Selectable>().onSubmit.AddListener(() =>
            {
                history.Clear();
                lobbyMainMenu.SetActive(true);
                teamSelectMenu.SetActive(false);
            });
        }

        private void OnProfileSelectButton(int index)
        {
            GameManager.singleton.profilesManager.ApplyProfileToPlayer(0, index);
            history.Clear();
            lobbyMainMenu.SetActive(true);
            teamSelectMenu.SetActive(false);
            
            PlayerRef localPlayerRef = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            ClientManager cm = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();
            cm.profiles[lobbyMenuInstance.playerID] = GameManager.singleton.profilesManager.Profiles[index].profileName;
            Refresh();
        }

        private void ExitLobby()
        {
            lobbyMenuInstance.lobbyMenuHandler.ExitLobby();
        }

        private void ReadyUp()
        {
            PlayerRef localPlayerRef = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            if (clientInfo.players.Count <= lobbyMenuInstance.playerID) return;
            ClientManager cm = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();
            cm.CLIENT_SetReadyStatus(!cm.ReadyStatus);
        }

        public void Refresh()
        {
            PlayerRef localPlayerRef = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            if (clientInfo.players.Count <= lobbyMenuInstance.playerID) return;
            ClientManager cm = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();

            profileButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Profile: {cm.profiles[lobbyMenuInstance.playerID]}";
            
            foreach(Transform child in characterContentTransform)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < clientInfo.players[lobbyMenuInstance.playerID].characterReferences.Count; i++)
            {
                GameObject chara = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
                chara.GetComponentInChildren<TextMeshProUGUI>().text = "?";
                if (clientInfo.players[lobbyMenuInstance.playerID].characterReferences[i].IsValid())
                {
                    IFighterDefinition fighterDefinition = ContentManager.singleton.
                        GetContentDefinition<IFighterDefinition>(clientInfo.players[lobbyMenuInstance.playerID].characterReferences[i]);
                    if(fighterDefinition) chara.GetComponentInChildren<TextMeshProUGUI>().text = fighterDefinition.Name[0].ToString();
                }
                int selectIndex = i;
            }

            var cGamemode = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.CurrentGameMode;
            if (cGamemode != null)
            {
                gamemodeNameText.text = cGamemode.definition.Name;
            }

            UpdatePlayerList();
        }

        private Dictionary<int, GameObject> teamHolders = new Dictionary<int, GameObject>();
        private void UpdatePlayerList()
        {
            var smg = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode;
            //var playerList = smg.GetPlayerList();

            foreach (Transform child in lobbyPlayerContentHolder)
            {
                Destroy(child.gameObject);
            }

            foreach (var th in teamHolders)
            {
                Destroy(th.Value);
            }
            teamHolders.Clear();

            for (int i = 0; i < smg.teamDefinitions.Count; i++)
            {
                teamHolders.Add(i, GameObject.Instantiate(lobbyPlayerTeamHolder, lobbyPlayerContentHolder, false));
            }

            if (smg.teamDefinitions.Count == 1)
            {
                /*
                var singleHorizontalHolder = GameObject.Instantiate(teamListHorizontalHolder, teamListContentHolder, false);
                singleHorizontalHolder.GetComponent<LayoutElement>().preferredHeight = 480;

                var singleTeamHolder = GameObject.Instantiate(teamHolder, singleHorizontalHolder.transform, false);
                teamHolders.Add(1, singleTeamHolder);
                singleTeamHolder.GetComponent<rwby.ui.Selectable>().onSubmit.AddListener(() => SetTeam(1));*/
            }
            else if(smg.teamDefinitions.Count != 0)
            {
                /*
                byte ts = 0;
                for (int i = 0; i < (smg.teamDefinitions.Count / 4)+1; i++)
                {
                    var horizontalHolder =
                        GameObject.Instantiate(teamListHorizontalHolder, teamListContentHolder, false);
                    horizontalHolder.GetComponent<LayoutElement>().preferredHeight = 240;

                    for (int j = 0; j < 4; j++)
                    {
                        if (ts >= smg.teamDefinitions.Count) return;
                        var singleTeamHolder = GameObject.Instantiate(teamHolder, horizontalHolder.transform, false);
                        teamHolders.Add(ts+1, singleTeamHolder);
                        byte teamIndx = ts;
                        singleTeamHolder.GetComponent<rwby.ui.Selectable>().onSubmit.AddListener(() => SetTeam(teamIndx));
                        ts++;
                    }
                }*/
            }

            var players = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.GetPlayerList();

            for (int w = 0; w < players.Count; w++)
            {
                var pInfo = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode
                    .ClientDefinitions[players[w].clientIndex]
                    .players[players[w].playerIndex];
                
                var playerHeader = GameObject.Instantiate(lobbyPlayerHeader, 
                    smg.teamDefinitions.Count > 1 ? teamHolders[pInfo.team].transform : lobbyPlayerContentHolder.transform, 
                    false);
                playerHeader.GetComponent<Image>().color = ExtDebug.TEAM_COLORS[pInfo.team % ExtDebug.TEAM_COLORS.Length];
                playerHeader.GetComponentInChildren<TextMeshProUGUI>().text = players[w].clientManager.nickname;
            }
        }

        public void SetTeam(byte team)
        {
            lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode
                .CLIENT_SetPlayerTeam(lobbyMenuInstance.playerID, team);
        }

        public void OpenCharacterSelect()
        {
            canvasGroup.interactable = false;
            characterSelectMenu.OnCharactersSelected += OnCharactersSelected;
            characterSelectMenu.charactersToSelect = 1;
            currentHandler.Forward((int)LobbyMenuType.CHARACTER_SELECT);
        }

        public void OnCharactersSelected()
        {
            characterSelectMenu.OnCharactersSelected -= OnCharactersSelected;

            PlayerRef localPlayerRef = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode
                .CLIENT_SetPlayerCharacterCount(lobbyMenuInstance.playerID, characterSelectMenu.charactersSelected.Count);

            for (int i = 0; i < characterSelectMenu.charactersSelected.Count; i++)
            {
                lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode
                    .CLIENT_SetPlayerCharacter(lobbyMenuInstance.playerID, i,
                        characterSelectMenu.charactersSelected[i]);
            }
            canvasGroup.interactable = true;
            EventSystem.current.SetSelectedGameObject(defaultSelectedUIItem);
        }

        public async void OpenSongSelector()
        {
            canvasGroup.interactable = false;
            int player = lobbyMenuInstance.playerID;
            await ContentSelect.singleton.OpenMenu(lobbyMenuInstance.playerID, (int)ContentType.Song,(a, b) =>
            {
                ContentSelect.singleton.CloseMenu(player);
                _ = SetSong(b);
            });
        }

        private async UniTask SetSong(ModIDContentReference modIDContentReference)
        {
            canvasGroup.interactable = true;
            bool loadResult = await ContentManager.singleton.LoadContentDefinition(modIDContentReference);
            if (!loadResult)
            {
                Debug.LogError("Could not load given song definition.");
                return;
            }
            var ob = (ISongDefinition)ContentManager.singleton.GetContentDefinition(modIDContentReference);

            await ob.Load();
            songText.text = ob.Name;
            lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.musicToPlay = ob;
        }
    }
}