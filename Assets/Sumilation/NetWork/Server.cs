using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server : MonoBehaviour
{
    int LOCA_LPORT = 3333;
    static UdpClient udp;
    Thread thread;

    private static volatile bool isRunning = false;

    void Start()
    {
        isRunning = true;
        udp = new UdpClient(LOCA_LPORT);
        udp.Client.ReceiveTimeout = 1000;
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    void Update()
    {
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        
        if(udp != null)
        {
            udp.Close();
        }
    }

    private static void ThreadMethod()
    {
        // ループの外側で定義 (whileループの度に初期化されないように)
        IPEndPoint remoteEP = null;
        byte[] data; // データはループ内で受け取るため定義のみ

        while (isRunning)
        {
            try
            {
                // 受信処理
                data = udp.Receive(ref remoteEP);

                // 受信したデータを処理（今回はログ出力）
                string text = Encoding.ASCII.GetString(data);
                Debug.Log($"[Received from {remoteEP.Address}:{remoteEP.Port}]: {text}");

                //Parser
                ParsePacket(data, remoteEP);
            }
            catch (SocketException e)
            {
                // SocketException 10004 (メッセージが閉じられたとき) は終了処理と見なす
                // タイムアウトや終了時以外のエラーならログに出す
                if (isRunning)
                {
                    Debug.LogError("受信エラー (Server.cs): " + e.Message);
                }
            }
            catch (System.ObjectDisposedException)
            {
                // udp.Close() によってソケットが破棄された際の正常な終了処理
                if (!isRunning)
                {
                    break;
                }
                // 予想外の場合は再スロー
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogError("受信スレッドで予期せぬエラー: " + e.Message);
                break; // ループを抜ける
            }
        }
        Debug.Log("受信スレッドを正常に終了しました。");
    }

    private static void ParsePacket(byte[] data, IPEndPoint senderEP)
    {
        // パケットは最低8バイト（MessageType 4バイト + SenderID 4バイト）必要
        if (data.Length < 8)
        {
            Debug.LogWarning("不正なサイズのパケットを受信しました。");
            return;
        }

        // 1. ヘッダー情報の読み取り
        // NetWorkStruct.cs に using System; が追加されている必要があります！
        int messageTypeInt = System.BitConverter.ToInt32(data, 0);
        int senderId = System.BitConverter.ToInt32(data, 4);

        NetWorkStruct.NetMessageType type = (NetWorkStruct.NetMessageType)messageTypeInt;

        // 2. メッセージタイプに基づいた処理の分岐
        switch (type)
        {
            case NetWorkStruct.NetMessageType.Text:
                // ペイロードはオフセット8から始まる
                byte[] payload = new byte[data.Length - 8];
                System.Buffer.BlockCopy(data, 8, payload, 0, payload.Length);

                // テキストに変換してログ出力
                string receivedText = Encoding.UTF8.GetString(payload);

                // このログは Unityのメインスレッドで実行する必要があるため、Updateなどで表示させるのが理想ですが、
                // デバッグのため、一旦ここでDebug.Logします。
                Debug.Log($"[Peer:{senderId}, Type:TEXT] {receivedText} (from {senderEP.Address})");
                break;

            case NetWorkStruct.NetMessageType.Position:
                // 💡 TODO: 座標データを受信した場合の処理
                // PlayerPositionData pos = NetWorkStruct.DeserializePosition(data.Skip(8).ToArray());
                // Debug.Log($"[Peer:{senderId}] Position: {pos.x}, {pos.y}, {pos.z}");
                break;

            case NetWorkStruct.NetMessageType.JoinRequest:
                // 💡 TODO: P2P参加要求を受信した場合の処理
                Debug.Log($"[Peer:{senderId}] 新しいピアが参加を要求しています。IP: {senderEP.Address}");
                // P2Pリストに追加する処理などを実行
                break;

            default:
                Debug.LogWarning($"Unknown message type ({type}) received from Peer:{senderId}.");
                break;
        }
    }
}

