/* 1. Behavior Tree 행동 트리 개념
 *  게임 AI가 어떤 행동을 어떤 순서로 수행할지를 트리 구조로 표현한 시스템
 *  Behavior Tree 는 게임 AI에서 널리 사용되는 계층적 의사 결정 구조 
 *  루트 노드에서 시작하여 조건과 제어 흐름 을 통해 분기하고 
 *  최종적으로 행동을 수행하는 방식
 *  트리 형태로 구성되면 Sequence,  Selector, Decorator 같은 노드들이 결합되어 
 *  복잡한 AI 의사결정을 단순한 규칙의 조합으로 표현 
 *  
 *  
 *  2 BT 구성 요소
 *   루트 : 트리 탐색의 시작점
 *   복합노드 : 자식 노드들을 제어 
 *   데코레이터(Decorator) : 하나의 자식 노드만 제어 
 *   리프 노드(Leaf Node) : 실제 행동이나 조건 검사 수행 
 *   
 *   
 *   3. BT 의 3가지 핵심 내부 노드 : 이 각각의 노드들은 조건을 평가하거나, 행동을 수행하거나, 트리의 흐름을 따라 최종 행동을 결정함
 *    - 시퀀스 : 순차 실행자로써,  자식 노드들을 순서대로 실행하며, 모두 성공해야 최종 성공을 반환
 *      자식 노드를 왼쪽부터 순서대로 실행을 하면서 중간에 하나라도 실패하면 실패를 반환
 *      즉, 모든 자식 노드가 모두 성공해야 성공을 반환하는 노드 
 *    - 선택자 : 자식 노드들을 순서대로 평가하며, 하나라도 성공하면 즉시 성공을 반환
 *      시퀀스 노드와 반대적인 면모가 있음
 *    - 리프 : 행동 또는 조건을 실제로 수행하는 가장 말단 노드 
 *      실제 AI 행동과 판단이 일어나고 예를 들어 공격하거나 이동을 하거나 대기하거나 이런식으로 실제 노드에 대한 행동이 파악
 *      
 *   
 *   4 Why Behavior Tree ?
 *    - 모듈화와 재사용성
 *    - 같은 행동 노드를 여러 캐릭터가 공유 가능 -> 유지보수성과 확장성이 뛰어남
 *    - 시각적 구조화 
 *    - FSM보다 상태전이가 복잡하게 얽히지 않고, 읽기 쉬운 계층적 구조를 제공
 *    - 복잡한 행동 패턴 표현 용이 
 *    - 단순히 공격 <=> 추적 처럼 정적인 상태 전환이 아니라 조건기반선택 순차적 단계 
 *       조건 장식자 등을 이용해 유연하게 동작을 설계할 수 있음
 *    - 언리얼 엔진, UnityAssetStore 등에서도 Behavior Tree 기반 툴과 패키지가 보편적으로 제공됨
 *    
 *   5 Behavior Tree 단점
 *    - 초기 설계 난이도가 높음
 *    - 트리 구조를 처음 접하는 사람에게는 추상적이고 이해가 어려울 수 있음
 *    - 노드 수 증가 -> 복잡성 증가 
 *    - 대규모 AI 를 설계할 경우 트리 노드가 수백 개에 이를 수 있으며, 관리와 최적화가 어려워질 수 있음
 *    - Selector, Sequence의 실행 순서를 잘못 설계하면 의도와 다르게 행동할 수 있음
 *    
 *   6 요약 
 *    - BT는 모듈화, 시각화, 유연성 측면에서 게임AI 설계에 매우 강력한 도구임
 *    - FSM 보다 복잡한 행동을 체계적으로 관리할 수 있지만, 트리 구조 관리에 부담이 있음
 *    
 *   
 */

using System.Collections.Generic; // List 사용을 위한 네임스페이스
using UnityEditor.Animations; 
using UnityEngine;

