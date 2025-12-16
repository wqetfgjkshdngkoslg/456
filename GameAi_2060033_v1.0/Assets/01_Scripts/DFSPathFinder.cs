/*
    DFS(Depth-First Search, 깊이 우선 탐색)를 사용해 시작 지점(Start)에서 도착 지점(End)까지의 경로를 찾는 스크립트
    DFS는 한 방향(분기)을 가능한 깊게 탐색한 뒤 막히면(더 이상 진행 불가) 직전 분기로 되돌아와 다른 분기를 탐색(백트래킹)
    이 스크립트는 GridManager에서 만든 타일을 바탕으로 DFS 알고리즘을 실행하여 시작 → 도착 경로를 찾는 기능을 담당한다.
*/

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class DFSPathFinder : MonoBehaviour
{
    // GridManager 를 통해 그리드 경계 / 통로 여부/타일 정보를 조회하기 위함
    GridManager gridManager = null;

    // DFS 탐색을 위한 시작 / 도착 지점 설정
    //  - Inspector 창에서  직접 수정 가능 
    // - (0,0) 좌표가 기본 시작, (4,4) 좌표가 기본 도착점
    [SerializeField]
    private Vector2Int vStartLocation = new Vector2Int(0, 0);   //시작 지점
    [SerializeField]
    private Vector2Int vEndLocation = new Vector2Int(14, 14);     // 도착 지점

    // DFS 탐색 중 실제로 꺼낸 노드 수 
    private int nDFSSearchCount = 0;

    // 탐색한 노드 수를 화면에 표시할 TextMEshPro 텍스트
    [SerializeField]
    private TMP_Text DFS_SearchCount_Text = null;

    void Awake()
    {
        gridManager = GetComponent<GridManager>();  // GridManager 컴포넌트를 가져옴
    }

    // 시작/도착 좌표를 코드로 변경하기
    //  - 필요에 따라 다른 스크립트에서 시작/도착 지점을 동적으로 변경 가능하다.
    //  - start : 시작지점을 설정
    //  - end : 도착지점을 설정
    public void f_SetStartLocation(Vector2Int start)  //시작지점을 설정하는 메소드
    {
        vStartLocation = start;
    }

    public void f_SetEndLocation(Vector2Int end)  //도착지점을 설정하는 메소드
    {
        vEndLocation = end;
    }

    // DFS 탐색 실행 메소드
    //  - HashSet을 사용하여 방문한 좌표를 기록 → 중복 방문 방지 + 빠른 탐색
    //  - 최종적으로 경로 리스트(List<Vector2Int>)를 반환
    public List<Vector2Int> f_GetDFSPath()
    {
        nDFSSearchCount = 0;
        /*
            [HashSet] : 중복된 값을 허용하지 않는 집합
            특징
                유일성 : 동일한 값을 여러 번 추가하려고 하면 무시
                빠른 검색 : 내부적으로 해시 해시 테이블(자료구조)을 사용하여 빠른 검색과 삽입
                무작위 순서 : 데이터를 추가한 순서를 유지하지 않음
            HashSet vs List
                List는 순서가 존재하는 데이터 목록이며 중복된 값을 허용한다
                따라서 List를 사용한다면 방문한 위치를 확인할 때마다 전체 리스트를 순회해야 하므로 비효율적
                HashSet은 순서가 없고 중복을 허용하지 않는 데이터 집합이므로 방문한 위치를 빠르게 확인할 수 있다
        */
        HashSet<Vector2Int> vVisitedLocation = new HashSet<Vector2Int>();

        

        return f_DFS(vStartLocation, vEndLocation, vVisitedLocation);


    }


    /* DFS 재귀 탐색 로직 만들기    
    [탐색 순서]
        현재 좌표가 유효한지 검사 (f_IsValid)
        이미 방문한 좌표라면 중단
        현재 좌표가 도착점이면 리스트에 담아 반환
        상·하·좌·우 순서로 재귀 탐색 진행
        경로 발견 시 현재 좌표를 맨 앞에 추가
    */  
    // DFS 알고리즘을 사용해 current 에서 end 까지의 경로를 재귀적으로 탐색하는 메소드
    private List<Vector2Int> f_DFS(Vector2Int current, Vector2Int end, HashSet<Vector2Int> visited)
    {
        if(!f_IsValid(current) || visited.Contains(current))
        {
            return null;
        }

        visited.Add(current);   // 현재 좌표를 방문 목록에 등록해 중복 방문을 방지 

        nDFSSearchCount++;

        if (current == end)  // 도착 지점에 도달했다면
        {
            if (DFS_SearchCount_Text != null)
            {
                DFS_SearchCount_Text.text = "DFS : " + nDFSSearchCount.ToString();
            }

            return new List<Vector2Int> { current };
        }

        /*
         상하좌우 네 방향을 순서대로 탐색
         이 순서가 바뀌면 DFS 특성상 처음 발견되는 경로가 달라질수 있다
         */
        Vector2Int[] vDirections = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        

        /*
         
        각 방향으로 한 칸 이동해 다음 좌표(vNeighbor)를 계산
        그 좌표를 시작점으로 재귀적으로 f_DFS 호출*/
        foreach (Vector2Int dir in vDirections)
        {
            //현재 위치에서 dir 방향으로 한 칸 이동한 이웃 좌표
            Vector2Int vNeighbor = current + dir;

            /*
             이웃 좌표에서 도착지점까지의 경로를 탐색
             - 경로가 발견되면 현재 좌표를 경로의 맨 앞에 추가하고 반환
             - path에는 vNeighbor → ... → end 순서로 좌표가 들어있다
             경로가 없으면 null이 반환되고, 다음 방향을 탐색한다
            */
            List<Vector2Int> path = f_DFS(vNeighbor, end, visited);

            /*
             경로가 발견되었다면, 현재 좌표를 경로의 맨 앞에 추가하고 반환
             current → vNeighbor → ... → end 순서로 경로가 구성된다
             경로가 하나라도 발견되면 즉시 반환하므로, 가장 먼저 발견된 경로가 선택된다(DFS 특성)
            */
            if (path != null)
            {
                path.Insert(0, current);

               
                return path;
            }
        }

        

        return null;

    }

    // 유효 좌표 검사 메소드
    //좌표가 그리드 내부이며, 이동 가능한 셀인지 검사하는 메소드
    bool f_IsValid(Vector2Int pos)
    {
        if (!gridManager.f_IsInside(pos)) //그리드 내부인지 확인
        {
            return false;
        }

        return gridManager.f_IsWalkable(pos); //이동 가능한 셀인지 확인(벽이 아니여야 함)
    }


}
