/*
 * DFSPathFinder 가 찾은 경로 정보를 시각적으로 보여주는 기능
 * 스페이스 키를 누르면 경로 타일이 초록색으로 순서대로 칠해지는 효과 발생
 * 스페이스 키 입력시 DFS로 찾은 경로를 하나씩 초록색으로 칠해가며 보여주는 스크립트
 */

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public class DFSVisualizer : MonoBehaviour
{
    DFSPathFinder dfsPathFinder = null; //경로 탐색기능
    GridManager gridManager = null;     // 그리드 경계/통로 여부/ 타일 정보 조회
    [SerializeField]
    private float fStepDelaySeconds = 0.1f; // 경로 표시 간격(초)

    // 시작 시 GridManager  DFSPathFinder 를 자동으로 찾아 연결
    private void Awake()
    {
        gridManager = GetComponent<GridManager>();  
        dfsPathFinder = GetComponent<DFSPathFinder>();  
    }



    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame)
        {
            StartCoroutine(ShowPathRoutine());
        }
    }

    /* 코루틴 ShowPathRoutine()
        코루틴이란? → 실행 도중 yield return으로 일시 중단/재개 가능한 함수
        한 칸씩 색칠하고 일정 시간 대기하는 방식으로 경로 시각화를 위해 사용함
        코루틴: IEnumerator를 반환하고, yield return을 사용하여 실행을 일시 중지할 수 있는 함수
        경로를 하나씩 초록색으로 칠해가며 보여주는 코루틴
    */

    private IEnumerator ShowPathRoutine()
    {
        List<Vector2Int> vPath = dfsPathFinder.f_GetDFSPath();

        // 예외 처리 : 경로가 없으면 아무것도 하지 않고 종료
        if(vPath == null || vPath.Count == 0 )
        {
            yield break;
        }

        // 각 좌표를 순회하면 해당 타일을 색칠하는 작업
        foreach(Vector2Int pos in vPath)
        {
            Tile tile = gridManager.f_GetTileInBounds(pos); // 해당 위치의 타일을 가져옴

            if(tile != null)
            {
                tile.f_SetColor(Color.green); // 타일을 칠함
            }
            yield return new WaitForSeconds(fStepDelaySeconds);     // 지정된 시간만큼 대기
        }
    }
}
