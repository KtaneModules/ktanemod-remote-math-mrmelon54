namespace RemoteMath.Actions
{
    // ActionByte contains the protocol codes
    // <- a packet from the server
    // -> a packet to the server
    // <> a reserved id used for the web module
    public static class ActionByte
    {
        // Sync
        public const byte Ping = 0x01; // <-
        public const byte Pong = 0x02; // ->

        // System
        public const byte PuzzleCreate = 0x10; // ->
        public const byte PuzzleConnect = 0x11; // <> code char[8]
        public const byte PuzzleReconnect = 0x12; // -> token char[32]
        public const byte PuzzleInvalid = 0x13; // <-
        public const byte PuzzleToken = 0x14; // <- token char[32]
        public const byte PuzzleLog = 0x15; // <- logUrl char[]

        // Game
        public const byte BombDetails = 0x20; // -> batteries int32, ports int32
        public const byte PuzzleCode = 0x21; // <- code char[8]
        public const byte PuzzleFruits = 0x22; // -> fruits byte[8]
        public const byte PuzzleSolution = 0x23; // <> press1 int32, press2 int32, calc char[], light byte
        public const byte PuzzleSolve = 0x24; // <-
        public const byte PuzzleStrike = 0x25; // <-

        // Twitch mode
        public const byte PuzzleTwitchMode = 0x30; // -> twitchId int32
        public const byte PuzzleTwitchCode = 0x31; // <- twitchCode char[4]
        public const byte PuzzleTwitchActivate = 0x32; // <>
        public const byte PuzzleTwitchConfirmCode = 0x32; // -> confirmCode char[4]
    }
}