/* Priority Queue 
 * 우선 순위 큐는 먼저 들어온 순서가 아니라 우선수위 가 더 좋은 요소 를 먼저 꺼내는 자료구조이다
 * 여기서는 숫자가 작을수록 더 높은 우선순위를 가진다고 가정한다. 
 * 예 A*알고리즘에서 f(n) 값이 더 작은 노드를 먼저 꺼내 탐색할 때 사용한다
 */

//List 와 같은 제네릭 컬렉션 을 사용하기 위해 필요한 네임스페이스를 가져온다
// PriorityQueue 내부에서 List 를 사용할 것이기 때문에 반드시 포함해야 한다

using UnityEngine;
using System.Collections.Generic;

// 우선순위 가 가장 낮은 요소를 먼저 꺼내는 Priority Queue 자료구조 
// A* 알고리즘에서 openSet 역할을 할 때 사용
// TItem 은 큐에 저장되는 요소의 타입을 의미함
public class PriorityQueue<TItem>
{
    // 멤버 변수정의 
    // 저장할 값 item 우선순위 priority 
    //priority 작을수록 더 높은 우선운위 를 가진다고 가정한다 
    private List<(TItem item, int priority)> listElemnts = new List<(TItem item, int priority)>();

    // 현재 큐 안에 저장된 요소의 개수를 관리하는 메소드 
    // A* 알고리즘에서는 openSet 비었는지 여부를 확인할 때 사용한다
    public int Count
    {
        get { return listElemnts.Count; }   
    }

    // 새로운 요소를 우선순위와 함께 큐에 추가하는 메소드
    // newitem  우선운위를 매기고 싶은 실제값 
    // newPriority 우선순위 값이 작을수록 먼저 처리된다 
    public void Enqueue(TItem newitem, int newPriority)
    {
        // 값 우선순위 튜플을 리스트 끝에 추가한다 
        listElemnts.Add((newitem, newPriority));    

    }



    // 큐안에서 우선순위가 가장 낮은 요소를 찾아 꺼낸 뒤,
    // 그 요소를 리스트에서 제거하고 item 만 반환하는 메소드 
    // A* 알고리즘에서 가장 fScore 가 낮은 노드 를 선택할 때 사용

    public TItem Dequeue()
    {
        // 우선순위 가 가장 좋은 요소의; 인덱스
        // 일단 0번 요소를 기준으로 두고 이후 더 좋은 것이 있으면 교체한다 
        int nBestIndex = 0;

        // 1번 인덱스부터 끝까지 검사하면서 
        // 현재까지 찾은 요소보다 더 낮은 priority 를 가진 요소를 찾는다 
        for (int i = 1;  i < listElemnts.Count; i++)
        {
            if (listElemnts[i].priority < listElemnts[nBestIndex].priority)
            {
                nBestIndex = i;
            }
        }

        // 가장 우선순위가 높은 요소의 item 값을 따로 저장해 둠
        TItem bestItem = listElemnts[nBestIndex].item;

        // 리스트에서 해당 요소 제거 
        listElemnts.RemoveAt(nBestIndex);

        // 실제 값만 반환
        return bestItem;
    }

}
