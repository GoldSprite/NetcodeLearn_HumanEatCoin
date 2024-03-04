using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using Unity.Sync.Relay.Model;
using Unity.Sync.Relay.Transport.Netcode;
using UnityEngine;
using static GoldSprite.MySyncPlayerManager.NetworkGameManager;
using Random = UnityEngine.Random;

namespace GoldSprite.TestSyncTemp
{
    public class TestSyncPropsHandler : NetworkBehaviour
    {
        private NetworkManager networkManager;
        private RelayTransportNetcode netTrans;

        private RelayRoom relayRoom;
        private RelayPlayer localPlayer;
        private RelayPlayer selfPlayer;

        public bool isInit;

        public LocalPlayerProps selfProps = new LocalPlayerProps();
        private SyncPlayerPropsVar selfProps2 = new SyncPlayerPropsVar();

        private SyncPlayerPropsVar EncodeSelfProps(LocalPlayerProps selfProps)
        {
            selfProps2.TransportId = selfProps.TransportId;
            selfProps2.PlayerName = selfProps.PlayerName;
            selfProps2.PlayerPos = selfProps.PlayerPos;
            return selfProps2;
        }

        private LocalPlayerProps DecodeSyncProps(SyncPlayerPropsVar selfProps2)
        {
            selfProps.TransportId = selfProps2.TransportId;
            selfProps.PlayerName = selfProps2.PlayerName;
            selfProps.PlayerPos = selfProps2.PlayerPos;
            return selfProps;
        }

        private NetworkVariable<SyncPlayerPropsVar> syncProps = new();
        public NetworkVariable<SyncPlayerPropsVar> SyncProps => syncProps;


        private void Start()
        {
            networkManager = NetworkManager.Singleton;
            netTrans = networkManager.GetComponent<RelayTransportNetcode>();
            relayRoom = netTrans.GetRoomInfo();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            transform.position = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        }

        private void Update()
        {
            try
            {
                if (!isInit) TestManager.Tip_Txt.text = $"{(IsLocalPlayer ? "����" : "�������")}��ʼ��������...";
                localPlayer = netTrans.GetCurrentPlayer();  //��ȡ����ڷ���ʵ��, ��ʾ�Ѽ��뷿��
                if (syncProps.Value == null || syncProps.Value.TransportId == 0)  //��ʾ��δ����transportId
                {
                    if (!isInit) TestManager.Tip_Txt.text = $"{(IsLocalPlayer ? "����" : "�������")}�Ѽ��뷿��, ��ʼ�����Identifier��...";
                    if (IsLocalPlayer)
                    {
                        selfProps.TransportId = localPlayer.TransportId;
                        selfProps.PlayerName = localPlayer.Name;
                        selfProps.PlayerPos = transform.position;
                        UploadData();
                    }
                }
                else  //�����transportId��ʼ��, ��ʼ���ݸ���
                {
                    if (!isInit)
                    {
                        isInit = true;
                        TestManager.Tip_Txt.text = $"{(IsLocalPlayer ? "����" : "����")}���Identifier��ʼ�����.";
                        if (IsLocalPlayer) TestManager.Instance.LocalPlayer = this;
                    }

                    //�ӷ���������������
                    selfProps = DecodeSyncProps(syncProps.Value);
                    //Ӧ���������
                    transform.position = selfProps.PlayerPos;

                    //������������������ط����
                    UploadData();
                }

                //Debug.Log($"{(IsServer ? "����" : "�ͻ���")}-{(IsLocalPlayer ? "����" : "�Ǳ���")}�������: [SyncTID-{syncProps.Value.TransportId}, selfTID-{selfProps.TransportId}, ], [SyncName-{syncProps.Value.PlayerName}, selfName-{selfProps.PlayerName}, ], .");
                Debug.Log($"{(IsServer ? "����" : "�ͻ���")}-{(IsLocalPlayer ? "����" : "�Ǳ���")}�������: [SyncTID-{syncProps.Value.TransportId}], [SyncPos-{syncProps.Value.PlayerPos}].");

            }
            catch (Exception)
            {
                //Debug.Log("δ���뷿��, ��CurrentPlayer");
            }
        }

        public void UploadData()
        {
            if (IsLocalPlayer)
            {
                selfProps.PlayerName = TestManager.Instance.playerInitName;
                if (IsServer)
                {
                    SyncPlayerProps_ClientRpc(EncodeSelfProps(selfProps));
                }
                else SyncPlayerProps_ServerRpc(EncodeSelfProps(selfProps));
            }
        }

        [ServerRpc]
        private void SyncPlayerProps_ServerRpc(SyncPlayerPropsVar props)
        {
            syncProps.Value = props;
        }

        [ClientRpc]
        private void SyncPlayerProps_ClientRpc(SyncPlayerPropsVar props)
        {
            if (IsLocalPlayer)
            {
                syncProps.Value = props;
            }
        }


        public void UploadMove(Vector3 vec, Rigidbody rb)
        {
            if (IsLocalPlayer)
            {
                rb.MovePosition(vec);
                selfProps.PlayerPos = transform.position;
                UploadData();
            }
        }

    }
}
