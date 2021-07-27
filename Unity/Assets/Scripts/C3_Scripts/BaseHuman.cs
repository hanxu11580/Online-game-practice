using UnityEngine;

namespace C3
{

    public class BaseHuman : MonoBehaviour
    {
        protected bool isMoving = false;

        private Vector3 targetPos;

        public float speed = 1;

        public string desc = "";

        public float limitHei;

        protected void Start()
        {
            limitHei = transform.position.y;
        }

        protected void Update()
        {
            MoveUpdate();
        }

        public void MoveTo(Vector3 pos)
        {
            pos.y = limitHei;
            targetPos = pos;
            isMoving = true;
        }

        public void MoveUpdate()
        {
            if (!isMoving) return;

            Vector3 currPos = transform.position;
            transform.position = Vector3.MoveTowards(currPos, targetPos, Time.deltaTime * speed);
            transform.LookAt(targetPos);
            if (Vector3.Distance(targetPos, transform.position) < 0.05f)
            {
                isMoving = false;
            }
        }
    }
}