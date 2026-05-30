namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;
    using Photon.Realtime;
    using TMPro;

    public class LobbyScreen : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_InputField roomNameInput;

        private bool _pendingHost = false;
        private bool _pendingJoin = false;
        private string _pendingRoomName = "";

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
                CreateRoom(roomName);
            else
            {
                _pendingHost = true;
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

        private void CreateRoom(string roomName)
        {
            RoomOptions options = new RoomOptions { MaxPlayers = 2 };
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