using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;

namespace TEngine
{
    public static class ProtoBufHelper
    {
        public static object FromSpan(Type type, Span<byte> span)
        {
            using var recyclableMemoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
            recyclableMemoryStream.Write(span);
            recyclableMemoryStream.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize(type, recyclableMemoryStream);
        }

        public static object FromMemory(Type type, Memory<byte> memory)
        {
            using var recyclableMemoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
            recyclableMemoryStream.Write(memory.Span);
            recyclableMemoryStream.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize(type, recyclableMemoryStream);
        }

        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            using var stream = MemoryStreamHelper.GetRecyclableMemoryStream();
            stream.Write(bytes, index, count);
            stream.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize(type, stream);
        }

        public static T FromBytes<T>(byte[] bytes)
        {
            using var stream = MemoryStreamHelper.GetRecyclableMemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize<T>(stream);
        }

        public static T FromBytes<T>(byte[] bytes, int index, int count)
        {
            using var stream = MemoryStreamHelper.GetRecyclableMemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize<T>(stream);
        }

        public static byte[] ToBytes(object message)
        {
            using var stream = MemoryStreamHelper.GetRecyclableMemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }

        public static void ToMemory(object message, Memory<byte> memory)
        {
            using var stream = MemoryStreamHelper.GetRecyclableMemoryStream();
            Serializer.Serialize(stream, message);
            stream.GetBuffer().AsMemory().CopyTo(memory);
        }

        public static void ToStream(object message, MemoryStream stream)
        {
            Serializer.Serialize(stream, message);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            return Serializer.Deserialize(type, stream);
        }

        public static T FromStream<T>(MemoryStream stream)
        {
            return (T)Serializer.Deserialize(typeof(T), stream);
        }

        public static T Clone<T>(T t)
        {
            using var stream = MemoryStreamHelper.GetRecyclableMemoryStream();
            Serializer.Serialize(stream, t);
            return Serializer.Deserialize<T>(stream);
        }
    }
}