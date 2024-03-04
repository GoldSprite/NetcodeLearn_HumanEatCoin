using System;
using Unity.Netcode;
using Unity.Sync.Relay.Lobby;
using Unity.Sync.Relay.Model;
using Unity.Sync.Relay.Transport.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace GoldSprite.TestSyncTemp
{
    public class TestManager : MonoBehaviour
    {
        public static TestManager Instance;
        public NetworkManager networkManager;
        public RelayTransportNetcode netTrans;

        public Text tip_Txt;
        public static Text Tip_Txt => Instance.tip_Txt;
        public string playerGuid;
        public string playerInitName;
        public string roomUuid;

        public TestSyncPropsHandler LocalPlayer;


        private void Start()
        {
            Instance = this;
            networkManager = NetworkManager.Singleton;
            netTrans = networkManager.GetComponent<RelayTransportNetcode>();

            //playerGuid = Guid.NewGuid().ToString();

            //playerInitName = GetRandomName();
            //netTrans.SetPlayerData(playerGuid, playerInitName);
        }

        public void StartHost()
        {

            Tip_Txt.text = "����Զ�̷�����..";
            StartCoroutine(LobbyService.AsyncCreateRoom(new CreateRoomRequest()
            {
                Name = "AAA",
                Namespace = "AAA",
                MaxPlayers = 4,
                OwnerId = playerGuid,
                Visibility = LobbyRoomVisibility.Public
            }, (resp) =>
            {
                if (resp.Code == (uint)RelayCode.OK)
                {
                    netTrans.SetRoomData(resp);
                    Tip_Txt.text = "���ӷ���ɹ�.";
                    NetworkManager.Singleton.StartHost();
                    Tip_Txt.text = "����������..";
                }
            }));
        }

        public void StartClient()
        {
            // �첽��ѯ�����б�
            StartCoroutine(LobbyService.AsyncListRoom(new ListRoomRequest()
            {
                Namespace = "AAA",
                Start = 0,
                Count = 10,
                //PlayerName = "U", // ѡ��������ڷ�������ģ������
            }, (resp) =>
            {
                if (resp.Code == (uint)RelayCode.OK)
                {
                    Debug.Log("List Room succeed.");
                    if (resp.Items.Count > 0)
                    {
                        foreach (var item in resp.Items)
                        {
                            if (item.Status == LobbyRoomStatus.Ready)
                            {
                                // �첽��ѯ������Ϣ
                                StartCoroutine(LobbyService.AsyncQueryRoom(item.RoomUuid,
                                    (_resp) =>
                                    {
                                        if (_resp.Code == (uint)RelayCode.OK)
                                        {
                                            // ��Ҫ�����ӵ�Relay������֮ǰ�����ú÷�����Ϣ
                                            NetworkManager.Singleton.GetComponent<RelayTransportNetcode>()
                                                .SetRoomData(_resp);
                                            // �����Private���͵ķ��䣬��Ҫ���������л�ȡJoinCode�����������·������ú�
                                            // NetworkManager.Singleton.GetComponent<RelayTransportNetcode>().SetJoinCode(JoinCode);

                                            NetworkManager.Singleton.StartClient();

                                            roomUuid = _resp.RoomUuid;
                                            Tip_Txt.text = $"�ɹ����뷿��[{_resp.Namespace}-{_resp.Name}].";
                                        }
                                        else
                                        {
                                            Tip_Txt.text = "Query Room Fail By Lobby Service";
                                        }
                                    }));
                                break;
                            }
                        }
                    }
                    else
                    {
                        Tip_Txt.text = "û�����з���ɼ���.";
                    }
                }
                else
                {
                    Tip_Txt.text = "List Room Fail By Lobby Service";
                }
            }));
        }


        [ContextMenu("RandomName")]
        public void SetRandomName()
        {

            playerInitName = GetRandomName();
        }

        public string GetRandomName()
        {
            var nList = new string[] { "���϶�", "���ɶ���", "����", "С����", "����", "��Ů", "���񲡻���", "��Ѫ��ħ" };
            var randomName = nList[UnityEngine.Random.Range(0, nList.Length - 1)];
            return randomName + "-" + UnityEngine.Random.Range(0, 500);
        }

    }
}
