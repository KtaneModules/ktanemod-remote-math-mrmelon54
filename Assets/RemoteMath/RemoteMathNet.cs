using System;
using System.Collections.Generic;
using System.Threading;
using kcp_csharp;
using RemoteMath.Actions;

namespace RemoteMath
{
    internal class RemoteMathNet
    {
        private const string ServerAddress = "localhost";
        private const int ServerPort = 13695;

        private readonly UdpSession _conn;
        private Thread _writeProc;
        private Thread _readProc;
        private bool _killMode;
        private readonly Queue<ActionMessage> _toWrite = new Queue<ActionMessage>();

        public class MessageEventArgs : EventArgs
        {
            public ActionMessage Message { get; private set; }

            public MessageEventArgs(ActionMessage message)
            {
                Message = message;
            }
        }

        public event EventHandler<MessageEventArgs> Message;

        private void OnMessage(ActionMessage message)
        {
            if (Message != null) Message.Invoke(this, new MessageEventArgs(message));
        }

        public event EventHandler<EventArgs> Disconnect;

        private void OnDisconnect()
        {
            if (Disconnect != null) Disconnect.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> Error;

        private void OnError()
        {
            if (Error != null) Error.Invoke(this, EventArgs.Empty);
        }

        public RemoteMathNet()
        {
            _conn = new UdpSession
            {
                AckNoDelay = true,
                WriteDelay = false
            };
        }

        public bool IsAlive()
        {
            return _conn != null && _conn.IsConnected;
        }

        public void Connect(string token)
        {
            try
            {
                _conn.Connect(ServerAddress, ServerPort);
                _writeProc = new Thread(InternalWriteProc) {IsBackground = true};
                _writeProc.Start();
                _readProc = new Thread(InternalReadProc) {IsBackground = true};
                _readProc.Start();
                Send(string.IsNullOrEmpty(token) ? ActionFactory.CreateEmpty(ActionByte.PuzzleCreate) : ActionFactory.PuzzleReconnect(token));
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void Close()
        {
            _killMode = true;
            if (_writeProc != null)
            {
                _writeProc.Join(TimeSpan.FromSeconds(5));
                if (_writeProc.IsAlive) _writeProc.Abort();
                _writeProc = null;
            }

            if (_readProc != null)
            {
                _readProc.Join(TimeSpan.FromSeconds(5));
                if (_readProc.IsAlive) _readProc.Abort();
                _readProc = null;
            }

            _conn.Close();
            OnDisconnect();
        }

        public void Send(ActionMessage message)
        {
            _toWrite.Enqueue(message);
        }

        private void InternalWriteProc()
        {
            while (true)
            {
                _conn.Update();
                if (_killMode) return;
                using (var eWrite = _toWrite.GetEnumerator())
                    while (eWrite.MoveNext())
                    {
                        if (_killMode) return;
                        if (eWrite.Current == null) goto writeOuter;
                        var b = eWrite.Current.Encode();
                        _conn.Send(b, 0, b.Length);
                    }
            }

            writeOuter:
            OnError();
        }

        private void InternalReadProc()
        {
            var buffer = new byte[1500];
            int pos = 0;
            while (true)
            {
                _conn.Update();
                if (_killMode) return;
                var n = _conn.Recv(buffer, 0, buffer.Length);
                if (n == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (n < 0) break;
                pos += n;

                if (pos <= 0) continue;
                var endianBuffer = new EndianBuffer(buffer, false);
                int len = endianBuffer.ReadInt32();

                if (pos <= len + 2) continue;
                var action = endianBuffer.ReadByte();
                var slice = new byte[len];
                Array.Copy(buffer, 5, slice, 0, len);
                pos -= len + 5;
                Array.Copy(buffer, len + 5, buffer, 0, pos);
                OnMessage(new ActionMessage(action, slice));
            }

            OnError();
        }

        public bool IsClosing()
        {
            return _killMode;
        }
    }
}