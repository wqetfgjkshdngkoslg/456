
/*
 * FSM이란, 
 * 한번에 하나의 상태만 가지며 특정 조건에 따라 다른 상태로 전환되는 구조
 * state : 현재 상태(Idle, Attack, Patrol, Chase, Die)
 *  public Enum State
 *  {
 *      Idle,
 *      Patrol,
 *      Chase,
 *      Attack,
 *      Die
 *  };
 *  
 *  Transition : 상태 변경 조건(거리. 시간, 이벤트 등)
 *  Entry/Exit : 상태 진입 시 동작 / 상태 탈출 시 동작
 *  Update : 상태 지속 중 반복 처리 ex : 추격중 이동
 * 
 *  이걸 왜 쓰는가?
 *  상태가 수정이 용이
 *  - 디자이너 혹은 기획자 분들이 이 상태를 수정을 하고 싶을 때 좀더 용이 하게 수정을 할 수 있게끔 처리해 줄 수 있는 방법
 *  간단하면서도 명확한 딕셔너리 구조
 *  - 업데이트 라이프 사이클 루프에서 현재 상태에 맞는 행동을 실행해주면서 조건에 따라서 각각의 상태를 전환할 수 있도록 처리함
 *  테스트 디버깅 용이
 *  상대별 책임 분리가 쉬움
 *  모든 ai 로직의 기반
 *  - fms ai 캐릭터가 어떤 상태에 있는지 그리고 언제 어떻게 상태를 바꾸는지를 설계하는 틀
 *  - fms는 계산 모델이자 제어 흐름을 정의하는 알고리즘 구조로 간주될 수 있음
 *  게임 개발 관점 : fsm 은 설계 구조. 디자인 구조, 상태처리 프레임 워크
 *  
 *  
 *  단점
 *  상태수가 많아질수록 복잡도가 급격히 증가
 *  - 예를 들어, 상태가 3~4개일때 문제가 없음. 상태가 10개 이상이라면 상태 전이 조건을 일일이 관리하기 어렵다
 *  - n개 상태가 있으면 전이조건은 최대 n^z개가 될 수 있음
 *  - 그러므로 상태 폭발 문제가있음
 *  행동이 고정적이라 유연성 부족
 *  - 상태마다 동작이 고정되어 있으므로, 상황에 따라 ai가 전략적으로 판단하는 행동을 만들기가 어렵다
 *  - 예 : 체력이 50퍼이하면 도망, 이것을 여러사애에서 구현하면 중복 코드,설계가 어렵다
 *  조건 분기가 많아질수록 관리가 힘듦
 *  - 조건문이 뒤엉키기 시작하면서 디버깅이 매우 어려워지고 상태간의 연결관계 시각화도 상당히 어려워짐
 *  재사용성과 확장성이 낮음
 *  - 상태가 늘어날 수록 기존 코드와 강하게 결합되어있어서 새로운 상태를 넣거나 뺄 떄 전체구조를 뜯어 고쳐야 할 수도 있음
 *  fsm은 간단하고 직관적인 구조이지만, 상태가 많아질수록 전위 조건과 분기처리가 굉장히 복잡해지고
 *  ai가 복잡핝 전략을 판단하는데 한계가 있기 때문에 이후에 사용하는 bt, goap같은 새로운 ai알고리즘이 등장
 *  
 */

