/* BT_Selector
 *  - 여러 자식 노드 중 하나라도 성공할 때까지 순서대로 실행하는 'QR' 논리 노드
 *  - 자식 중 하나가 성공하면 즉시 자신도 성공을 반환하고 모든 자식이 실패해야만 자신도 실패를 반환
 *  - Selector 노드는 "자식 노드들을 순서대로" 평가하며, 하나라도 Success가 나오면 즉시 Success를 반환함.
 *  - 모든 자식이 Failure일 때만 Failure를 반환
 *  - 자식 중 하나가 Running 이면 즉시 Running을 반환해 진행 중임을 알림
 * 
 */

using UnityEngine;
using System.Collections.Generic; // List 사용을 위한 네임스페이스

public class BT_Selector : BT_Node // BT_Node 상속
{
    private List<BT_Node> lstChildren; // Selector가 관리할 자식 노드들을 담는 리스트

    // 생성자 : 외부에서 자식 노드 리스트를 받아 내부에 저장
    public BT_Selector(List<BT_Node> arglstChildren)
    {
        this.lstChildren = arglstChildren;
    }

    public override BT_NodeStatus Evaluate()
    {
        foreach (BT_Node node in lstChildren)
        {
            BT_NodeStatus status = node.Evaluate();

            if (status == BT_NodeStatus.Success ) // 하나라도 성공했다면, Selector는 즉시 성공
            {
                return BT_NodeStatus.Success;
            }

            if (status == BT_NodeStatus.Running)
            {
                return BT_NodeStatus.Running;
            }
        }

        return BT_NodeStatus.Failure; // 위의 반복에서 성공/진행중을 만나지 못했다면. 모든 자식이 실패
    }
}
