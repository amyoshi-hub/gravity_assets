using UnityEngine;
using System;

public class NetWorkStruct : MonoBehaviour
{
    public enum NetMessageType : int
    {
        Unknown = 0,         // 未知のタイプ
        Text = 1,            // 汎用的なテキストメッセージ
        Position = 2,        // プレイヤーの位置座標
        Rotation = 3,        // プレイヤーの回転情報
        Health = 4,          // HP (体力) の更新
        JoinRequest = 5,     // P2P参加要求
    }

    [System.Serializable]
    public struct PlayerPositionData
    {
        public float x, y, z;
    }
    [System.Serializable]
    public struct NetWorkMessage
    {
        public NetMessageType type;
        public int sessionId;
        public byte[] payload;
    }

    public static byte[] makePayload(NetMessageType type, int senderId, byte[] data)
    {
        int headerSize = 8;
        int payloadSize = data != null ? data.Length : 0;

        // 2. 全体を格納する新しい配列を作成
        byte[] finalPacket = new byte[headerSize + payloadSize];

        // 3. ヘッダー部分を書き込む (BitConverter を使用)
        // MessageType
        Buffer.BlockCopy(BitConverter.GetBytes((int)type), 0, finalPacket, 0, 4);
        // SessionID
        Buffer.BlockCopy(BitConverter.GetBytes(senderId), 0, finalPacket, 4, 4);

        // 4. ペイロードを書き込む
        if (payloadSize > 0)
        {
            Buffer.BlockCopy(data, 0, finalPacket, headerSize, payloadSize);
        }

        return finalPacket;
    }

    public static byte[] SerializePosition(PlayerPositionData pos)
    {
        // floatは4バイトなので、合計12バイト
        byte[] data = new byte[12];

        // float -> byte[]
        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, data, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, data, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, data, 8, 4);

        return data;
    }
}
