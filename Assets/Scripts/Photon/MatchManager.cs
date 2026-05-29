namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;
    using Unity.Cinemachine;
    using System.Collections.Generic;
    using TMPro;

    public class MatchManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private Transform spawnPoint;
        private IList<GameObject> players = new List<GameObject>();
        [SerializeField] private TextMeshProUGUI redScoreText;
        [SerializeField] private TextMeshProUGUI blueScoreText;
        private static readonly int maxBalls = 200;
        private int redScore;
        private int blueScore;

        void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                TrySpawnPlayer();
            }

            redScore = 100;
            blueScore = 100;
            UpdateScore();
        }

        private void TrySpawnPlayer()
        {
            GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            Quaternion.identity
            );

            players.Add(player);

            Team assignedTeam = (PhotonNetwork.CurrentRoom.PlayerCount % 2 == 1) ? Team.Red : Team.Blue;
            player.GetComponent<Player>().SetTeamRPC(assignedTeam);

            GameObject camera = FindFirstObjectByType<CinemachineCamera>().gameObject;

            if (player.GetComponent<PhotonView>().IsMine)
            {
                camera.GetComponent<CinemachineCamera>().Target.TrackingTarget = player.transform;
            }
        }

        public void RemovePlayer(GameObject player)
        {
            players.Remove(player);
        }

        public int GetScore(Team team)
        {
            return team == Team.Red ? redScore : blueScore;
        }

        public void AddScore(Team team)
        {
            photonView.RPC("RPC_AddScore", RpcTarget.AllBuffered, (int)team);
        }

        public void RemoveScore(Team team)
        {
            photonView.RPC("RPC_RemoveScore", RpcTarget.AllBuffered, (int)team);
        }

        [PunRPC]
        public void RPC_AddScore(int teamIndex)
        {
            Team team = (Team)teamIndex;
            if (team == Team.Red)
            {
                redScore = Mathf.Clamp(redScore + 1, 0, maxBalls);
            }
            else
            {
                blueScore = Mathf.Clamp(blueScore + 1, 0, maxBalls);
            }
            UpdateScore();
        }

        [PunRPC]
        public void RPC_RemoveScore(int teamIndex)
        {
            Team team = (Team)teamIndex;
            if (team == Team.Red)
            {
                redScore = Mathf.Clamp(redScore - 1, 0, maxBalls);
            }
            else
            {
                blueScore = Mathf.Clamp(blueScore - 1, 0, maxBalls);
            }
            UpdateScore();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(redScore);
                stream.SendNext(blueScore);
            }
            else
            {
                redScore = (int)stream.ReceiveNext();
                blueScore = (int)stream.ReceiveNext();
                UpdateScore();
            }
        }

        public void UpdateScore()
        {
            redScoreText.text = redScore.ToString();
            blueScoreText.text = blueScore.ToString();
        }
    }
}