namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;

    public class Player : MonoBehaviourPun, IPunObservable
    {
        [SerializeField]
        private Team team;
        [SerializeField] private int balls;
        private static readonly int maxBalls = 200;

        [SerializeField] private CharacterData characterData;
        [SerializeField] private PlayerVisuals playerVisuals;
        [SerializeField] private BoxCollider2D triggerCol;
        [SerializeField] private PlayerMovement playerMovement;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (photonView.IsMine)
            {
                balls = 100;
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
                stream.SendNext(balls);
            }
            else
            {
                balls = (int)stream.ReceiveNext();
            }
        }

        public void OnDisconnectedFromServer()
        {
            if (photonView.IsMine)
            {
                FindFirstObjectByType<MatchManager>().RemovePlayer(gameObject);
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
            balls = Mathf.Clamp(balls + 1, 0, maxBalls);
        }

        public void RemoveBall()
        {
            if (balls > 0)
            {
                balls = Mathf.Clamp(balls - 1, 0, maxBalls);
            }
        }

        public int GetBalls()
        {
            return balls;
        }
    }
}