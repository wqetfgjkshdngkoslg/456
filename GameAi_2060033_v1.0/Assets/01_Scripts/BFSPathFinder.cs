/* BFS(너비 우선 탐색)란 ?
 *  - BFS는 그래프나 격자에서 시작점부터 가까운 노드부터 차례대로 탐색하는 방식이다.
 *  - 큐 자료구조를 사용하여, 현재 레벨의 모든 놓드를 확인한뒤  그 다음 레벨로 넘어간다 .
 *  -  DFS가 깊게 파고드는 방식이라면 BFS는 넓게 퍼져나가는 탐색을 한다 
 * BFS를 사용하는 이유
 *  - 최단 경로 보장
 *  - BFS는 시작점에서 목표 지점까지 가는 가장 짧은 경로를 보장한다 .
 *  - 시작점에서 한칸 두칸 세칸씩 차례대로 확장하므로 탐색의 범위가 원형으로 퍼져나가는 구조를 직관적으로 볼수 있다.
 *  - 미로에서 최단 탈출 경로 찾기 
 *  - NPC 가 가장 빠르게 플레이어에게 도달하는 AI구현
 */

//List Queue Dictionary HashSet 같은 자료구조 타입을 사용하기 위해 필요한 네임스페이스 
// BFS 에서는 큐와 방문집합 부모추적 DIctionary 를 사용할 예정
using UnityEngine;
using System.Collections.Generic;
using TMPro;

// BFS 알고리즘으로 시작 지점에서 도착지점 까지의 경로를 찾는 클래스 
public class BFSPathFinder : MonoBehaviour
{
    // 필드 & Awake : 환경 준비 
    // 이 필드는 현재 씬에 있는 GridManager 컴포넌트를 참조하기 위한 변수 
    // GridManager는 이좌표가 미로 안인지 여기가 길인지 벽인지 같은 정보를 제공
    GridManager gridManager = null;

    //Vector2Int 는 (x, y) 좌표를 정수 단위로 표현하는 타입
    // 미로는 타일 단위로  움직이기 때문에 소수점 이하가 필요없다 -> 그래서 Vector2 대신 Vector2Int 자료형을 사용한다 
    // SerializeField 덕분에 Inspector 에서 직접 시작/끝 좌표를 수정할 수 있어서 알고리즘을 돌려보면서 여러 위치를 시험해 보기 적합하다 .
    [SerializeField]
    private Vector2Int vStartLocation = new Vector2Int(0, 0);
    [SerializeField]
    private Vector2Int vEndLocation = new Vector2Int(14, 14);

    // BFS 탐색 중 실제로 꺼낸 노드 수 
    private int nBFSSearchCount = 0;

    // 탐색한 노드 수를 화면에 표시할 TextMEshPro 텍스트
    [SerializeField]
    private TMP_Text BFS_SearchCount_Text = null;

    //Awake 는 유니티 생명주기 함수 중 하나로 게임이 시작할 때 이 컴포넌트가 준비되는 순간 한번만 호출되는 메소드 
    // GetComponent<GridManager>() 는 같은 GameObject에 붙어 있는 GridManager 컴포넌트를 찾아오는 메소드 
    // 빈 오브젝트인 GridRoot 에 GridMAnager와 BFSPathFinder 를 같이 붙였기 때문에  이 한 줄의 코드로 오브젝트에 해당 기능 추가
    void Awake()
    {
        gridManager = GetComponent<GridManager>();  // GridManager 컴포넌트를 가져옴
    }

