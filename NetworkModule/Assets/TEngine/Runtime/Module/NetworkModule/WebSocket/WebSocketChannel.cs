using System;
using System.Collections.Generic;
using UnityWebSocket;

namespace TEngine
{
    /// <summary>
    /// WebSocket网络通信频道，负责处理WebSocket协议的数据收发与编解码。
    /// </summary>
    public class WebSocketChannel : IDisposable
    {
        private readonly Queue<INetPackage> _sendQueue = new Queue<INetPackage>(2 ^ 14); 
        private readonly Queue<INetPackage> _receiveQueue = new Queue<INetPackage>(2 ^ 14);
        private readonly List<INetPackage> _decodeTempList = new List<INetPackage>(2 ^ 7); 

        // 网络数据缓冲区。
        private byte[] _receiveBuffer;
        private RingBuffer _encodeBuffer;  // 编码用环形缓冲区。
        private RingBuffer _decodeBuffer;  // 解码用环形缓冲区。
        private int _packageBodyMaxSize;   // 单个包体最大尺寸。
        private INetPackageEncoder _packageEncoder;  // 网络包编码器。
        private INetPackageDecoder _packageDecoder;  // 网络包解码器。

        /// <summary>
        /// WebSocket连接实例。
        /// </summary>
        private IWebSocket _socket;

        /// <summary>
        /// 初始化网络频道。
        /// </summary>
        /// <param name="socket">WebSocket连接实例。</param>
        /// <param name="packageBodyMaxSize">包体最大尺寸限制。</param>
        /// <param name="encoder">自定义包编码器。</param>
        /// <param name="decoder">自定义包解码器。</param>
        internal void InitChannel(IWebSocket socket, int packageBodyMaxSize, INetPackageEncoder encoder, INetPackageDecoder decoder)
        {
            // 参数校验与初始化。
            if (packageBodyMaxSize <= 0)
            {
                throw new ArgumentException($"PackageMaxSize is invalid : {packageBodyMaxSize}");
            }
            
            _socket = socket;

            // 初始化编解码器并注册错误回调。
            _packageBodyMaxSize = packageBodyMaxSize;
            _packageEncoder = encoder;
            _packageEncoder.RegisterHandleErrorCallback(HandleError);
            _packageDecoder = decoder;
            _packageDecoder.RegisterHandleErrorCallback(HandleError);

            // 初始化缓冲区（4倍最大包尺寸，保证处理效率）。
            int encoderPackageMaxSize = packageBodyMaxSize + _packageEncoder.GetPackageHeaderSize();
            int decoderPackageMaxSize = packageBodyMaxSize + _packageDecoder.GetPackageHeaderSize();
            _encodeBuffer = new RingBuffer(encoderPackageMaxSize * 4);
            _decodeBuffer = new RingBuffer(decoderPackageMaxSize * 4);
            _receiveBuffer = new byte[decoderPackageMaxSize];
        }

        /// <summary>
        /// 检查WebSocket连接状态是否为已打开。
        /// </summary>
        public bool IsConnected()
        {
            return _socket is { ReadyState: WebSocketState.Open };
        }

        /// <summary>
        /// 释放资源，关闭连接并清空队列。
        /// </summary>
        public void Dispose()
        {
            try
            {
                _socket?.CloseAsync();
                // 清空所有数据队列
                _sendQueue.Clear();
                _receiveQueue.Clear();
                _decodeTempList.Clear();
                // 重置缓冲区
                _encodeBuffer.Clear();
                _decodeBuffer.Clear();
            }
            catch (Exception)
            {
                // 忽略客户端已关闭时的异常
            }
            finally
            {
                if (_socket != null)
                {
                    _socket.CloseAsync();
                    _socket = null;
                }
            }
        }

        /// <summary>
        /// 主线程更新，处理数据发送。
        /// </summary>
        public void Update()
        {
            if (_socket == null || _socket.ReadyState == WebSocketState.Closed)
            {
                return;
            }
            UpdateSending();
        }

        /// <summary>
        /// 接收消息事件处理。
        /// </summary>
        /// <param name="e">包含原始字节数据的消息事件参数。</param>
        public void OnReceive(MessageEventArgs e)
        {
            // 写入解码缓冲区
            _decodeBuffer.WriteBytes(e.RawData, 0, e.RawData.Length);
            // 执行解码操作
            _decodeTempList.Clear();
            _packageDecoder.Decode(_packageBodyMaxSize, _decodeBuffer, _decodeTempList);
            // 加锁将解码后的包加入接收队列
            lock (_receiveQueue)
            {
                for (int i = 0; i < _decodeTempList.Count; i++)
                {
                    _receiveQueue.Enqueue(_decodeTempList[i]);
                }
            }
        }

        /// <summary>
        /// 处理发送队列中的数据。
        /// </summary>
        private void UpdateSending()
        {
            if (_sendQueue.Count > 0)
            {
                _encodeBuffer.Clear();  // 清空编码缓冲区
                // 批量编码待发送数据
                while (_sendQueue.Count > 0)
                {
                    int encoderPackageMaxSize = _packageBodyMaxSize + _packageEncoder.GetPackageHeaderSize();
                    if (_encodeBuffer.WriteableBytes < encoderPackageMaxSize)
                        break;

                    INetPackage package = _sendQueue.Dequeue();
                    _packageEncoder.Encode(_packageBodyMaxSize, _encodeBuffer, package);
                }
                // 发送编码后的字节流
                _socket.SendAsync(_encodeBuffer.GetBuffer(), 0, _encodeBuffer.ReadableBytes);
            }
        }

        /// <summary>
        /// 将网络包加入发送队列。
        /// </summary>
        public void SendPackage(INetPackage package)
        {
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(package);
            }
        }

        /// <summary>
        /// 从接收队列中取出一个网络包。
        /// </summary>
        public INetPackage PickPackage()
        {
            INetPackage package = null;
            lock (_receiveQueue)
            {
                if (_receiveQueue.Count > 0)
                {
                    package = _receiveQueue.Dequeue();
                }
            }
            return package;
        }

        /// <summary>
        /// 错误处理回调，记录日志并根据需要释放资源。
        /// </summary>
        private void HandleError(bool isDispose, string error)
        {
            Log.Error(error);
            if (isDispose)
            {
                Dispose();
            }
        }
    }
}
