using UnityEngine;
using DG.Tweening;
public class Obstacle : GameUnit
{ 
    [SerializeField] private float rotationSpeed = 30f; // Tốc độ xoay (độ/giây)

    void Start()
    {
        // Sử dụng DOTween để xoay đối tượng liên tục quanh trục Z
        transform.DORotate(new Vector3(0, 0, 360), 360f / rotationSpeed, RotateMode.FastBeyond360)
            .SetRelative(true) // Xoay tương đối so với góc hiện tại
            .SetLoops(-1, LoopType.Incremental) // Lặp vô hạn và tăng dần góc xoay
            .SetEase(Ease.Linear); // Xoay đều
    }
        void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Block>())
        {
            //GameManager_.Instance.LoseGame();
        }
    }
}
