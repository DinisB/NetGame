namespace NetGame.Assets.Scripts
{
    using Photon.Pun;
    using UnityEngine;
    using System.Collections;

    public class PlayerCollision : MonoBehaviour
    {
        [SerializeField] private bool hurt = false; // serialize para debug
        private GameObject ballPrefab;
        private int ballLayer;

        private Player player;
        private PlayerMovement playerMovement;

        private Coroutine resetHurtCoroutine;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            player = GetComponent<Player>();
            playerMovement = player.GetPlayerMovement();
            ballPrefab = Resources.Load<GameObject>("Ball");
            ballLayer = LayerMask.NameToLayer("Ball");
        }

        void Update()
        {
            if (!player.photonView.IsMine) return;
            DetectAttack();
        }

        void DetectAttack()
        {
            Collider2D hit = Physics2D.OverlapBox(player.GetTriggerCol().bounds.center, player.GetTriggerCol().bounds.size, 0f, LayerMask.GetMask("Attack"));

            if (hit != null && !hurt && !playerMovement.IsDefending() && !playerMovement.IsRolling())
            {
                if (player.GetTeam() == hit.GetComponentInParent<Player>().GetTeam())
                {
                    return;
                }
                hurt = true;
                Debug.Log("Player hit!");
                if (player.GetBalls() > 0)
                {
                    GameObject ball = PhotonNetwork.Instantiate("Ball", transform.position, Quaternion.identity);
                    ball.GetComponent<Ball>().SetTeam(player.GetTeam());
                    ball.GetComponent<Ball>().Side(hit.transform.position.x - transform.position.x);
                    ball.GetComponent<Ball>().Launch();
                    player.RemoveBall();
                }
                if (resetHurtCoroutine != null)
                    StopCoroutine(resetHurtCoroutine);
                resetHurtCoroutine = StartCoroutine(ResetHurt());
                playerMovement.SetCanMove(false);
                playerMovement.HurtJump(hit.transform.position.x - transform.position.x);
            }
        }

        private IEnumerator ResetHurt()
        {
            yield return new WaitForSeconds(.5f);
            hurt = false;
            playerMovement.SetCanMove(true);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == ballLayer)
            {
                Ball ball = collision.gameObject.GetComponent<Ball>();
                if (ball == null) return;

                if (!hurt && !player.GetPlayerMovement().CheckIfIAmASandBag())
                {
                    Debug.Log(collision.gameObject.GetComponent<Ball>().GetTeam() != player.GetTeam() ? "Got a ball!" : "Recovered your ball!");
                    PhotonNetwork.Destroy(collision.gameObject);
                    player.AddBall();
                }
            }
        }
    }
}