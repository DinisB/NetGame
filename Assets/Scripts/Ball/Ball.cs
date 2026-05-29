namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;

    public class Ball : MonoBehaviourPun, IPunObservable
    {
        [SerializeField]
        private Team team;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private float side;
        private Vector2 _netPosition;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            TeamColor();
            if (!photonView.IsMine)
                rb.bodyType = RigidbodyType2D.Kinematic;
        }

        void Update()
        {
            if (!photonView.IsMine)
                rb.MovePosition(Vector2.Lerp(rb.position, _netPosition, 0.2f));
        }

        // Update is called once per frame
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void Launch()
        { rb.AddForce(new Vector2(side * 8, Random.Range(8f, 25f)), ForceMode2D.Impulse); }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(rb.position);
                stream.SendNext(rb.linearVelocity);
                stream.SendNext(team);
            }
            else
            {
                _netPosition = (Vector2)stream.ReceiveNext();
                Vector2 remoteVelocity = (Vector2)stream.ReceiveNext();
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                _netPosition += remoteVelocity * lag;
                team = (Team)stream.ReceiveNext();
                TeamColor();
            }
        }

        protected void TeamColor()
        {
            if (team == Team.Red)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Balls/ballred");
            }
            else if (team == Team.Blue)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Balls/ballblu");
            }
        }

        public void Side(float side)
        {
            if (side > 0)
            {
                this.side = -1;
            }
            else
            {
                this.side = 1;
            }
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            TeamColor();
        }

        public Team GetTeam()
        {
            return team;
        }
    }
}