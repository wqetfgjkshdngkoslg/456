/*
 * BFSPathFinder 에서구한 최단 경로를 한칸씩 파란색으로 칠하여 시각화하는 스크립트 
 * 키입력 B Key 을 감지해 코루틴을 실행
 */

// 코루틴에서 사용하는 IEnumerator 타입을 사용하기 위해 필요합니다.
// ShowBFSPath() 메소드는 IEnumerator를 반환하는 코루틴이므로, 이 using 구문이 필수입니다.
using System.Collections;
using UnityEngine;
// 새로운 Input System을 사용할 때 필요한 네임스페이스입니다.
// 이 스크립트에서는 Keyboard.current.bKey.wasPressedThisFrame 과 같이, 키보드 입력을 코드에서 직접 감지하는 기능을 사용합니다.
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class BFSVisualizer : MonoBehaviour
{
    // 필드(멤버 변수) : BFSPathFinder, GridManager, 지연 시간 값
    // 경로 탐색기능
    //   - 같은 오브젝트에 붙어 있는 BFSPathFinder 컴포넌트를 참조하는 변수입니다.
    //   - 최단 경로를 계산하기 위해 BFS 알고리즘을 실행하는 역할을 합니다.
    BFSPathFinder bfsPathFinder = null;

    // 그리드 경계/통로 여부/타일 정보 조회
    //  - 같은 오브젝트에 붙어 있는 GridManager 컴포넌트를 참조하는 변수입니다.
    //  - 현재 씬에 생성된 타일 정보, 그리드 범위, 타일 가져오기 기능(f_GetTileInBounds) 등을 제공해 줍니다.
    GridManager gridManager = null;

    // 경로 표시 간격(초)
    //   - 각 타일을 칠할 때, 다음 타일로 넘어가기 전에 얼마나 기다릴지(초 단위) 를 지정하는 값입니다.
    //   - [SerializeField] 덕분에 Inspector에서 이 값을 직접 조정할 수 있습니다.
    //   - 예: 0.1로 줄이면, 경로가 매우 빠르게 칠해지고
    //   - 1.0으로 늘리면, 한 칸씩 “천천히” 진행되는 연출을 볼 수 있습니다.
    [SerializeField] private float fStepDelaySeconds = 0.1f;






    // Start() 는 유니티 생명주기 함수로, 게임 오브젝트가 활성화된 직후 한 번 호출됩니다.
    //  - BFSVisualizer는 Start()에서 “길찾기 두뇌(BFSPathFinder)”와 “맵 관리자(GridManager)”를 준비해 놓고,
    //  - 나중에 키 입력이 오면 이 둘을 조합해 경로를 그려주는(시각화) 역할을 합니다.
    void Start()
    {
        // 현재 GameObject(GridRoot)에 붙어 있는 BFSPathFinder 컴포넌트를 찾아서 bfsPathFinder 변수에 저장합니다.
        bfsPathFinder = GetComponent<BFSPathFinder>();

        // 동일하게 GridManager 컴포넌트를 찾아 gridManager 변수에 저장합니다.
        gridManager = GetComponent<GridManager>();

    }


    // 새로운 Input System의 'B' key 입력 감지 방법(wasPressedThisFrame == GetKeyDown)
    // 키보드 입력이 null이 아니고, 'B' 키가 이번 프레임에 눌렸다면 코루틴 시작
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            // 조건을 만족하면 StartCoroutine(ShowBFSPath()) 를 호출합니다.
            // 이는 코루틴을 실행해서, 한 프레임에 모든 색을 바꾸는 대신, 여러 프레임에 걸쳐 순차적으로 타일 색을 변경하게 하는 역할을 합니다.
            // 결과적으로, 경로가 “한 칸씩 칠해지는 애니메이션”처럼 보이게 되어 시각적으로 BFS 알고리즘을 쉽게 확인할 수 있도록 합니다.
            StartCoroutine(ShowBFSPath());
        }

    }


    // ShowBFSPath 코루틴 : 경로를 한 칸씩 색칠하는 시각화 로직
    IEnumerator ShowBFSPath()
    {
        // BFSPathFinder 에서 경로 갖오기 
        // f_GetBFSPath 는 시작 지점에서 도착 지점까지의 최단 경로를 List <VectorInt> 로 반환
        // 이 리스트안에는 타일좌표들이 순서대로 들어있다 
        var path = bfsPathFinder.f_GetBFSPath();

        // foreach – 경로 리스트를 순서대로 순회
        foreach (Vector2Int pos in path) //경로의 각 위치에 대해 반복
        {
            // 해당 위치의 타일을 가져오기
            //  - GridManager에게 “이 좌표에 해당하는 타일 오브젝트를 달라”고 요청합니다.
            //  - GridManager 내부에서는 2차원 배열을 통해 해당 좌표의 Tile을 찾아 반환합니다.
            Tile tile = gridManager.f_GetTileInBounds(pos);

            // 이 좌표에 타일이 없다면 tile이 null일 수 있기 때문에, 널 체크 후에 색을 변경합니다.
            if (tile != null)
            {
                // Tile 스크립트에 정의된 메소드로, 해당 타일의 SpriteRenderer 색상을 파랑색으로 바꿉니다.
                tile.f_SetColor(Color.blue);
            }

            // 지정된 시간만큼 대기
            // fStepDelaySeconds 에 지정된 시간 동안 기다렸다가, 다음 좌표로 넘어가 경로를 계속 칠합니다.
            // “한 번에 모든 타일이 바뀌는 것”이 아니라 한 칸씩, 차례대로 색이 칠해지는 애니메이션 효과를 얻을 수 있습니다.
            yield return new WaitForSeconds(fStepDelaySeconds);
        }

    }
}
