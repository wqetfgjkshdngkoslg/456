/* 
 * A* 알고리즘이란
 *  A* 는 f(n) = g(n) + h(n) 공식을 기반으로 최적 경로를 찾는 탐색 알고리즘이다 
 *   g(n) 시작점에서 현재 노드까지의 실제 이동 비용
 *   h(n) 현재 노드에서 목표까지의 추정 거리 
 *   항상 f(n) 값이 가장 작은 노드를 먼저 탐색하여 불 필요한 확장을 줄이면서 목표 방향으로 빠르게 탐색한다 
 *   휴리스틱은 상 하 좌 우  격자에서 맨해튼 거리 를 h(n) 으로 사용
 
 */

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AStarPathFinder : MonoBehaviour
{
    // 멤버 변수 정의 
    // 그리드 정보 를 제공하는 매니저 

    GridManager gridManager = null;

    // 시작위치와 도착위치
    [SerializeField]
    private Vector2Int vStartLocation = new Vector2Int(0, 0);
    [SerializeField]
    private Vector2Int vEndLocation = new Vector2Int(14, 14);

    // A* 탐색 중 실제로 꺼낸 노드 수 
    private int nAStarSearchCount = 0;

    // 탐색한 노드 수를 화면에 표시할 TextMEshPro 텍스트
    [SerializeField]
    private TMP_Text AStar_SearchCount_Text = null;

    // 탐색 방향 상 하 좌 우 네 방향으로 이동
    // 현재 타일에서 4방향을 한번에 처리하기 위한 배열
    // 이배열 덕분에 foreach 를 사용해서 이웃 칸을 간단하게 탐색할 수 있다 
    Vector2Int[] vDirections = new Vector2Int[]
    {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
    };

    // 초기화
    private void Awake()
    {
        gridManager = GetComponent<GridManager>();  // GridManager 컴포넌트를 가져옴
    }

    // A* 메인 로직 경로 계산
    // A* 알고리즘을 사용하여 시작 -> 종료 까지의 경로 계산
    // 경로가 존재하면 타일 좌표 리스트를 반환 없으면 null 반환
    public List<Vector2Int> f_GetAStarPath()
    {
         // A* 오픈 리스트 아직 확정되지 않은 후보 노드들
         PriorityQueue<Vector2Int> openSet = new PriorityQueue<Vector2Int>();

        //각  노드의 부모 노드를 기록  ( 경로 복원용)
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(); 

        // gScore[n] 시작점에서 n까지의 실제 이동 비용
        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>(); 

        // fScore[n] : f(n) = g(n) + h(n)  총 예상 비용
        Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>(); 

        // 이미 최적 경로로 확정된 노드 집합
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        // 시작 노드 초기값 설정
        gScore[vStartLocation] = 0;
        fScore[vStartLocation] = f_Heuristic(vStartLocation, vEndLocation);

        // 시작 노드를 오픈 리스트에 등록 (우선순위 = fScore)
        openSet.Enqueue(vStartLocation, fScore[vStartLocation]);

        // 탐색 카운트 리셋 
        nAStarSearchCount = 0;

        // 오픈 리스트가 빌 때까지 반복
        while (openSet.Count > 0)
        {
            // fSocre 가 가장 작은 노드를 꺼낸다 
            Vector2Int current = openSet.Dequeue();

            // 탐색 카운트 누적 
            nAStarSearchCount++;

            // 도착 지점에 도달한 경우 경로 복원후 반환
            if(current == vEndLocation)
            {
                // 탐색한 노드 수를 UI에 표시 
                if(AStar_SearchCount_Text != null)
                {
                    AStar_SearchCount_Text.text = "AStar : " + nAStarSearchCount.ToString();
                }

                return f_ReconstructPath(cameFrom, current);
            }

            // 현재 노드는 확정 집합에 추가 
            closedSet.Add(current);

            // 상/하/좌/우 이웃 노드들 검사
            foreach (Vector2Int direction in vDirections)
            {
                Vector2Int neighbor = current + direction;

                // 그리드 밖이거나, 이미 확정된 노드면 건너뛴다.
                if (!f_IsValid(neighbor) || closedSet.Contains(neighbor))
                {
                    continue;
                }

                // current를 거쳐서 neighbor로 이동했을 때의 gScore 후보 값
                int tentative_gScore = gScore[current] + 1;  // 인접 타일까지 비용 1

                // 더 나은 경로인지 검사
                //   - 아직 gScore 정보가 없거나
                //   - 기존 gScore보다 더 작은 값인 경우에만 갱신
                bool isBetterPath =
                    !gScore.ContainsKey(neighbor) ||
                    tentative_gScore < gScore[neighbor];

                if (isBetterPath)
                {
                    // neighbor로 오는 최적의 경로는 current를 통해 왔다고 기록
                    cameFrom[neighbor] = current;

                    // gScore, fScore 갱신
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = tentative_gScore + f_Heuristic(neighbor, vEndLocation);

                    // 오픈 리스트에 후보로 추가 (우선순위 = fScore)
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }

            }

        }

        // 오픈 리스트가 모두 빌 때까지 도착 지점에 도달하지 못했다면 -> 경로 없음
        return null;

    }

    // 휴리스틱 계산 맨해튼 거리 
    // 두좌표 a b 사이의 맨해튼 거리 를 계산
    // 상화좌우 로만 이동할 수 있는 격자에서 최소 이동 칸 수를 의미 
    private int f_Heuristic(Vector2Int a, Vector2Int b)
    {
        int nDeltaX = Mathf.Abs(a.x - b.x);
        int nDeltaY = Mathf.Abs(a.y - b.y);

        return nDeltaX + nDeltaY;
    }

    // ⑤ 경로 복원 : cameFrom을 이용해 시작→도착 경로 만들기 
    //     - cameFrom 정보를 이용하여 end 지점에서 시작 지점까지 역추적한 뒤,
    //       "시작 → 도착" 순서의 경로 리스트를 만들어 반환한다.
    private List<Vector2Int> f_ReconstructPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int end)
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

    // ⑥ 좌표 유효성 검사 : 맵 범위 + 이동 가능 여부 
    //     - 주어진 좌표가 그리드 내부이며, 이동 가능한 타일인지 검사한다.
    private bool f_IsValid(Vector2Int pos)
    {
        if (!gridManager.f_IsInside(pos)) //그리드 내부인지 확인
        {
            return false;
        }

        return gridManager.f_IsWalkable(pos); //이동 가능한 셀인지 확인(벽이 아니여야 함)

    }

}