    // f_GetBFSPath : BFS로 실제로 경로를 찾는 메소드 
    // BFS 알고리즘을 사용해 시작 지점에서 도착 지점까지의 경로를 찾는 메소드 
    // 시작 지점에서 도착 지점까지의 최단 경로를 List<Vector2Int> 형태로 반환 없으면 null반환
    public List<Vector2Int> f_GetBFSPath()
    {
        // BFS에 필요한 자료구조 만들기 
        // 방문집합 : 이미 방문한 노드 기록 
        // visited
        // - 이미 체크해본 타일 목록
        // - 같은 곳을 여러번 보지 않게 해서 무한 루프를 방지하고 성능도 확보한다 
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        //부모 추적 특정 노드의 직전 위치를 기록 -> 경로 복원에 사용
        // cameFrom
        //- 어떤 타일이 어디 타일에서 왔는지 체인정보저장
        // - 예 : (2 ,1 ) 은 (1,1 )에서 왔다 
        // 나중에 도착 지점에서 거꾸로 따라가며 경로를 복원할 때 사용
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        // 탐색 큐 : BFS 핵심 자료구조 FIFO 원칙으로 노드 탐색
        // Queue 
        // - BFS의 핵심인 큐 
        // - 먼저 들어온 좌표부터 꺼내서 처리하는 줄 서기 자료구조 
        // - 초기 설정 queue.Enqueue(vStartLocation);
        // -> 시작 좌표를 제일 처음 탐색 대상으로 줄에 세운다.
        // - visited,Add(vStartLocation); 
        // -> 시작 좌표는 이미 방문했다고 표시 
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        //초기 설정 시작 지점을 큐에 넣고 방문 처리 
        queue.Enqueue(vStartLocation);
        visited.Add(vStartLocation);

        // 탐색 방향 상 하 좌 우 네 방향으로 이동
        // 현재 타일에서 4방향을 한번에 처리하기 위한 배열
        // 이배열 덕분에 foreach 를 사용해서 이웃 칸을 간단하게 탐색할 수 있다 
        Vector2Int[] vDirections = new Vector2Int[]
        {
            Vector2Int.down,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right
        };

        nBFSSearchCount = 0;

        // BFS 메인 루프 빈 큐가 나올때까지 반복
        while (queue.Count > 0)
        {
            // current 꺼내기 현재 노드 큐에서 꺼낸 가장 오래된 노드 
            // Vector2Int current = queue.Dequeue();
            // 큐에서 가장 먼저 들어왔던 좌표를 꺼낸다 
            // BFS는 가까운 거리 순서대로 큐에 쌓이기 때문에 항상거리 1->2-> 3 순서로 탐색해 나간다
            Vector2Int current = queue.Dequeue();
            

            // 탐색 카운트 누적 
            nBFSSearchCount++;

            // 도착 지점에 도달한 경우 경로 복원후 반환
            if (current == vEndLocation)
            {
                // 탐색한 노드 수를 UI에 표시 
                if (BFS_SearchCount_Text != null)
                {
                    BFS_SearchCount_Text.text = "BFS : " + nBFSSearchCount.ToString();
                }

                return f_ReconstructPath(cameFrom, current);
            }

            // 도착 지점에 도달했는지 확인
            // 종료 조건 도착 지점에 도달하면 경로 복원
            // 현재 칸이 우리가 찾고 있던 도착 좌표라면 더 이상 탐색할 필요 없이 바로 경로 복원 단계로 넘어간다 
            // f_ReconstructPath 는 출구에 도착했으니까 이제 지나온 길을 역으로 따라가서 경로 리스트를 만들어줘 라는 의미 
            if (current == vEndLocation)
            {
                return f_ReconstructPath(cameFrom, vEndLocation);
            }

            // 이웃 탐색 상 하 좌 우 네 방향
            // BFS 알고리즘의 탐색 실행
            foreach (Vector2Int dir in vDirections)
            {
                Vector2Int next = current + dir;

                // 유효한 위치이고 아직 방문하지 않았다면
                if (f_IsValid(next) && !visited.Contains(next))
                {
                    queue.Enqueue(next);    //큐추가 

                    visited.Add(next);  // 방문처리


                    cameFrom[next] = current; // next 부모는 current
                }
               
            }



        }
        // 큐가 공백일때 까지도착점을 못찾은경우 
        // while (queue.COunt > 0 ) 루프가 끝났다는 것은 더 이상 탐색할 좌표가 없는데도 도착 지점에 도달하지 못했다는 것을 의미
        // 이때는 return null; 로 갈 수 있는 길이 없다는 결과를 돌려주도록 함
        return null;

    }

    // f_ReconstructPath : BFS 탐색 결과를 이용해서 실제 경로 리스트를 만드는 메소드 
    // 부모 추적 정보를 사용해 시작 지점에서 도착 지점까지의 경로를 복원하는 메소드 
    // DIctionary 로 부모 정보를 저장해둔 이유는 
    // -cameFrom Dictionary 에는 Key 자식 노드 좌표 Value 부모 노드 좌표 형태로 
    //  어떤 칸이 어디서 왔는지 가 줄줄이 연결된 상태로 저장돼 있다 
    // 예를 들어 
    // (1,0) 은 0,0 에서 
    //  2, 0은 1, 0에서 
    //  2, 1은 2,0 에서 
    //  이런 식으로  체인처럼 이어져있다 
    List<Vector2Int> f_ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end)
    {
        
        List<Vector2Int> path = new List<Vector2Int>();

        // current 를 도착좌표로 설정
        Vector2Int current = end;

        // end 에서 시작 위치까지 부모를 따라 거슬러 올라감
        // 현재 칸이 cameFrom 에 Key 로 존재하는 동안반복
        // 즉 이 칸이 어디에서 왔는지 기록이 남아 있는 동안 반복
        // while 이 끝나면 더 이상 부모 정보가 없는 칸에 도착 -> 시작 칸 
        while (cameFrom.ContainsKey(current))
        {
            // 지금 칸을 경로에 추가 
            path.Add(current);       

            // 한 칸 이전 위치(부모)로 이동
            current = cameFrom[current];   
        }

        // 마지막으로 시작 위치 추가 
        // 이 시점에서 path는 end -> .. -> start 순서로 거꾸로 쌓여 있음
        path.Add(current);

        // 현재는 도착 -> 시작 순서 반대로 뒤집음
        // 리스트를 뒤집어서 start -> end 순서로 정렬
        path.Reverse();

        // 리스트를 반환하면 BFSVisualizer 가 이좌표들을 순서대로 읽으면서 타일 색을 바꿔 줄 수 있다.
        return path;
    }

    //f_IsValid : 특정 좌표가 그리드 안 + 이동 가능한 칸인지 검사하는 메소드 
    bool f_IsValid(Vector2Int pos)
    {
        if (!gridManager.f_IsInside(pos)) //그리드 내부인지 확인
        {
            return false;
        }

        return gridManager.f_IsWalkable(pos); //이동 가능한 셀인지 확인(벽이 아니여야 함)
    }

}
