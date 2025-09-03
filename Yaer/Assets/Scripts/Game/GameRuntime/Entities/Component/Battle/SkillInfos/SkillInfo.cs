using Game.GameRuntime.Entities.Component.Battle.Attack;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.SkillInfos
{
    public enum SkillShapeType
    {
        Circle,
        Rectangle,
        Triangle
    }

    public struct SkillShapeCircleData
    {
        public float radius;
        public Vector3 center;
    }

    public struct SkillShapeRectangleData
    {
        public float width;
        public float height;
        public Vector3 center;
    }

    public struct SkillShapeTriangleData
    {
        public float baseLength;
        public float triangleHeight;
        public Vector3 center;
    }

    public class SkillInfo : MonoBehaviour
    {
        public string skillName;
        public SkillShapeType shapeType;
        public AttackData data;

        // Attack Shape Parameters
        public float radius; // For Circle
        public float width; // For Rectangle
        public float height; // For Rectangle
        public float baseLength; // For Triangle
        public float triangleHeight; // For Triangle

#if UNITY_EDITOR
        public bool openGizmos;
        private void OnDrawGizmos()
        {
            if (openGizmos == false)
            {
                return;
            }

            switch (shapeType)
            {
                case SkillShapeType.Circle:
                    Gizmos.DrawWireSphere(transform.position, radius);
                    break;
                case SkillShapeType.Rectangle:
                    Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
                    break;
                case SkillShapeType.Triangle:
                    var p1 = transform.position + new Vector3(-baseLength / 2, triangleHeight, 0);
                    var p2 = transform.position + new Vector3(baseLength / 2, triangleHeight, 0);

                    Gizmos.DrawLine(p1, p2);

                    p1 = transform.position + new Vector3(-baseLength / 2, -triangleHeight, 0);
                    p2 = transform.position + new Vector3(baseLength / 2, -triangleHeight, 0);

                    Gizmos.DrawLine(p1, p2);
                    break;
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(skillName))
            {
                skillName = gameObject.name;
            }
        }
#endif

        public object GetSkillShapeData()
        {
            switch (shapeType)
            {
                case SkillShapeType.Circle:
                    return new SkillShapeCircleData
                    {
                        radius = radius,
                        center = transform.position
                    };
                case SkillShapeType.Rectangle:
                    return new SkillShapeRectangleData
                    {
                        width = width,
                        height = height,
                        center = transform.position
                    };
                case SkillShapeType.Triangle:
                    return new SkillShapeTriangleData
                    {
                        baseLength = baseLength,
                        triangleHeight = triangleHeight,
                        center = transform.position
                    };
                default:
                    Debug.LogError("未知的攻击形状类型");
                    return null;
            }
        }
    }
}