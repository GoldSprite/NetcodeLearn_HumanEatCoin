using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectCoinManager : MonoBehaviour
{
    public static CollectCoinManager Instance { get; private set; }
    public Transform CoinBasketCenterTrans;
    private List<Transform> coins => GameManager.Instance.Coins;
    public float GetPointRange = 1;
    public int CoinPoint { get => GameManager.Instance.CoinPoint; set => GameManager.Instance.CoinPoint=value; }

    public Animator GetPointTip;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        GameManager.Instance.Coins = coins.Where(p=>p!=null).ToList();
        var coinsToRemove = coins.Where(coin => Vector3.Distance(CoinBasketCenterTrans.position, coin.position) < GetPointRange).ToList();

        foreach (var coin in coinsToRemove)
        {
            this.coins.Remove(coin);
            Destroy(coin.gameObject);
            CoinPoint++;
            Debug.Log("��һ��");
            GetPointTip.gameObject.SetActive(true);
            GetPointTip.Play("Default", 0, 0);
        }



        //var coins = new List<Transform>(this.coins);
        //foreach (var coin in coins)
        //{
        //    var distance = Vector3.Distance(CoinBasketCenterTrans.position, coin.position);
        //    if ( distance < GetPointRange)
        //    {
        //        this.coins.Remove(coin);
        //        Destroy(coin);
        //        CoinPoint += 1;
        //    }
        //}
    }
}
