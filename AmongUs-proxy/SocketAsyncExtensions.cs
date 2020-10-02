using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AmongUs_proxy
{
    static class SocketAsyncExtensions
    {
        private static AsyncCallback _ReceiveFromCallback, _SendToCallback, _ReceiveCallback;

        static SocketAsyncExtensions()
        {
            _ReceiveCallback = new AsyncCallback(ReceiveCallback);
            _ReceiveFromCallback = new AsyncCallback(ReceiveFromCallback);
            _SendToCallback = new AsyncCallback(SendToCallback);
        }

        public static Task<int> ReceiveFromAsync(this Socket socket, byte[] buffer, ref EndPoint endPoint)
            => ReceiveFromAsync(socket, buffer, ref endPoint, CancellationToken.None);

        public static Task<int> ReceiveFromAsync(this Socket socket, byte[] buffer, ref EndPoint endPoint, CancellationToken cancellationToken)
            => ReceiveFromAsync(socket, buffer, SocketFlags.None, ref endPoint, cancellationToken);

        public static Task<int> ReceiveFromAsync(this Socket socket, byte[] buffer, SocketFlags flags, ref EndPoint endPoint, CancellationToken cancellationToken)
            => ReceiveFromAsync(socket, buffer, 0, buffer.Length, flags, ref endPoint, cancellationToken);

        public static Task<int> ReceiveFromAsync(this Socket socket, byte[] buffer, int offset, int count, SocketFlags flags, ref EndPoint endPoint, CancellationToken cancellationToken)
        {
            TaskCompletionSource<int> taskSrc = new TaskCompletionSource<int>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskSrc.SetCanceled();
                return taskSrc.Task;
            }
            cancellationToken.Register(() =>
            {
                taskSrc.TrySetCanceled(cancellationToken);
            });
            socket.BeginReceiveFrom(buffer, offset, count, flags, ref endPoint, _ReceiveFromCallback, new object[] { taskSrc, socket, endPoint });
            return taskSrc.Task;
        }

        private static void ReceiveFromCallback(IAsyncResult ar)
        {
            var objs = (object[])ar.AsyncState;
            var taskSrc = (TaskCompletionSource<int>)(objs[1]);
            var socket = (Socket)(objs[1]);
            var endPoint = (EndPoint)(objs[2]);
            try
            {
                var receivedSize = socket.EndReceiveFrom(ar, ref endPoint);
                taskSrc.TrySetResult(receivedSize);
            }
            catch (Exception ex)
            {
                taskSrc.TrySetException(ex);
            }
        }

        public static Task<int> SendToAsync(this Socket socket, byte[] buffer, EndPoint endPoint)
            => SendToAsync(socket, buffer, endPoint, CancellationToken.None);

        public static Task<int> SendToAsync(this Socket socket, byte[] buffer, EndPoint endPoint, CancellationToken cancellationToken)
            => SendToAsync(socket, buffer, SocketFlags.None, endPoint, cancellationToken);

        public static Task<int> SendToAsync(this Socket socket, byte[] buffer, SocketFlags flags, EndPoint endPoint, CancellationToken cancellationToken)
            => SendToAsync(socket, buffer, 0, buffer.Length, flags, endPoint, cancellationToken);

        public static Task<int> SendToAsync(this Socket socket, byte[] buffer, int offset, int count, SocketFlags flags, EndPoint endPoint, CancellationToken cancellationToken)
        {
            TaskCompletionSource<int> taskSrc = new TaskCompletionSource<int>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskSrc.SetCanceled();
                return taskSrc.Task;
            }
            cancellationToken.Register(() =>
            {
                taskSrc.TrySetCanceled(cancellationToken);
            });
            socket.BeginSendTo(buffer, offset, count, flags, endPoint, _SendToCallback, new object[] { taskSrc, socket });
            return taskSrc.Task;
        }

        private static void SendToCallback(IAsyncResult ar)
        {
            var objs = (object[])ar.AsyncState;
            var taskSrc = (TaskCompletionSource<int>)(objs[1]);
            var socket = (Socket)(objs[1]);
            try
            {
                var receivedSize = socket.EndSendTo(ar);
                taskSrc.TrySetResult(receivedSize);
            }
            catch (Exception ex)
            {
                taskSrc.TrySetException(ex);
            }
        }

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer)
            => ReceiveAsync(socket, buffer, CancellationToken.None);

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, CancellationToken cancellationToken)
            => ReceiveAsync(socket, buffer, SocketFlags.None, cancellationToken);

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, SocketFlags flags, CancellationToken cancellationToken)
            => ReceiveAsync(socket, buffer, 0, buffer.Length, flags, cancellationToken);

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int count, SocketFlags flags, CancellationToken cancellationToken)
        {
            TaskCompletionSource<int> taskSrc = new TaskCompletionSource<int>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskSrc.SetCanceled();
                return taskSrc.Task;
            }
            cancellationToken.Register(() =>
            {
                taskSrc.TrySetCanceled(cancellationToken);
            });
            socket.BeginReceive(buffer, offset, count, flags, _ReceiveCallback, new object[] { taskSrc, socket });
            return taskSrc.Task;
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var objs = (object[])ar.AsyncState;
            var taskSrc = (TaskCompletionSource<int>)(objs[1]);
            var socket = (Socket)(objs[1]);
            var endPoint = (EndPoint)(objs[2]);
            try
            {
                var receivedSize = socket.EndReceive(ar);
                taskSrc.TrySetResult(receivedSize);
            }
            catch (Exception ex)
            {
                taskSrc.TrySetException(ex);
            }
        }
    }
}
