using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Block : GameUnit
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Type type;
    [SerializeField]private int col;
    [SerializeField]private int row;
    [SerializeField]private float speed = 0.3f;
    [SerializeField] private GameObject directionImage;
    private float timeElasticity = 0.1f;
    public Type BlockType => type;
    private bool canTouch;
    public bool CanTouch=>canTouch;
    void Awake()
    {
        canTouch=true;
    }
    public Vector2Int GetRowAndColumn()
    {
        return new Vector2Int(row,col);
    }
    public void SetBlockRowAnCol(int row, int col)
    {
        this.row=row;
        this.col=col;
    }
    public void SetDirection(Type type)
    {
        this.type=type;
        switch (type)
        {
            case Type.LEFT:
            directionImage.transform.rotation= Quaternion.Euler(0,0,180);
            break;
            case Type.RIGHT:
            directionImage.transform.rotation= Quaternion.Euler(0,0,0);
            break;           
            case Type.UP:
            directionImage.transform.rotation= Quaternion.Euler(0,0,90);
            break;
            case Type.DOWN:
            directionImage.transform.rotation= Quaternion.Euler(0,0,270);
            break;                        
            default:
            break;
        }
    }

    public void MovetoPos(Vector2 pos,bool disableSelf,Block fartestBlock)
    {
        Debug.Log("Move"+pos);
        canTouch=false;
        transform.DOKill();
        transform.DOLocalMove(pos, Vector3.Distance(this.transform.position,pos)/speed).OnComplete(() =>
        {
            if (disableSelf)
            {
                Destroy(gameObject);
                //gameObject.SetActive(false);
            }
            else
            {
                // If we're stopping because of a non-block obstacle (e.g. rotator cell),
                // `fartestBlock` can be null.
                if (fartestBlock != null)
                {
                    fartestBlock.transform.DOPunchPosition(pos.normalized*0.1f, timeElasticity, 1);
                }
            }
            canTouch=true;
        });
    }

}
public enum Type
{
    LEFT=0,
    RIGHT=5,
    UP=10,
    DOWN=20,
}
