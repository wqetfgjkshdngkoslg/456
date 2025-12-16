/*
 * 하나의 타일을 표현하는 스크립트
 * 좌표와 색상은 외부에서  접근 불가능 초기화 & 색상 설정 메소드 제공
 * GridManager를 통해 타일 생성 및 관리
 */

using UnityEngine;

public class Tile : MonoBehaviour
{
    Vector2Int vGridPosition = Vector2Int.zero; // 자신의 그리드 좌표
    SpriteRenderer spriteRenderer = null;       // 색상 변경을 위한 스프라이트 렌더러 컴포넌트 

    // 현재 오브젝트에서 스프라이트 렌더러 컴포넌트를 가져옴
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // private로 선언된 내부값을 외부에서 초기화할 수 있도록 초기화 메소드 구현
    // GridManager에서 호출되는 초기화 메소드
    public void f_Initialize(Vector2Int gridPos, Color initialColor)
    {
        vGridPosition = gridPos;

        f_SetColor(initialColor);
    }


    // 타일의 표시 색상을 변경하는 메소드
    // 타일 프리팹은 Inspector창에서 보면 알 수 있듯이 Sprite Rederer 컴포넌트를 가지고 있다.
    // 타일의 색상을 변경하기 위해  Sprite Rederer 컴포넌트를 가져와 Color 값을 변경해야 한다.
    public void f_SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