// 지금까지 만든 Node, Leaf, Selector, Sequence 클래스를 조립하여 실제 적 AI의 행동 트리를 구성하고 실행하는 메인 스크립트
// 적의 상태(공격, 추적, 순찰, 대기)와 그에 따른 행동을 정의하는 클래스 스크립트
public class EnemyBT : MonoBehaviour
{
    /* 필드(멤버 변수) 정의하기
     * - root : 행동 트리의 가장 최상위 노드. 모든 로직은 이 root에서 시작됨
     * - animatorMonsterState : 적 캐릭터의 애니메이션을 제어하는 컴포넌트
     * - characterTarget : 추적하고 공격할 대상의 위치 정보
     * - fMonsterSpeed, fChaseRange, fAttackRange : 각각 이동속도, 추적 시작거리, 공격 가능 거리변수
     */
    private BT_Node root = null;              // BT루트 노드 : 모든 Evaluate() 호출이 시작되는 진입점
    Animator animatorMonsterState = null;     // 애니메이터 상태 변수
    public Transform characterTarget = null;  // 추적 대상

    float fMonsterSpeed = 2.0f; // 몬스터가 NPC를 추적하는 속도값
    float fChaseRange = 5.0f;   // 추적할 수 있는 거리 변수
    float fAttackRange = 1.5f;  // 공격할 수 있는 거리 변수

    // 순찰 관련 변수
    public Transform[] WayPoints;             // 순찰 지점들
    private int nWayPointIndex = 0;           // 현재 순찰 인덱스

    /* 루트 = Selector
     * 자식 1 : 공격 시퀀스 = [공격 범위?] -> [공격]
     * 자식 1 : 추적 시퀀스 = [추적 범위?] -> [추적]
     * 자식 1 : 대기 시퀀스 = [기본 상태] -> Idle(리프)
     * 우선 순위는 리스트 순서로 구현(앞에 있을수록 먼저 평가)
     */
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animatorMonsterState = GetComponent<Animator>(); // 애니메이터 컴포넌트 가져오기

