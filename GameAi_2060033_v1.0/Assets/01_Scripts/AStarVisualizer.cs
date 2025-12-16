using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class AStarVisualizer : MonoBehaviour
{
    // 멤버 변수 정의
    // A* 경로를 계산하는 스크립트 
    private AStarPathFinder aStarPathFinder = null;

    // 그리드 경계/통로 여부/타일 정보 조회
    //  - 같은 오브젝트에 붙어 있는 GridManager 컴포넌트를 참조하는 변수입니다.
    //  - 현재 씬에 생성된 타일 정보, 그리드 범위, 타일 가져오기 기능(f_GetTileInBounds) 등을 제공해 줍니다.
    private GridManager gridManager = null;

    // 경로 표시 간격(초)
    //   - 각 타일을 칠할 때, 다음 타일로 넘어가기 전에 얼마나 기다릴지(초 단위) 를 지정하는 값입니다.
    //   - [SerializeField] 덕분에 Inspector에서 이 값을 직접 조정할 수 있습니다.
    //   - 예: 0.1로 줄이면, 경로가 매우 빠르게 칠해지고
    //   - 1.0으로 늘리면, 한 칸씩 “천천히” 진행되는 연출을 볼 수 있습니다.
    [SerializeField] private float fStepDelaySeconds = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 같은 GameObject 에 붙여있는 AStarPathFinder GridManager 를 가져온다 
        aStarPathFinder = GetComponent<AStarPathFinder>();  

        gridManager = GetComponent<GridManager>();  
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
        {
            // 조건을 만족하면 StartCoroutine(ShowBFSPath()) 를 호출합니다.
            // 이는 코루틴을 실행해서, 한 프레임에 모든 색을 바꾸는 대신, 여러 프레임에 걸쳐 순차적으로 타일 색을 변경하게 하는 역할을 합니다.
            // 결과적으로, 경로가 “한 칸씩 칠해지는 애니메이션”처럼 보이게 되어 시각적으로 BFS 알고리즘을 쉽게 확인할 수 있도록 합니다.
            StartCoroutine(ShowAStarPath());
        }
    }

    private IEnumerator ShowAStarPath()
    {
        // A* 알고리즘을 사용해 계산된 최종 경로를 가져온다.
        List<Vector2Int> path = aStarPathFinder.f_GetAStarPath();

        // 경로가 없으면 아무것도 하지 않고 종료
        if (path == null)
        {
            yield break;
        }

        // 경로에 포함된 각 좌표를 순서대로 처리
        foreach (Vector2Int pos in path)
        {
            // 해당 좌표에 있는 Tile 객체를 GridManager에게서 가져온다.
            Tile tile = gridManager.f_GetTileInBounds(pos);

            // 타일이 존재하면 색을 변경한다. (예: 노란색)
            if (tile != null)
            {
                tile.f_SetColor(Color.yellow);
            }

            // fStepDelaySeconds 동안 대기한 뒤, 다음 타일로 넘어간다.
            yield return new WaitForSeconds(fStepDelaySeconds);
        }

    }

}
