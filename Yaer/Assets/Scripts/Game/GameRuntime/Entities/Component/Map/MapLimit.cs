using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Map
{
    public class MapLimit : MonoBehaviour
    {
        [SerializeField] private EdgeCollider2D edgeCld;
        [SerializeField] private PolygonCollider2D polygonCld;

        [Header("MapAirWallIndex")] [SerializeField]
        private int startIndex;

        [SerializeField] private int endIndex;
        [SerializeField] private float targetHeight;

        public int StartIndex => startIndex;
        public int EndIndex => endIndex;
        public float TargetHeight => targetHeight;
        public PolygonCollider2D PolygonCld => polygonCld;
        public EdgeCollider2D EdgeCld => edgeCld;

        private void Awake()
        {
            Init();
        }

        private void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            edgeCld = GetComponent<EdgeCollider2D>();
            polygonCld = GetComponent<PolygonCollider2D>();

            if (edgeCld == null) edgeCld = gameObject.AddComponent<EdgeCollider2D>();
            if (polygonCld == null) polygonCld = gameObject.AddComponent<PolygonCollider2D>();
        }
    }
}