        // Root : Selector
        root = new BT_Selector(new List<BT_Node>
        {
                new BT_Sequence(new List<BT_Node>          // 공격 시퀀스 : [공격 범위?] -> [공격]
                {
                    new BT_Leaf(CheckPlayerAttackRange),   // 공격 조건 leaf
                    new BT_Leaf(AttackPlayer)              // 행동 Leaf
                }),
                new BT_Sequence(new List<BT_Node>          // 추적 시퀀스 : [추적 범위?] -> [추적]
                {
                    new BT_Leaf(CheckPlayerChaseRange),    // 추적 조건 Leaf
                    new BT_Leaf(ChasePlayer)               // 행동 Leaf
                }),
                new BT_Sequence(new List<BT_Node>          // 순찰 시퀀스
                {
                new BT_Leaf(CheckPatrol),                  // 순찰 조건 Leaf  
                new BT_Leaf(Patrol)                        // 순찰 Leaf
                }),
                new BT_Leaf(IdlePlayer)                    // 아무 조건도 충족하지 못하면 Idle

        });
    }

    // 입력받은 range 값과 플레이어와의 실제 거리를 비교하여, 플레이어가 공격 범위안에 있으면 Success, 밖에 있으면 Failure를 반환하는 메소드
    BT_NodeStatus CheckPlayerAttackRange()
    {
        float fMonsterCharacterDist = Vector3.Distance(transform.position, characterTarget.position);

        return (fMonsterCharacterDist < fAttackRange)? BT_NodeStatus.Success : BT_NodeStatus.Failure;
    }
    // 입력 받은 range 값과 플레이어와의 실제거리를 비교하여, 플레이어가 추적 범위 안에 있으면 Success, 밖에 있으면 Failure를 반환하는 메소드
    BT_NodeStatus CheckPlayerChaseRange()
    {
        float fMonsterCharacterDist = Vector3.Distance(transform.position, characterTarget.position);

        return (fMonsterCharacterDist < fChaseRange) ? BT_NodeStatus.Success: BT_NodeStatus.Failure;
    }

    // 순찰 가능 여부를 판단하는 메서드
    BT_NodeStatus CheckPatrol()
    {
        // WayPoints 배열이 존재하고, 하나 이상의 순찰 포인트가 설정되어 있으면
        // Success 반환
        // 그렇지 않으면 Failure 반환
        return (WayPoints != null && WayPoints.Length > 0)
            ? BT_NodeStatus.Success : BT_NodeStatus.Failure; 
    }

    BT_NodeStatus IdlePlayer() // 대기 상태
    {
        f_Rotate(); // 몬스터가 NPC를 볼 수 있도록 Rotation변경
        f_MonsterAnimatorStateChange("IDLE"); // 애니메이션 대기 상태로 변환
        return BT_NodeStatus.Success;
    }
    BT_NodeStatus AttackPlayer() // 공격 상태
    {
        f_Rotate(); // 몬스터가 NPC를 볼 수 있도록 Rotation변경
        f_MonsterAnimatorStateChange("ATTACK"); // 애니메이션 공격 상태로 변환
        return BT_NodeStatus.Success;
    }
    BT_NodeStatus ChasePlayer() // 추적 상태
    {
        // Time.delteTime에 speed를 곱해준 값으로 몬스터 이동
        transform.position = Vector3.MoveTowards(transform.position, characterTarget.position, Time.deltaTime * fMonsterSpeed);
        f_Rotate();
        f_MonsterAnimatorStateChange("CHASE");
        return BT_NodeStatus.Running; 
    }

    // 현재 WayPoint로 이동하며, 도착 시 다음 WayPoint로 전환
    //  WayPoint가 없으면 실패 반환
    BT_NodeStatus Patrol()   // 순찰 상태
    {
        f_MonsterAnimatorStateChange("PATROL");   // 순찰 애니메이션 상태로 전환

        // 현재 순찰 지점을 목표로 설정
        Transform patrolTarget = WayPoints[nWayPointIndex];

        // 목표 방향으로 회전 (현재 WayPoint 바라보도록)
        f_Rotate(patrolTarget);

        transform.position = Vector3.MoveTowards(transform.position,patrolTarget.position,fMonsterSpeed * Time.deltaTime );

        // 목표 지점에 거의 도착하면 다음 지점으로
        if (Vector3.Distance(transform.position, patrolTarget.position) < 0.2f)
        {
            nWayPointIndex = (nWayPointIndex + 1) % WayPoints.Length;
        }

        return BT_NodeStatus.Running;
    }


    //유니티 애니메이터는 캐릭터 및 게임 오브젝트의 움직음을 제어하는 Mecanim 시스템의 핵심 컴포넌트 
    // 애니메이터는 애니메이션 클립을 상태로 정의 하고 
    private void f_MonsterAnimatorStateChange(string strState)
    {
        animatorMonsterState.SetBool("IDLE", false);   // 모든 상태를 false로 초기화
        animatorMonsterState.SetBool("CHASE", false);
        animatorMonsterState.SetBool("ATTACK", false);
        animatorMonsterState.SetBool("PATROL", false);

        //매개변수로 전달 받은 애니메이터 상태만  true
        animatorMonsterState.SetBool(strState, true); // 전달받은 상태만 true로 설정
    }

    // 몬스터가 NPC를 볼 수 있도록 하는 메소드 /  순찰 포인트를 바라볼 수 있도록
    private void f_Rotate(Transform target = null)
    {
        // target이 없으면 characterTarget을 기본으로 사용
        Vector3 vectortargetPos;

        if (target != null)
        {
            vectortargetPos = target.position;    // 특정 타겟(예: WayPoint)을 바라보는 경우
        }
        else
        {
            vectortargetPos = characterTarget.position;  // 플레이어를 바라보는 경우 
        }
        Vector3 vector3Direction = (vectortargetPos - transform.position).normalized;
        vector3Direction.y = 0.0f; // 수직 방향 회전 제거
        transform.forward = vector3Direction;
        
    }

    // root.Evaluate()를 매 프레임 호출해 트리 갱신
    // Start() 에서 설계한 행동 트리 전체가 최상위 노드부터 시작하여 매 프레임마다 자신의 상태를 평가하고 적절한 행동을 수행
    // Update is called once per frame
    void Update()
    {
        // 루트의 Evaluate() 메소드를 호출하여 하위 로직을 일괄 수행
        // Sequence, Selector 노드가 AND/OR, Leaf 노드가 실제 동작을 수행하도록 Update
        root.Evaluate();
    }
}
