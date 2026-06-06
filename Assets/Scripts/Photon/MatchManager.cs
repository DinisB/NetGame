namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;
    using Unity.Cinemachine;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using TMPro;

    public class MatchManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private Transform[] spawnPoint;
        private IList<GameObject> players = new List<GameObject>();
        [SerializeField] private TextMeshProUGUI redScoreText;
        [SerializeField] private TextMeshProUGUI blueScoreText;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private TextMeshProUGUI winText;
        private static readonly int maxBalls = 80;
        private int redScore;
        private int blueScore;

        [SerializeField] private TextMeshProUGUI timerText;
        private float matchTime = 120f;

        void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                TrySpawnPlayer();
            }

            redScore = 40;
            blueScore = 40;
            UpdateScore();
            StartCoroutine(CountdownTimer());
        }

        private IEnumerator CountdownTimer()
        {
            while (PhotonNetwork.PlayerList.Length < 2)
            {
                yield return null;
                timerText.text = "à espera do 2º player";
            }
            while (matchTime > 0)
            {
                timerText.text = TimeSpan.FromSeconds(matchTime).ToString(@"mm\:ss");
                yield return new WaitForSeconds(1f);

                if (PhotonNetwork.IsMasterClient)
                {
                    matchTime -= 1f;
                }
            }
            timerText.text = "0";

            photonView.RPC("RPC_EndMatch", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_EndMatch()
        {
            for (int i = players.Count - 1; i >= 0; i--)
                players[i].SetActive(false);

            winScreen.SetActive(true);
            string winner = redScore > blueScore ? $"Red + {redScore}" : blueScore > redScore ? $"Blue + {blueScore}" : "Empate";
            winText.text = "Vencedor: \n" + winner;
        }

        public void BackToMenu()
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Login");
        }

        private void TrySpawnPlayer()
        {
            GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint[PhotonNetwork.CurrentRoom.PlayerCount - 1].position,
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
            photonView.RPC("RPC_AddScore", RpcTarget.All, (int)team);
        }

        public void RemoveScore(Team team)
        {
            photonView.RPC("RPC_RemoveScore", RpcTarget.All, (int)team);
        }

        public void Destroy(int viewID)
        {
            photonView.RPC("RPC_Destroy", RpcTarget.All, viewID);
        }

        [PunRPC]
        public void RPC_Destroy(int viewID)
        {
            GameObject obj = PhotonView.Find(viewID).gameObject;
             Destroy(obj);
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

        public void SpawnBall(Team team, Vector2 position, float side)
        {
            photonView.RPC("RPC_SpawnBall", RpcTarget.MasterClient, (int)team, position, side);
        }

        [PunRPC]
        public void RPC_SpawnBall(Team team, Vector2 position, float side)
        {
            object[] initData = new object[] { team, side };
            float sideX = side > 0 ? -1f : 1f;
            position.x += sideX;
            PhotonNetwork.Instantiate("Ball", position, Quaternion.identity, 0, initData);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(redScore);
                stream.SendNext(blueScore);
                stream.SendNext(matchTime);
            }
            else
            {
                redScore = (int)stream.ReceiveNext();
                blueScore = (int)stream.ReceiveNext();
                matchTime = (float)stream.ReceiveNext();
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