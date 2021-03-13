using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class CardPostionInfo
{
    [Range(-15, 15)]
    public float _zRotate = 0f;

    [Range(-50, 50)]
    public float _offsetY = 0f;
}

public class CardDrager : MonoBehaviour
    , IDragHandler, IEndDragHandler, IBeginDragHandler
{
    //滑动方向定义
    public const int LEFT = 0;
    public const int RIGHT = 1;

   [Title("中心两边卡片缩进像素", "#FF4F63")]
    [Range(0, 100)]
    public float _shrinkCenter = 10f;

     [Title("缩小图的比例", "#FF4F63")]
    [Range(0.8f, 1f)]
    public float MinScaleRate = 0.96f;

     [Title("缩小图的堆叠的宽度比例", "#FF4F63")]
    [Range(0.1f, 0.6f)]
    public float MinWidthRate = 0.1f;

   [Title("报纸旋转角度和上下偏移量", "#FF4F63")]
    public List<CardPostionInfo> _RotateAndOffsetList;

     [Title("惯性系数", "#FF4F63")]
    [Range(0.1f, 0.3f)]
    public float Inertia = 0.135f;//手指结束滑动惯性系数

     [Title("滑动速度基数", "#FF4F63")]
    [Range(1f, 5f)]
    public float Speed = 1f;//滑动速度基数
    private float AutoSpeed = 1f;

     [Title("滑动加速度", "#FF4F63")]
    [Range(1, 3)]
    public float Acceleration = 1f;//滑动的加速度

    private float Elsic = 0.15f;//卡牌边缘和中心点的偏移位置比例判断
    public GameObject Template;
    public GameObject _UIPool;
    public bool DragEnable { get; set; }
    [HideInInspector]
    public bool _isEditorChange = false;

    private float _cardWidth = 1;
    private float _cardJudgeInCenterPosX = 1;

    private float _scaleStart;
    private float _minSpeed;

    private Vector3 _centerCardPos = Vector3.zero;
    private int _centerIndex;
    private int _cardsCount;
    private float _InterTiaDelta = 0;

    private float _MoveToPostion;//本次要滑动到的目标位置
    private float _curPosX;
    private float _FromPosX;
    private int _autoDragIndex = -1;
    private int _autoDragDirection = 0;
    private bool _isStartCor = false;
    // [Title("中心点停止判断范围", "#FF4F63")]
    private float _toCenterRange = 5;

    private List<CardDragElement> _cardList;

    private int _cardIndex = 1;
    private bool _isEditorMode = false;
#if UNITY_EDITOR
    void OnValidate()
    {
        _isEditorChange = true;
    }
#endif

    void Awake()
    {
        this.init();
    }

    private void init()
    {
        int childCount = this.transform.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            if (child.Equals(this._UIPool))
                continue;
            GameObject.Destroy(child);
        }

        this._UIPool.gameObject.SetActive(false);
        DragEnable = true;

        _minSpeed = MinScaleRate * MinWidthRate;
        _cardWidth = Template.GetComponent<RectTransform>().sizeDelta.x;
        this._cardJudgeInCenterPosX = (this._cardWidth - this._shrinkCenter) * _minSpeed;
        _scaleStart = _cardWidth - this._shrinkCenter;
    }

    void Start()
    {
        this.SetCardList(16, 0);
    }

    public void SetCardList(int count, int centerIndex)
    {
        //初始化数量和当前剧中的下标
        this._cardIndex = 1;
        if (null == _cardList)
            _cardList = new List<CardDragElement>();

        var cardCount = _cardList.Count;
        for (var i = 0; i < count; i++)
        {
            if (i >= cardCount)
            {
                var card = new CardDragElement();
                _cardList.Add(card);
            }
            _cardList[i].Name = "Card" + i;
            _cardList[i].DataIndex = i;
        }

        _cardsCount = count;
        InitCardPositon(centerIndex);
    }

    private void InitCardPositon(int cardIndex)
    {
        if(cardIndex >= _cardsCount)
            return;
        _centerIndex = cardIndex;
        _centerCardPos = Vector3.zero;
        var count = _cardsCount;
        Vector3 pos;
        float scale;
        for (var i = 0; i < count; i++)
        {
            var card = _cardList[i];
            pos = GetCardPos(i);
            scale = GetCardScale(pos.x);
            this.SetCardRotation(pos.x, i);
            if (null == card.Card)
            {
                CreatCardObj(card, i + 1);
                if (null != card.Card && null != card.Card.transform)
                {
                    card.SetSiblingIndex(this.GetSortOrder(i));
                }
            }

            card.localScale = new Vector3(scale, scale, scale);
            card.localPosition = pos;
            card.Card.doMoveAndScale(pos.x, this._isEditorMode);
        }//end for
        this.SetCardClickEnable(true);
    }

    private void CreatCardObj(CardDragElement ele, int index)
    {
        if (null != ele.Card)
            return;

        GameObject obj = GameObject.Instantiate(Template);
        obj.name = index.ToString();
        ele.Card = CreateCacheElement(obj);
        ele.Card?.transform.SetParent(this.transform, false);
    }

    private CardDragCacheElement CreateCacheElement(GameObject obj)
    {
        CardDragCacheElement ele = obj.GetComponent<CardDragCacheElement>();
        if (this._cardIndex <= _RotateAndOffsetList.Count)
        {
            ele.init(_cardIndex, this, this._RotateAndOffsetList[_cardIndex - 1], this._cardWidth);
            this._cardIndex++;
        }
        return ele;
    }


    private float _BeginTime;
    public void OnBeginDrag(PointerEventData eventData)
    {
        this._autoDragIndex = -1;
        this.SetCardClickEnable(false);
        this._BeginTime = Time.time; ;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var delta = eventData.delta.x;// * this._deltaRate;
        UpdateDrag(delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float delttime = 0;
        float beginAndEndTimeDesc = Time.time - this._BeginTime;
        Vector2 deltaPos = Vector2.zero;
        if (beginAndEndTimeDesc < 0.15f)
        {
            deltaPos = eventData.position - eventData.pressPosition;
            delttime = beginAndEndTimeDesc;
        }
        else
        {
            deltaPos = eventData.delta;
            delttime = Time.deltaTime;
        }

        float totleDelta = Mathf.Sqrt(deltaPos.x * deltaPos.x + deltaPos.y * deltaPos.y);
        if (delttime > 0 && totleDelta != 0)
        {
            float xOffset = eventData.position.x - eventData.pressPosition.x;
            if (xOffset < 0)
                totleDelta = -totleDelta;
            this._InterTiaDelta = totleDelta / delttime * this.Inertia;//得到最终的惯性速度                                   
        }
        else
        {
            this._InterTiaDelta = 0;
        }
        this.EndDrag();
    }

    private void EndDrag()
    {
        float value = this._InterTiaDelta / 1000f;
        float absValue = Mathf.Abs(value);
        if (absValue <= 0.05f)
        {
            var miniOffset = _cardList[_centerIndex].localPosition.x;
            int endCenterIndex = this._centerIndex;
            float offsetRate = Mathf.Abs(miniOffset / (this._cardWidth * 0.5f));
            if (offsetRate >= this.Elsic)
            {
                int offsetIndex = miniOffset < 0 ? 1 : -1;
                endCenterIndex += offsetIndex;

                if (endCenterIndex < 0)
                    endCenterIndex = 0;
                else if (endCenterIndex >= this._cardsCount)
                    endCenterIndex = this._cardsCount - 1;
            }

            this.AutoSpeed = this.Speed;
            this.AutoDragTo(endCenterIndex);
        }
        else
        {
            int needMoveCount = 0;
            if (value > 1f)
                needMoveCount = Mathf.CeilToInt(value);
            else if (value < -1f)
                needMoveCount = Mathf.FloorToInt(value);
            if (needMoveCount == 0)
            {
                var miniOffset = _cardList[_centerIndex].localPosition.x;
                float offsetRate = Mathf.Abs(miniOffset / (this._cardWidth * 0.5f));
                if (offsetRate >= this.Elsic)
                    needMoveCount = value > 0 ? 1 : -1;
            }

            int finalIndex = _centerIndex - needMoveCount;
            if (finalIndex > this._cardsCount - 1)
                finalIndex = this._cardsCount - 1;
            else if (finalIndex < 0)
                finalIndex = 0;

            this.ComputeAutoSpeed(finalIndex);
        
            this.AutoDragTo(finalIndex);
        }
    }


    public void AutoDragTo(int autoIndex)
    {
        this._isStartCor = true;
        var cardAutoX = _cardList[autoIndex].localPosition.x;
        _autoDragDirection = cardAutoX < 0 ? RIGHT : LEFT;;
        _MoveToPostion = Mathf.Abs(cardAutoX);
        this._curPosX = cardAutoX;
        this._FromPosX = 0;
        this._autoDragIndex = autoIndex;
    }

    void LateUpdate()
    {
        if (this._isStartCor)
            this.DoSortLayer(false);
    }

    void Update()
    {
        if (this._autoDragIndex >= 0)
        {
            //计算每一帧滑动的速度
            float Speed = this.AutoSpeed;
            if (this._MoveToPostion > 0)
            {
                float curCountRate = 1f - Mathf.Abs(this._curPosX) / (float)this._MoveToPostion;
                if (curCountRate > 1f)
                    curCountRate = 1f;
                float AccleteRate = (float)(1f - Math.Sqrt(1 - Math.Pow(curCountRate, 2))) *this.Acceleration; //可以增加一个加速度系数 
                Speed = this.AutoSpeed * (1 + AccleteRate);
            }

            this._curPosX = _cardList[this._autoDragIndex].localPosition.x;
            float deltaVal = -Mathf.Lerp(this._FromPosX, this._curPosX, Speed * Time.deltaTime);

            if (deltaVal < 0 && deltaVal > -_toCenterRange)
                deltaVal = -_toCenterRange;
            else if (deltaVal > 0 && deltaVal < _toCenterRange)
                deltaVal = _toCenterRange;
            UpdateDrag(deltaVal);

            var cardAutoX = _cardList[this._autoDragIndex].localPosition.x;
            if ((_autoDragDirection == RIGHT && cardAutoX >= 0) ||
                (_autoDragDirection == LEFT && cardAutoX <= 0))
            {
                this._centerIndex = this._autoDragIndex;
                _cardList[this._centerIndex].localPosition = Vector3.zero;
                this.SetCardClickEnable(true);
            }
        }
    }

    public void ComputeAutoSpeed(int finalIndex)
    {
        int absCount = Mathf.Abs(finalIndex - this._centerIndex);
        this.AutoSpeed = (absCount * this.Speed);
        if (this.AutoSpeed == 0)
            this.AutoSpeed = this.Speed;
    }

    public void onClickCardCallBack(int uid)
    {
        //点击卡牌进行自动滑动
        int autoIndex = -1;
        var count = _cardsCount;
        for (var i = 0; i < count; i++)
        {
            var card = _cardList[i];
            if (card.Card != null && card.Card.Index == uid)
            {
                autoIndex = i;
                break;
            }
        }//end for

        if (autoIndex >= 0 && autoIndex != this._centerIndex)
        {
            //进行滑动 
            this.ComputeAutoSpeed(autoIndex);
            this.AutoDragTo(autoIndex);
        }
        else if (autoIndex == this._centerIndex)
        {
            if (this._autoDragIndex >= 0)
                return;
            //通知正中间点击事件
        }
    }

    public void UpdateDrag(float delta)
    {
        if (false == CheckDrag(delta))
            return;
        _centerCardPos = _cardList[_centerIndex].localPosition;
        _centerCardPos.x += delta;
        _cardList[_centerIndex].localPosition = _centerCardPos;
        var scale = GetCardScale(_centerCardPos.x);
        this.SetCardRotation(_centerCardPos.x, this._centerIndex);
        _cardList[_centerIndex].localScale = new Vector3(scale, scale, scale);
        this.SetCardInnerIconPosAndScale(this._centerIndex);

        var prePos = _centerCardPos;
        var pos = Vector3.zero;
        var count = _cardsCount;
        for (int i = 0; i < count; ++i)
        {
            if (i == this._centerIndex)
                continue;
            pos = this.GetCardPos(i);
            scale = GetCardScale(pos.x);
            this.SetCardRotation(pos.x, i);
            _cardList[i].localScale = new Vector3(scale, scale, scale);
            _cardList[i].localPosition = pos;
            this.SetCardInnerIconPosAndScale(i);
        }

        int oldCenterIndex = this._centerIndex;
        var moveLeft = _centerCardPos.x < 0;
        if (moveLeft)
        {
            UpdateDragLeft(delta);
        }
        else
        {
            UpdateDragRight(delta);
        }

        if (oldCenterIndex != this._centerIndex && this._isStartCor == false)
        {
            //中心卡发生变化，调整卡牌层级关系
            _isStartCor = true;
        }
    }

    private void UpdateDragRight(float delta)
    {
        var centerLeft = _centerIndex - 1;
        if (centerLeft >= 0)
        {
            if (_cardList[centerLeft].localPosition.x >= -this._cardJudgeInCenterPosX)
            {
                _centerIndex = centerLeft;
                _centerCardPos = _cardList[_centerIndex].localPosition;
            }
        }
    }

    private void UpdateDragLeft(float delta)
    {
        var centerRight = _centerIndex + 1;
        if (centerRight < _cardsCount)
        {
            if (_cardList[centerRight].localPosition.x <= this._cardJudgeInCenterPosX)
            {
                _centerIndex = centerRight;
                _centerCardPos = _cardList[_centerIndex].localPosition;
            }
        }
    }//end function

    private Vector3 GetCardPos(int index)
    {
        var prePos = _centerCardPos;
        var pos = prePos;
        int indexOffset = this._centerIndex - index;
        float nextX = GetCardPositionXByOffsetInner(-indexOffset) + _centerCardPos.x * _minSpeed;
        if (indexOffset == 1)
        {
            pos.x -= _cardWidth;
            pos.x += this._shrinkCenter;
            if (pos.x < nextX)
                pos.x = nextX;
        }
        else if (indexOffset == -1)
        {
            pos.x += _cardWidth;
            pos.x -= this._shrinkCenter;
            if (pos.x > nextX)
                pos.x = nextX;
        }
        else
        {
            pos = _cardList[index].localPosition;
            pos.x = GetCardPositionXByOffsetInner(-indexOffset) + _centerCardPos.x * _minSpeed;
        }
        return pos;
    }

    private float GetCardPositionXByOffsetInner(int offset)
    {
        if (offset < 0)
            return -GetCardPositionXByOffsetInner(-offset);

        if (0 == offset)
            return 0;
        
        return _cardWidth + (offset - 1) * _cardWidth * MinWidthRate - this._shrinkCenter;
    }

    private float GetCardScale(float posx)
    {
        var dis = Mathf.Abs(posx);
        if (dis < _scaleStart)
        {
            float MinDes = 1 - MinScaleRate;
            float cur = this._scaleStart - dis;
            float scale = cur / this._scaleStart * MinDes + MinScaleRate;
            return scale;
        }
        return MinScaleRate;
    }

    private bool CheckDrag(float delta)
    {
        if (!DragEnable)
            return false;
        float xCur = _cardList[this._centerIndex].localPosition.x;
        if (delta > 0 && xCur > 0 && this._centerIndex == 0)
            return false;
        else if (delta < 0 && xCur < 0 && this._centerIndex == this._cardsCount - 1)
            return false;
        return true;
    }

    private void SetCardRotation(float posx, int index)
    {
        CardPostionInfo pos = this._RotateAndOffsetList[index];
        float z = pos._zRotate;
        float y = pos._offsetY;
        var dis = Mathf.Abs(posx);
        if (dis < _scaleStart && this._isEditorMode == false)
        {
            float cur = this._scaleStart - dis;
            z = dis / this._scaleStart * z;
            y = dis / this._scaleStart * y;
        }

        if (this._cardList[index].Card != null)
        {
            this._cardList[index].Card.setRotateZ(z);
            this._cardList[index].Card.setOffsetY(y);
        }
    }

    private void SetCardInnerIconPosAndScale(int index)
    {
        this._cardList[index].Card.doMoveAndScale(this._cardList[index].localPosition.x, this._isEditorMode);
    }

    public void SetCardClickEnable(bool isEn)
    {
        var count = _cardList.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_cardList[i].Card == null)
                continue;
            _cardList[i].Card.setEnable(isEn);
        }

        if (isEn)
        {
            this.DoSortLayer(true);
            this._autoDragIndex = -1;
        }
    }

    void DoSortLayer(bool isAll = false)
    {
        int left = this._centerIndex - 1;
        int right = this._centerIndex + 1;
        var count = _cardsCount;
        for (int i = 0; i < count; ++i)
        {
            if (i == left || i == right || i == _centerIndex || isAll)
                _cardList[i].SetSiblingIndex(this.GetSortOrder(i));
        }
        this._isStartCor = false;
    }

    private int GetSortOrder(int index)
    {
        int descInt = index - this._centerIndex;
        if (descInt < 0)
            descInt = -descInt;
        return this._cardsCount - descInt;
    }




    //Editor相关
    public void RemovePreview()
    {
        int count = this.transform.childCount;
        List<GameObject> deslist = new List<GameObject>();
        for (int i = 0; i < count; ++i)
        {
            Transform child = transform.GetChild(i);
            if (child.name.Equals("Pool"))
                continue;
            deslist.Add(child.gameObject);
        }

        foreach (GameObject obj in deslist)
        {
            GameObject.DestroyImmediate(obj);
        }
        deslist = null;
        this._UIPool.SetActive(true);
        this._cardList = null;
    }

    public void EditorPreview()
    {
        this._isEditorMode = true;
        this.RemovePreview();
        this.init();
        int count = this._RotateAndOffsetList.Count;
        this.SetCardList(count, count / 2);
        this.InitCardPositon(this._centerIndex);
    }

    public void RefreshEditor()
    {
        if (this._cardList == null || this._cardList.Count == 0)
            return;
        int count = this._cardList.Count;
        for (var i = 0; i < count; i++)
        {
            if (this._cardList[i].Card == null)
                return;
            var card = _cardList[i];
            var pos = GetCardPos(i);
            var scale = GetCardScale(pos.x);
            this.SetCardRotation(pos.x, i);

            card.localScale = new Vector3(scale, scale, scale);
            card.localPosition = pos;
            card.Card.doMoveAndScale(pos.x, this._isEditorMode);
        }
    }

}//end class



