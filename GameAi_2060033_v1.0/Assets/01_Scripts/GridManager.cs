/*
  그리드와 타일 생성을 담당하는 매니저 스크립트
  내부 데이터는 비공개, 외부는 메소드를 통해서만 접근
 */

using System;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private GameObject gTilePrefab = null;  // 각 셀을 표현할 타일 프리팹

    /* 미로만들기 : 그리드 데이터
     0 : 이동 가능(통로)
     1 : 이동 불가(벽)
     readonly : 런타임 중 코드로 값 변경 금지 → 맵 정의의 안정성 확보

     <좌표 일치 규칙>
     nGridData[y, x]에서 y=행(위→아래), x=열(왼→오른쪽)
     타일 생성 시 월드 좌표 (x, -y)로 배치하여 "아래로 갈수록 y가 증가"처럼 보이도록 함
    */
    private readonly int[,] nGridData = new int[,]
    {

        {0,1,0,1,0, 1,0,0,0,1, 0,1,0,0,0},
        {0,1,0,1,0, 1,0,1,0,1, 0,1,0,1,0},
        {0,0,0,0,0, 1,0,1,0,0, 0,1,0,1,0},
        {1,1,1,1,0, 0,0,1,1,1, 0,0,0,1,0},
        {0,0,0,1,0, 1,0,0,0,1, 1,1,0,1,0},

        {0,1,0,1,0, 1,0,1,0,0, 0,0,0,1,0},
        {0,1,0,1,0, 0,0,1,1,1, 0,1,1,1,0},
        {0,0,0,0,0, 1,0,0,0,1, 0,0,0,0,0},
        {1,1,1,1,0, 1,0,1,0,1, 1,1,1,1,0},
        {0,0,0,1,0, 0,0,1,0,0, 0,0,0,1,0},

        {0,1,0,1,1, 1,0,0,0,1, 1,1,0,1,0},
        {0,1,0,0,0, 0,0,1,0,0, 0,1,0,1,0},
        {0,1,1,1,1, 1,0,1,1,1, 0,1,0,0,0},
        {0,0,0,0,0, 0,0,0,0,1, 0,1,1,1,0},
        {1,1,1,1,1, 1,1,1,0,0, 0,0,0,0,0}

    };



    // 생성된 Tile 참조를 보관할 배열 ( 행 열 순서 ) 
    private Tile[,] tiles = null;

    // Start 메소드에서 그리드 자동 생성 호출
    private void Start()
    {
        f_GenerateGrid();   // 게임 시작 시 그리드 생성
    }

    // 미로를 생성하는 메소드 
    private void f_GenerateGrid()
    {
        int nRows = f_GetHeight();
        int nCols = f_GetWidth();
        tiles = new Tile[nRows, nCols];


        /* 미로를 만드는 반복문 알고리즘    
        2중 for문으로 nGridData 순회
          Instantiate로 타일 생성
        좌표(x, -y) 위치에 배치
        벽(1) → 검정, 길(0) → 흰색으로 색상 지정
        생성된 타일을 tiles[y, x] 에 저장
        */
        for (int y = 0; y < nRows; y++)
        {
            for (int x = 0; x < nCols; x++)
            {
                // 씬 좌표 배치: x는 오른쪽, y는 아래로 증가하도록 -y 사용
                Vector3 vSpawnPos = new Vector3(x, -y, 0.0f);   // 타일 위치
                Quaternion qRot = Quaternion.identity;          // 회전 없음

                GameObject gTileObj = Instantiate(gTilePrefab, vSpawnPos, qRot); // 타일 프리팹 생성
                Tile tile = gTileObj.GetComponent<Tile>();

                // 색상: 벽(1)은 회색, 길(0)은 흰색
                Color cInitial = (nGridData[y, x] == 1) ? Color.gray : Color.white;

                //Tile의 초기화 메소드 호출로 좌표/색상 설정
                tile.f_Initialize(new Vector2Int(x, y), cInitial);

                //좌표에 해당하는 타일을 2차원 배열에 저장
                tiles[y, x] = tile;
            }
        }
    }

    // 현재 미로의 세로 길이를 반환하는 메소드 
    private int f_GetHeight()
    {
        return nGridData.GetLength(0);
    }

    // 현재 미로의  가로 길이를 반환하는 메소드 
    private int f_GetWidth()
    {
        return nGridData.GetLength(1);
    }

    //주어진 좌표가 그리드 내부인지 여부를 반환하는 메소드
    public bool f_IsInside(Vector2Int pos)
    {
        int nWidth = f_GetWidth();
        int nHeight = f_GetHeight();

        bool isInside =
            (pos.x >= 0) && (pos.x < nWidth) &&
            (pos.y >= 0) && (pos.y < nHeight);

        return isInside;
    }

    //주어진 좌표가 이동 가능한 셀(0)인지 여부를 반환하는 메소드
    public bool f_IsWalkable(Vector2Int pos)
    {
        if (!f_IsInside(pos))
        {
            return false;
        }
        return nGridData[pos.y, pos.x] == 0;
    }


    public Tile f_GetTileInBounds(Vector2Int vPosition)
    {
        if(!f_IsInside(vPosition))
        {
            return null;
        }
        return tiles[vPosition.y, vPosition.x];
    }


}
