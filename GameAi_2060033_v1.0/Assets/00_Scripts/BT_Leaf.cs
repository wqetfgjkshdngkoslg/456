/* BT_Leaf
 *  - 행동 트리의 가장 마지막에 위치하는 노드로 실제 행동이나 조건 검사를 담당하는 실행자 역할
 *  - 예 : "공격하라" , "플레이어가 범위 내에 있는가?"와 같은 구체적인 로직을 수행
 *  - Leaf(리프) 노드는 행동 트리의 가장 말단에 위치하는 노드
 *  - 실제 행동 또는 조건을 수행하는 역할
 *  - BT_Leaf는 델리게이트(System.Func<BT_NodeStatus>)에 원하는 함수를 넘겨
 *  - 해당 함수를 Evaluate()에서 호출하는 방식으로 동작
 */
using UnityEngine;

/* 메소드를 담는 변수 (델리게이트)Func
 *  - System.Func는 c#에서 미리 만들어놓은 '메소드를 담는 변수(델리게이트)'
 *  - System.Func<BT_NodeStatus> : 반환형이 BT_NodeStatus인 메소드 레퍼런스를 저장
 *  - 예시 : "System.Func<BT_NodeStatus>"
 *  - Func : "나는 지금부터 메소드를 담을 거야" 라는 선언
 *  - <BT_NodeStatus>:"내가 담을 메소드는 반드시 BT_NodeStatus라는 타입을 반환 해야해"라는 규칙을 의미
 * 
 */
public class BT_Leaf : BT_Node // BT_Node 상속
{
    // Func 델리게이트를 사용해 '메소드' 자체를 저장하는 변수
    // BT_NodeStatus(성공, 실패, 실행 중)을 반환하는 어떤 메소드든 담을 수 있는 변수
    // 이 변수는 노드가 수행할 실제 행동의 내용의 메소드를 담는다
    private System.Func<BT_NodeStatus> action;

    // 생성자 : Leaf 노드를 만들 때 어떤 "행동/조건 함수"를 수행할 지 외부에서 주입
    // 매개변수 : 'action'에 들어온 함수를 이 노드의 내부 변수에 저장
    public BT_Leaf(System.Func<BT_NodeStatus> action)
    {
        this.action = action;
    }

    /* Leaf 노드의 Evaluate 메소드 오버라이드
     * - 생성자를 통해 action 변수에 저장해 두었던 메소드를 그대로 호출하고,
     *   그 메소드가 반환하는 BT_NodeStatus 값을 상위 노드에서 전달하는 역할
     * - 호출만 담당하므로 심플하고 재사용성이 높음
     * - Evaluate(): 이 노드가 1프레임 동안 할 일을 실행하고 결과 상태를 반환
     * - Leaf는 담아둔 함수를 그대로 호출해 그 반환값을 상위 노드에게 전달
     */
    public override BT_NodeStatus Evaluate()
    {
        return action();
    }
}
