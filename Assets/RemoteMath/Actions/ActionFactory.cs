using System.Text;

namespace RemoteMath.Actions
{
    public static class ActionFactory
    {
        public static ActionMessage CreateEmpty(byte action)
        {
            return new ActionMessage(action, new byte[0]);
        }

        // System
        public static ActionMessage PuzzleConnect(string code)
        {
            return new ActionMessage(ActionByte.PuzzleConnect, Encoding.ASCII.GetBytes(code));
        }

        public static ActionMessage PuzzleReconnect(string token)
        {
            return new ActionMessage(ActionByte.PuzzleReconnect, Encoding.ASCII.GetBytes(token));
        }

        public static ActionMessage PuzzleToken(string token)
        {
            return new ActionMessage(ActionByte.PuzzleToken, Encoding.ASCII.GetBytes(token));
        }

        public static ActionMessage PuzzleLog(string logCode)
        {
            return new ActionMessage(ActionByte.PuzzleLog, Encoding.ASCII.GetBytes(logCode));
        }

        // Game
        public static ActionMessage BombDetails(int batteries, int ports)
        {
            var endianBuffer = new EndianBuffer(new byte[0], false);
            endianBuffer.WriteInt32(batteries);
            endianBuffer.WriteInt32(ports);
            return new ActionMessage(ActionByte.BombDetails, endianBuffer.ToArray());
        }

        public static ActionMessage PuzzleCode(string code)
        {
            return new ActionMessage(ActionByte.PuzzleCode, Encoding.ASCII.GetBytes(code));
        }

        public static ActionMessage PuzzleFruits(byte[] fruits)
        {
            return new ActionMessage(ActionByte.PuzzleFruits, fruits);
        }

        public static ActionMessage PuzzleSolution(int press1, int press2, string calc, byte light)
        {
            var endianBuffer = new EndianBuffer(new byte[0], false);
            endianBuffer.WriteInt32(press1);
            endianBuffer.WriteInt32(press2);
            endianBuffer.WriteString(calc);
            endianBuffer.WriteByte(light);
            return new ActionMessage(ActionByte.PuzzleSolution, endianBuffer.ToArray());
        }

        public static ActionMessage PuzzleTwitchMode(int twitchId)
        {
            var endianBuffer = new EndianBuffer(new byte[0], false);
            endianBuffer.WriteInt32(twitchId);
            return new ActionMessage(ActionByte.PuzzleTwitchMode, endianBuffer.ToArray());
        }

        public static ActionMessage PuzzleTwitchCode(string twitchCode)
        {
            return new ActionMessage(ActionByte.PuzzleTwitchCode, Encoding.ASCII.GetBytes(twitchCode));
        }

        public static ActionMessage PuzzleTwitchConfirmCode(string confirmCode)
        {
            return new ActionMessage(ActionByte.PuzzleTwitchConfirmCode, Encoding.ASCII.GetBytes(confirmCode));
        }
    }
}