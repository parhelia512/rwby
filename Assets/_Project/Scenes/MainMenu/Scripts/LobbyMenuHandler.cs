using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace rwby.ui.mainmenu
{
    public class LobbyMenuHandler : MainMenuMenu
    {
        public List<LobbyMenuInstance> menuInstances = new List<LobbyMenuInstance>();
        public LobbyMenuInstance instancePrefab;
        public Transform instanceParent;

        public SessionManagerGamemode sessionManagerGamemode;

        [SerializeField] private Camera mainMenuCamera;
        [SerializeField] private GameObject defaultSelectedUIItem;
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;
        
        private void OnEnable()
        {
            instancePrefab.gameObject.SetActive(false);
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
            eventSystem = EventSystem.current;
            base.Open(direction, menuHandler);
            eventSystem = EventSystem.current;
            localPlayerManager = GameManager.singleton.localPlayerManager;
            ClientManager.OnPlayerCountChanged += WhenClientPlayerCountChanged;
            sessionManagerGamemode.OnGamemodeStateChanged += WhenGamemodeStateChanged;
            sessionManagerGamemode.OnClientDefinitionsChanged += UpdatePlayerInfo;
            sessionManagerGamemode.OnLobbySettingsChanged += UpdateLobbyInfo;
            sessionManagerGamemode.OnGamemodeSettingsChanged += UpdateLobbyInfo;

            gameObject.SetActive(true);
            
            //GameManager.singleton.controllerAssignmentMenu.OnControllersAssigned += OnControllersAssigned;
            //GameManager.singleton.controllerAssignmentMenu.OpenMenu();
            //GameManager.singleton.localPlayerManager.SetPlayerCount(1);
            //GameManager.singleton.localPlayerManager.AutoAssignControllers();
            var playerRef = sessionManagerGamemode.Runner.LocalPlayer;
            ClientManager localClient = sessionManagerGamemode.Runner.GetPlayerObject(playerRef).GetBehaviour<ClientManager>();
            if (localClient.ClientPlayerAmount != GameManager.singleton.localPlayerManager.localPlayers.Count)
            {
                WhenLocalPlayerCountChanged(GameManager.singleton.localPlayerManager,
                    GameManager.singleton.localPlayerManager.localPlayers.Count);
            }
            else
            {
                WhenClientPlayerCountChanged(localClient);
            }
            //OnControllersAssigned();
        }

        private async void WhenGamemodeStateChanged(SessionManagerGamemode sessionmanager, SessionGamemodeStateType previous)
        {
            switch (sessionmanager.SessionState)
            {
                case SessionGamemodeStateType.LOADING_GAMEMODE:
                    await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("MainMenu"));
                    break;
                case SessionGamemodeStateType.LOBBY:
                    Debug.Log("Should load MainMenu.");
                    break;
            }
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            GameManager.singleton.controllerAssignmentMenu.OnControllersAssigned -= OnControllersAssigned;
            for (int i = 0; i < menuInstances.Count; i++)
            {
                menuInstances[i].Cleanup();
                Destroy(menuInstances[i].gameObject);
            }
            menuInstances.Clear();
            
            ClientManager.OnPlayerCountChanged -= WhenClientPlayerCountChanged;
            if (sessionManagerGamemode)
            {
                sessionManagerGamemode.OnClientDefinitionsChanged -= UpdatePlayerInfo;
                sessionManagerGamemode.OnLobbySettingsChanged -= UpdateLobbyInfo;
                sessionManagerGamemode.OnGamemodeSettingsChanged -= UpdateLobbyInfo;
            }
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
            return true;
        }

        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(eventSystem, localPlayerManager.systemPlayer))
            {
                eventSystem.SetSelectedGameObject(defaultSelectedUIItem);
            }
        }

        public async UniTask StartMatch()
        {
            bool startResult = await sessionManagerGamemode.TryStartMatch();
            if (!startResult) return;
            ClientManager.OnPlayerCountChanged -= WhenClientPlayerCountChanged;
            sessionManagerGamemode.OnClientDefinitionsChanged -= UpdatePlayerInfo;
            sessionManagerGamemode.OnLobbySettingsChanged -= UpdateLobbyInfo;
            sessionManagerGamemode.OnGamemodeSettingsChanged -= UpdateLobbyInfo;
            
            
        }

        public async void ExitLobby()
        {
            sessionManagerGamemode.LeaveSession();
            currentHandler.Back();
        }

        LobbyMenuInstance InitializeMenuInstance(int playerID)
        {
            LobbyMenuInstance instance = GameObject.Instantiate(instancePrefab, instanceParent, false);
            instance.canvas.worldCamera = GameManager.singleton.localPlayerManager.localPlayers[playerID].camera;
            instance.playerID = playerID;
            instance.Initialize(this);
            instance.gameObject.SetActive(true);
            return instance;
        }
        
        private void OnControllersAssigned(ControllerAssignmentMenu menu)
        {
            GameManager.singleton.controllerAssignmentMenu.CloseMenu();
            WhenLocalPlayerCountChanged(GameManager.singleton.localPlayerManager, GameManager.singleton.localPlayerManager.localPlayers.Count);
        }
        
        [SerializeField] private Camera lobbyPlayerCameraPrefab;
        private void WhenLocalPlayerCountChanged(LocalPlayerManager localplayermanager, int currentplaycount)
        {
            var playerRef = sessionManagerGamemode.Runner.LocalPlayer;
            ClientManager localClient = sessionManagerGamemode.Runner.GetPlayerObject(playerRef).GetBehaviour<ClientManager>();
            localClient.CLIENT_SetPlayerCount((uint)currentplaycount);
        }

        private void WhenClientPlayerCountChanged(ClientManager manager)
        {
            if (manager.Runner != sessionManagerGamemode.Runner) return;
            
            LocalPlayerManager localplayermanager = GameManager.singleton.localPlayerManager;
            int currentplaycount = localplayermanager.localPlayers.Count;

            if (manager.ClientPlayerAmount != currentplaycount)
            {
                localplayermanager.SetPlayerCount((int)manager.ClientPlayerAmount);
                currentplaycount = (int)manager.ClientPlayerAmount;
            }
            
            manager.profiles.Add(ProfilesManager.defaultProfileIdentifier);
            
            while (menuInstances.Count > currentplaycount)
            {
                int i = menuInstances.Count;
                menuInstances[i].Cleanup();
                Destroy(menuInstances[i]);
                menuInstances.RemoveAt(i);
            }
            while(menuInstances.Count < currentplaycount) menuInstances.Add(null);
            
            for (int i = 0; i < currentplaycount; i++)
            {
                if (localplayermanager.localPlayers[i].camera == null)
                {
                    var temp = localplayermanager.localPlayers[i];
                    temp.camera = Instantiate(lobbyPlayerCameraPrefab, Vector3.zero, Quaternion.identity);
                    temp.camera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
                    temp.camera.transform.position = mainMenuCamera.transform.position;
                    temp.camera.transform.rotation = mainMenuCamera.transform.rotation;
                    localplayermanager.localPlayers[i] = temp;
                }
                if(menuInstances[i] == null) menuInstances[i] = InitializeMenuInstance(i);
                menuInstances[i].canvas.planeDistance = 0.5f;
            }
            
            localplayermanager.ApplyCameraLayout();
            if(localplayermanager.systemPlayer.camera) localplayermanager.systemPlayer.camera.enabled = false;
            
            UpdateLobbyInfo(sessionManagerGamemode);
            UpdatePlayerInfo();
        }
        
        private void UpdateLobbyInfo(SessionManagerGamemode sessionManager)
        {
            for (int i = 0; i < menuInstances.Count; i++)
            {
                //menuInstances[i].FillGamemodeOptions(this);
            }
        }

        private void UpdatePlayerInfo()
        {
            for (int i = 0; i < menuInstances.Count; i++)
            {
                menuInstances[i].Refresh();
            }
        }

        private void UpdatePlayerInfo(SessionManagerGamemode sessionManager)
        {
            UpdatePlayerInfo();
        }
    }
}