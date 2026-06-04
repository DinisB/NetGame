namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;
    using Photon.Realtime;
    using System.Collections.Generic;
    using TMPro;

    public class LobbyScreen : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_InputField roomNameInput;

        private bool _pendingHost = false;
        private bool _pendingJoin = false;
        private string _pendingRoomName = "";
        private List<RoomInfo> _cachedRooms = new List<RoomInfo>();

        void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.SendRate = 60;
            PhotonNetwork.SerializationRate = 60;
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("Lobby pronto");

            if (_pendingHost)
            {
                _pendingHost = false;
                CreateRoom(_pendingRoomName);
            }
            else if (_pendingJoin)
            {
                _pendingJoin = false;
                PhotonNetwork.JoinRoom(_pendingRoomName);
            }
        }

        public void HostGame()
        {
            string roomName = "";
            for (int i = 0; i < 5; i++)
                roomName += Random.Range(0, 9).ToString();

            Debug.Log("Sala " + roomName);
            _pendingRoomName = roomName;

            if (PhotonNetwork.InLobby)
                CreateRoom(roomName, false);
            else
            {
                _pendingHost = true;
                if (!PhotonNetwork.IsConnected)
                    PhotonNetwork.ConnectUsingSettings();
            }
        }

        public void Matchmaking()
        {
            if (PhotonNetwork.InLobby)
            {
                foreach (RoomInfo room in _cachedRooms)
                {
                    if (room.PlayerCount == 1 && room.IsVisible == true)
                    {
                        PhotonNetwork.JoinRoom(room.Name);
                        return;
                    }
                }
                PhotonNetwork.CreateRoom(Random.Range(0, 100000).ToString("D5"), new RoomOptions { MaxPlayers = 2, IsVisible = true });
            }
            else
            {
                _pendingJoin = true;
                if (!PhotonNetwork.IsConnected)
                    PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            _pendingJoin = false;
            _pendingHost = false;
            _pendingRoomName = "";

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                return;
            }

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

            roomNameInput.text = "";
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _cachedRooms = roomList;
        }

        public void JoinGame()
        {
            if (roomNameInput.text == "")
            {
                Debug.Log("Escreve o código da sala");
                return;
            }
            if (roomNameInput.text.Length != 5)
            {
                Debug.Log("Sala tem de ter 5 números");
                return;
            }

            _pendingRoomName = roomNameInput.text;

            if (PhotonNetwork.InLobby)
                PhotonNetwork.JoinRoom(_pendingRoomName);
            else
            {
                _pendingJoin = true;
                if (!PhotonNetwork.IsConnected)
                    PhotonNetwork.ConnectUsingSettings();
            }
        }

        private void CreateRoom(string roomName, bool isVisible = true)
        {
            RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = isVisible };
            PhotonNetwork.CreateRoom(roomName, options);
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LoadLevel("Game");
        }

        void Update()
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.SendAllOutgoingCommands();
            }
        }
    }
}