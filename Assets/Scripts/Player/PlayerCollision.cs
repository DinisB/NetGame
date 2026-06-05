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
        private PlayerVisuals playerVisuals;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            player = GetComponent<Player>();
            playerMovement = player.GetPlayerMovement();
            playerVisuals = player.GetPlayerVisuals();
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
            Collider2D hit = Physics2D.OverlapBox(player.GetTriggerCol().bounds.center,
            player.GetTriggerCol().bounds.size, 0f, LayerMask.GetMask("Attack"));

            if (hit != null && !hurt && !playerMovement.IsDefending() && !playerMovement.IsRolling())
            {
                if (player.GetTeam() == hit.GetComponentInParent<Player>().GetTeam())
                {
                    return;
                }
                hurt = true;
                playerVisuals.GetAnimator().SetBool("Hurt", true);
                Debug.Log("Player hit!");
                if (player.GetMatchManager().GetScore(player.GetTeam()) > 0)
                {
                    player.GetMatchManager().SpawnBall(player.GetTeam(), transform.position, hit.transform.position.x - transform.position.x);
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
            playerVisuals.GetAnimator().SetBool("Hurt", false);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!player.photonView.IsMine) return;

            if (collision.gameObject.layer == ballLayer)
            {
                Ball ball = collision.gameObject.GetComponent<Ball>();
                if (ball == null) return;

                if (!hurt && !player.GetPlayerMovement().CheckIfIAmASandBag())
                {
                    Debug.Log(collision.gameObject.GetComponent<Ball>().GetTeam() != player.GetTeam()
                        ? "Apanhaste uma bola" : "Recuperaste uma bola");

                    player.GetMatchManager().Destroy(collision.gameObject.GetComponent<PhotonView>().ViewID);
                    player.AddBall();
                }
            }
        }
    }
}