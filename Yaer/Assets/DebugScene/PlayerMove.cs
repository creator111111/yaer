using UnityEngine;

namespace DebugScene
{
    public class PlayerMove : MonoBehaviour
    {
        private static readonly int walk = Animator.StringToHash("Walk");
        public Vector2 velocity;

        public float xSpeed = 10f;
        public float parameter = 1f;
        public float axis;
        private Animator animator;
        private float lastX;

        private Rigidbody2D rg;

        // Start is called before the first frame update
        private void Start()
        {
            animator = GetComponent<Animator>();
            rg = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        private void Update()
        {
            // 速度控制移动
            axis = Input.GetAxisRaw("Horizontal");
            // 动画控制
            if (axis != 0)
            {
                velocity = new Vector2(axis * xSpeed * parameter, rg.velocity.y);

                if (axis > 0)
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                else if (axis < 0) transform.rotation = Quaternion.Euler(0, -180, 0);
            }
            else
            {
                velocity = new Vector2(0, 0);
            }

            rg.velocity = velocity;

            if (rg.velocity.x != 0)
                animator.SetBool(walk, true);
            else
                animator.SetBool(walk, false);
        }

        private void Walk0()
        {
            // if (lastX == 0)
            // {
            //     
            // }
            lastX = transform.position.x;
            // Debug.Log("1: " + lastX);
        }

        private void Walk3()
        {
            Debug.Log("3: " + (transform.position.x - lastX));
            lastX = transform.position.x;
        }

        private void Walk7()
        {
            Debug.Log("7: " + (transform.position.x - lastX));
            lastX = transform.position.x;
        }

        private void Print(int count)
        {
            Debug.Log(count + ": " + (transform.position.x - lastX));
            lastX = transform.position.x;
        }
    }
}