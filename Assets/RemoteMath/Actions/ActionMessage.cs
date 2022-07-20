namespace RemoteMath.Actions
{
    public class ActionMessage
    {
        public byte Action { get; private set; }
        public byte[] Data { get; private set; }

        public ActionMessage(byte actionByte, byte[] data)
        {
            Action = actionByte;
            Data = data;
        }

        public byte[] Encode()
        {
            byte[] buffer = new byte[1 + Data.Length];
            buffer[0] = Action;
            Data.CopyTo(buffer, 1);
            return buffer;
        }
    }
}