using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public enum State
    {
        Idle,    // 대기 상태
        Chase,   // 추적 상태
        Attack,  // 공격 상태
        Patrol   // 순찰 상태
    }

    public State MonsterCurrentState = State.Idle; // 현재 적의 상태를 Idle로 초기화
    private Animator animatorMonsterState; // 애니메이터 컴포넌트를 참조할 변수

    public float fMonsterSpeed = 2.0f; // 몬스터 이동 속도

    public Transform characterrTarget = null; // 추적할 대상(플레이어)의 Transform

    public float fChaseRange = 5.0f; // 추적 시작 거리
    public float fAttackRange = 1.5f; // 공격 시작 거리

    public Transform[] WayPoints;  // 순찰 지점들
    private int nWayPointIndex = 0; // 현재 순찰 중인 목표 지점의 인덱스

    void Start()
    {
        animatorMonsterState = GetComponent<Animator>(); // 시작 시 Animator 컴포넌트 가져오기
    }

    void Update()
    {
        switch (MonsterCurrentState)
        {
            case State.Idle:
                f_MonsterIdleState(); // 대기 상태 처리
                break;
            case State.Chase:
                f_MonsterChaseState(); // 추적 상태 처리
                break;
            case State.Attack:
                f_MonsterAttackState(); // 공격 상태 처리
                break;
            case State.Patrol:
                f_MonsterPatrolState(); // 순찰 상태 처리
                break;
        }

        f_MonsterTransitionCheck(); // 상태 전환 조건 확인
    }

    private void f_MonsterIdleState()
    {
        f_MonsterAnimatorStateChange("IDLE"); // 애니메이터에 IDLE 상태 적용
    }

    // 추적 상태 로직
    private void f_MonsterChaseState()
    {
        f_MonsterAnimatorStateChange("CHASE"); // 애니메이터에 CHASE 상태 적용

        f_MonsterRotate(characterrTarget.position); // 타겟을 향해 몬스터 회전

        transform.position = Vector3.MoveTowards(
            transform.position, // 현재 위치에서
            characterrTarget.position, // 타겟 위치로
            Time.deltaTime * fMonsterSpeed // 일정 속도로 이동
        );
    }

    // 공격 상태 로직
    private void f_MonsterAttackState()
    {
        f_MonsterAnimatorStateChange("ATTACK"); // 애니메이터에 ATTACK 상태 적용

        f_MonsterRotate(characterrTarget.position);  // 공격 시 타겟 방향으로 회전
    }

    // 순찰 상태 로직
    private void f_MonsterPatrolState()
    {
        f_MonsterAnimatorStateChange("PATROL"); // 애니메이터에 PATROL 상태 적용

        if (WayPoints.Length == 0) return; // 순찰 지점이 없다면 종료

        Transform patrolTarget = WayPoints[nWayPointIndex]; // 현재 목표 순찰 지점 가져오기

        f_MonsterRotate(patrolTarget.position); // 순찰 지점을 향해 회전

        transform.position = Vector3.MoveTowards
        (
            transform.position, // 현재 위치에서
            patrolTarget.position, // 순찰 목표 지점으로
            fMonsterSpeed * Time.deltaTime // 일정 속도로 이동
        );

        // 목표 지점에 거의 도달했다면 다음 순찰 지점으로 이동
        if (Vector3.Distance(transform.position, patrolTarget.position) < 0.2f)
        {
            nWayPointIndex = (nWayPointIndex + 1) % WayPoints.Length; // 다음 지점, 순환 구조
        }
    }

    // 애니메이터 상태 전환 처리
    private void f_MonsterAnimatorStateChange(string strState)
    {
        animatorMonsterState.SetBool("IDLE", false);   // 모든 상태를 false로 초기화
        animatorMonsterState.SetBool("CHASE", false);
        animatorMonsterState.SetBool("ATTACK", false);
        animatorMonsterState.SetBool("PATROL", false);

        animatorMonsterState.SetBool(strState, true); // 전달받은 상태만 true로 설정
    }

    // 거리 기반 상태 전환 검사
    private void f_MonsterTransitionCheck()
    {
        

        float fDistance = Vector3.Distance(transform.position, characterrTarget.position); // 타겟과의 거리 측정

        if (fDistance < fAttackRange) // 공격 범위 내라면
        {
            MonsterCurrentState = State.Attack; // 공격 상태로 전환
        }
        else if (fDistance < fChaseRange) // 추적 범위 내라면
        {
            MonsterCurrentState = State.Chase; // 추적 상태로 전환
        }
        else // 범위를 벗어나면
        {
            MonsterCurrentState = State.Patrol; // 순찰 상태로 전환
        }
    }

    // 적을 특정 위치로 회전시키는 함수
    private void f_MonsterRotate(Vector3 targetPosition)
    {
        Vector3 vectorDirection = (targetPosition - transform.position).normalized; // 타겟 방향 벡터 계산 및 정규화
        /*
         * normalized는 벡터의 방향만 유지하고 크기를 1로 만들어 정확한 방향 회전을 위해 사용됩니다.
           회전에서는 방향만 필요하고 거리(크기)는 불필요하므로, 정규화로 불필요한 영향 제거가 가능합니다.
           사용하지 않으면 회전 오류나 예기치 않은 동작이 발생할 수 있어 필수입니다.
         */
        vectorDirection.y = 0.0f; // y축은 회전하지 않도록 고정


        transform.forward = vectorDirection; // 해당 방향으로 회전

    }
}
