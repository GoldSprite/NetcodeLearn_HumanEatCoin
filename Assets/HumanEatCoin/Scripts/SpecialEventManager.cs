using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������¼�������
/// </summary>
public class SpecialEventManager : MonoBehaviour
{
    public Transform CoinBasketCenterTrans => CollectCoinManager.Instance.CoinBasketCenterTrans;
    public Transform PlayerTrans => GameManager.Instance.PlayerTrans;
    public Transform GameElementsParent => GameManager.Instance.GameElementsParent;
    private List<Transform> coins => GameManager.Instance.Coins;
    [SerializeField]
    private GameObject WindmillPrefab;

    private float nearBasketRange = 10;
    private float nearPlayerRange = 3;

    private Coroutine windmillTask;
    [SerializeField]
    [Tooltip("�糵����������/s")]
    private float windmillTaskInterval = 3f;
    [SerializeField]
    [Tooltip("�糵���ɸ���0~1")]
    [Range(0, 1)]
    private float probability = 0.4f;

    private System.Random random = new System.Random();
    private float ticker;

    private void Update()
    {
        var currentSeconds = Time.realtimeSinceStartup;
        if (currentSeconds > ticker)
        {
            RandomWindmillTask();
            ticker = currentSeconds + windmillTaskInterval;
        }
    }

    private void RandomWindmillTask()
    {
        try
        {
            //���ɷ糵
            foreach (var coin in coins)
            {
                var nearBasket = Vector3.Distance(CoinBasketCenterTrans.position, coin.position) < nearBasketRange;
                var nearPlayer = Vector3.Distance(PlayerTrans.position, coin.position) < nearPlayerRange;
                if (nearBasket && nearPlayer)
                {
                    var spawnCenter = coin.position;
                    RandomSpawnWindmill(spawnCenter);
                    break;
                }
            }
        }
        catch (Exception e) { }
    }

    private void RandomSpawnWindmill(Vector3 spawnCenter)
    {
        var isSpawn = random.NextDouble() < probability;
        Debug.Log($"���ɷ糵:{isSpawn}");
        if (isSpawn)
        {
            var obj = Instantiate(WindmillPrefab, GameElementsParent);
            var trans = obj.transform;
            var range = 1.3f;
            var randomPos = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0,
                UnityEngine.Random.Range(-1f, 1f)
                ) * range;
            trans.position = spawnCenter + randomPos;
            var dir = spawnCenter - trans.position;
            var windmill = trans.GetComponentInChildren<RotationWindmill>();
            windmill.moveVel = dir.normalized * UnityEngine.Random.Range(-1f, 1f) * 6;
            trans.localScale = Vector3.one * UnityEngine.Random.Range(0.3f, 2f);
        }
    }
}
