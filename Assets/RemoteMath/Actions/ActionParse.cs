using System.Text;

namespace RemoteMath.Actions
{
    public static class ActionParse
    {
        // System
        public static void PuzzleConnect(byte[] data, out string code)
        {
            code = Encoding.ASCII.GetString(data);
        }

        public static void PuzzleReconnect(byte[] data, out string token)
        {
            token = Encoding.ASCII.GetString(data);
        }

        public static void PuzzleToken(byte[] data, out string token)
        {
            token = Encoding.ASCII.GetString(data);
        }

        public static void PuzzleLog(byte[] data, out string logCode)
        {
            logCode = Encoding.ASCII.GetString(data);
        }

        // Game
        public static void BombDetails(byte[] data, out int batteries, out int ports)
        {
            var endianBuffer = new EndianBuffer(data, false);
            batteries = endianBuffer.ReadInt32();
            ports = endianBuffer.ReadInt32();
        }

        public static void PuzzleCode(byte[] data, out string code)
        {
            code = Encoding.ASCII.GetString(data);
        }

        public static void PuzzleFruits(byte[] data, out byte[] fruits)
        {
            fruits = data;
        }

        public static void PuzzleSolution(byte[] data, out int press1, out int press2, out string calc, out byte light)
        {
            var endianBuffer = new EndianBuffer(data, false);
            press1 = endianBuffer.ReadInt32();
            press2 = endianBuffer.ReadInt32();
            calc = endianBuffer.ReadString();
            light = endianBuffer.ReadByte();
        }

        public static void PuzzleTwitchMode(byte[] data, out int twitchId)
        {
            var endianBuffer = new EndianBuffer(data, false);
            twitchId = endianBuffer.ReadInt32();
        }

        public static void PuzzleTwitchCode(byte[] data, out string code)
        {
            code = Encoding.ASCII.GetString(data);
        }

        public static void PuzzleTwitchConfirmCode(byte[] data, out string code)
        {
            code = Encoding.ASCII.GetString(data);
        }
    }
}