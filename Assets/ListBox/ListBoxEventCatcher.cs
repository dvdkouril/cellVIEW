/* ListBox 1.1                          */
/* April 1, 2015                        */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* games, components and freelance work */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ListBoxEventCatcher : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public ListBox listBox;

    public void OnPointerDown(PointerEventData data)
    {
        listBox.OnPointerDown(data);
    }

    public void OnPointerUp(PointerEventData data)
    {
        listBox.OnPointerUp(data);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        listBox.OnBeginDrag(data);
    }
    public void OnDrag(PointerEventData data)
    {
        listBox.OnDrag(data);
    }
    public void OnEndDrag(PointerEventData data)
    {
        listBox.OnEndDrag(data);
    }

}
