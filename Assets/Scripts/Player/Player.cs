namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;

    public class Player : MonoBehaviourPun, IPunObservable
    {
        /// <summary>
        /// Serve como um "Hub" para as diferentes componentes do cliente.
        /// </summary>
        [SerializeField]
        private Team team;


        [SerializeField] private CharacterData characterData;
        [SerializeField] private PlayerVisuals playerVisuals;
        [SerializeField] private BoxCollider2D triggerCol;
        [SerializeField] private PlayerMovement playerMovement;
        private MatchManager matchManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (photonView.IsMine)
            {
                matchManager = FindFirstObjectByType<MatchManager>();
            }
        }

        public void SetTeamRPC(Team team)
        {
            photonView.RPC("RPC_SetTeam", RpcTarget.AllBuffered, (int)team);
        }

        [PunRPC]
        public void RPC_SetTeam(int teamIndex)
        {
            team = (Team)teamIndex;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(team);
            }
            else
            {
                team = (Team)stream.ReceiveNext();
            }
        }

        public void OnDisconnectedFromServer()
        {
            if (photonView.IsMine)
            {
                matchManager.RemovePlayer(gameObject);
                PhotonNetwork.Destroy(gameObject);
            }
        }

        public CharacterData GetCharacterData()
        {
            return characterData;
        }

        public PlayerVisuals GetPlayerVisuals()
        {
            return playerVisuals;
        }

        public BoxCollider2D GetTriggerCol()
        {
            return triggerCol;
        }

        public PlayerMovement GetPlayerMovement()
        {
            return playerMovement;
        }

        public Team GetTeam()
        {
            return team;
        }

        public void SetTeam(Team team)
        {
            this.team = team;
        }

        public void AddBall()
        {
            matchManager.AddScore(team);
        }

        public void RemoveBall()
        {
            matchManager.RemoveScore(team);
        }

        public MatchManager GetMatchManager()
        {
            return matchManager;
        }
    }
}