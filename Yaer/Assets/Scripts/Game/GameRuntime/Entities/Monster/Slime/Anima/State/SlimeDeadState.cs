using Game.GameRuntime.Entities.Component.CldController;
using Game.GameRuntime.Entities.Monster.TenWan;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    /// <summary>
    /// ʷ��ķ����̬��������ս����󶨸�ʬ�壬�� Dead �����������ߵ���/�Ƴ���
    /// <para>
    /// 0913 ������ʬ��FreezeAll + kinematic������������������ Gravity=-100 ק�����⣩��
    /// 0913 ʬ�����ᣨ��������Enter <b>��</b> <see cref="SnapToCombatAxisY"/>��Ȩ������ CombatAxisY=-6.61����
    /// <b>��</b> FreezeAll���ɽ��Dead ��ֹ Snap����Ե�ʱ��ʵʱ��� y���������������ʱ�����Ʒ���
    /// �������/��ɱ/������������ᶳ��ƫ�� Y����ʬ�ߵʹ��䡣
    /// �����������ʱ�� Snap����©�����������㣨��Ʒ�������
    /// ��ֹ��ȥ�� FreezeAll���ع��������Snap �Ļ����ʵʱ y���ָ� 0723 GroundCld ʵ�ġ�
    /// </para>
    /// </summary>
    public class SlimeDeadState : BaseSlimeState
    {
        public override void Enter()
        {
            base.Enter();
            slime.isProtect = false; // ʷ��ķʬ�����ó��ܱ�����
            slime.componentSystem.GetComponent<CldControllerComponent>().SetActiveAll(true);

            // ���� 0913 ʬ�������������᣺������ Snap �ٶ� ����
            // Idle.Exit �ѽⶳ Y������/������ʱ transform.y �� �� ?6.61���� Snap �� FreezeAll �ᶤƫ��
            SnapToCombatAxisY(alsoClearVerticalVelocity: true);

            // ���� 0913 ����������ʬ ����
            if (moveCpn != null)
            {
                moveCpn.StopMove();       // �� Velocity / rg.velocity����������ڶ�ǰ��Ųһ֡
                moveCpn.canGravity = false;
            }

            var bodyRg = slime.BodyRg;
            if (bodyRg != null)
            {
                bodyRg.velocity = Vector2.zero;
                bodyRg.constraints = RigidbodyConstraints2D.FreezeAll;
                bodyRg.isKinematic = true; // ������/ľ��һ�£�ʬ�岻������������
            }
        }

        public override void Update()
        {
            base.Update();
            if (IsFinished)
            {
                slime.isProtect = true; // ��������������������޵�
                slime.componentSystem.GetComponent<CldControllerComponent>().SetActiveAll(false);
                if (FirstMeetSlimeGuideStoryMgr.getInstance().hasInCurStory)
                {
                    FirstMeetSlimeGuideStoryMgr.getInstance().CheckEventHasEnd();
                }
            }
        }
    }
}
