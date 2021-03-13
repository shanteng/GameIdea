using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDragElement
{
    public int DataIndex;
    public string Name;
    public CardDragCacheElement Card;

    private Vector3 _scale;
    public Vector3 localScale
    {
        get { return _scale; }
        set
        {
            _scale = value;
            if (null != Card && null != Card.transform)
            {
                Card.transform.localScale = value;
            }
        }
    }//end localScale

    private Vector3 _pos;
    public Vector3 localPosition
    {
        get { return _pos; }
        set
        {
            _pos = value;
            if (null != Card && null != Card.transform)
            {
                Card.transform.localPosition = value;
            }
        }
    }//end localPosition

    public void SetSiblingIndex(int index)
    {
        Card.SetSortOrder(index);
    }

    public void UpdateDetalis()
    {

    }
}//end class