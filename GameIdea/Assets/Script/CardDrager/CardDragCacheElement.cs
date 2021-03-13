using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;

public class CardDragCacheElement : UIBehaviour
    , IPointerClickHandler
{
    [Range(0, 1f)]
    public float _MaxScaleRate = 0.8f;

    [Tooltip("内部图片最大的缩放比例")]
    public float maxIconScale = 1.5f;

    [Tooltip("内部图片停留中心点时的缩放比例")]
    public float centerIconScale = 1.2f;

    [Tooltip("内部图片最大的偏移量")]
    public float _moveDelta = 50;
    private float _moveX = 0;
    private float _tanZ = 0;
    private float _CtanZ = 0;

    [HideInInspector]
    public int Index;
    [HideInInspector]
    public CardDrager _view;

    public Transform _rotateTran;
    public Transform _iconParentTran;
    public Transform _iconTran;
  

    private float _rotateZ = 0;
    private float _cardWidth = 0;
    private bool _isEnable = true;

    public void init(int idx, CardDrager view, CardPostionInfo rotateZ, float cwidth)
    {
        _cardWidth = cwidth;
        this.Index = idx;
        _view = view;

        int imgIndex = idx % 6 + 1;
        string path = "Card/" + imgIndex;
        Sprite sp = Resources.Load<Sprite>(path);
        this._iconTran.GetComponent<Image>().sprite = sp;
    }

    public void setOffsetY(float yoffset)
    {
        float movey = _CtanZ * yoffset;
        this._rotateTran.localPosition = new Vector3(0, movey, 0);
    }

    public void setRotateZ(float zRotate)
    {
        this._rotateZ = zRotate;
        this._rotateTran.localEulerAngles = new Vector3(0, 0, this._rotateZ);
        this._iconParentTran.localEulerAngles = new Vector3(0, 0, -this._rotateZ);
        this._iconTran.localEulerAngles = new Vector3(0, 0, this._rotateZ);

        //计算出 根据 旋转计算出TweenPos的X，Y
        float z = (_rotateZ * (Mathf.PI)) / 180;
        this._moveX = Mathf.Cos(z) * this._moveDelta;
        _tanZ = Mathf.Tan(z);
        this._CtanZ = Mathf.Cos(z);
    }

    public void SetSortOrder(int order)
    {
        this.transform.SetSiblingIndex(order);
    }

    public void doMoveAndScale(float localPosX, bool isEditor)
    {
        //根据和中心点0的距离比例，计算出当前icon的偏移量和缩放大小
        float absPos = Mathf.Abs(localPosX);
        float descRate = this.maxIconScale - this.centerIconScale;
        float innerRate = 1 - this._MaxScaleRate;
        float distanceX = Mathf.Abs(localPosX);

        float curRate = distanceX / this._cardWidth;
        float curScale = 1f;//当前的图片缩放大小
        float curPosX = 0f;//当前的图片位置x
        if (curRate >= 0 && curRate < innerRate)
        {
            curScale = Mathf.Lerp(centerIconScale, maxIconScale, 1f - (innerRate - curRate) / innerRate);
            curPosX = Mathf.Lerp(0, this._moveX, 1f - (innerRate - curRate) / innerRate);
        }
        else if (curRate <= 1f && curRate >= innerRate)
        {
            curScale = Mathf.Lerp(1f, maxIconScale, 1f - (curRate - innerRate) / this._MaxScaleRate);
            curPosX = Mathf.Lerp(1f, this._moveX, 1f - (curRate - innerRate) / this._MaxScaleRate);
        }

        Vector3 pos = this._iconParentTran.localPosition;
        pos.x = curPosX;
        if (pos.x > this._moveX)
            pos.x = this._moveX;

        if (localPosX < 0)
            pos.x = -pos.x;

        float movey = _tanZ * curPosX;
        if (movey > 0)
            pos.y = -movey;
        else
            pos.y = movey;

        this._iconParentTran.localScale = new Vector3(curScale, curScale, curScale);
        this._iconParentTran.localPosition = pos;
    }

    public void setEnable(bool ieEn)
    {
        _isEnable = ieEn;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this._isEnable == false)
            return;
        this.ClickCard();
    }
    private void ClickCard()
    {
        this._view.onClickCardCallBack(this.Index);
    }
}




