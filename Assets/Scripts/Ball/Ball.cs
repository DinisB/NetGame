namespace NetGame.Assets.Scripts
{
    using UnityEngine;

    public class Ball : MonoBehaviour
    {
        [SerializeField]
        private Team team;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private float side;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            TeamColor();
        }

        // Update is called once per frame
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void Launch()
        { rb.AddForce(new Vector2(side * 8, 10), ForceMode2D.Impulse); }

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