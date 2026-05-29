namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;
    using Unity.Cinemachine;
    using System.Collections.Generic;

    public class MatchManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private Transform spawnPoint;
        private IList<GameObject> players = new List<GameObject>();

        void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                TrySpawnPlayer();
            }
        }

        private void TrySpawnPlayer()
        {
            GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            Quaternion.identity
            );

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
    }
}