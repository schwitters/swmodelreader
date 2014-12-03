using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zlib;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SwModelReaderCore
{
    public enum SwFileReaderResult
    {
        Ok,
        Fail
    }

    public class SwStorageChunkInfo
    {
        public byte[] Chunk { get; set; }
        public uint UncompressedSize { get; set; }
        public uint CompressedSize { get; set; }
        public uint ChunkOffset { get; set; }
        public int StartCompressedBlock { get; set; }
        public string ChunkName { get; set; }

    }
    public class SwStorage
    {
        private readonly List<SwStorageChunkInfo> m_chunks = new List<SwStorageChunkInfo>();

        public uint Header { get; set; }
        public uint Key { get; set; }


        public void AddChunk(SwStorageChunkInfo chunk)
        {
            this.m_chunks.Add(chunk);
        }
        
        public IReadOnlyList<SwStorageChunkInfo> GetChunks()
        {
            return m_chunks;
        }

    }
    public class SwModelReader : IDisposable
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        private readonly Stream m_stream;
        private SwStorage m_storage;
        private readonly static byte[] CHUNK_MAGIC = new byte[] { //
		(byte) 0x8d, //
				(byte) 0x61, //
				(byte) 0x84,//
				(byte) 0xa5, //
				(byte) 0x14, //
				(byte) 0x00, //
				(byte) 0x06, //
				(byte) 0x00 //
        };

        public SwModelReader(Stream stream)
        {
            this.m_stream = stream;
            m_storage = Decode(m_stream);

        }

        private SwStorage Decode(Stream stream)
        {
            SwStorage storage = new SwStorage();
            DecodeHeader(storage);
            DecodeChunks(storage);
            return storage;
        }

        private void DecodeChunks(SwStorage storage)
        {
            //
            byte[] blob = StreamHelper.ReadToEnd(m_stream);
            for (int blockstart = 0; blockstart < blob.Length - CHUNK_MAGIC.Length; blockstart++)
            {
                byte[] window = new byte[CHUNK_MAGIC.Length];
                Array.Copy(blob, blockstart, window, 0, CHUNK_MAGIC.Length);
                if (!ByteArrayCompare(CHUNK_MAGIC, window))
                {
                    continue;
                }
                storage.AddChunk(ReadChunk(storage, blob, blockstart));
            }
        }

        private static SwStorageChunkInfo ReadChunk(SwStorage storage, byte[] blob, int blockstart)
        {
            uint compressedSize = GetUInt(blob, blockstart + 0x12);

            uint uncompressedSize = GetUInt(blob, blockstart + 0x12 + 4);

            int nameSize = (int)GetUInt(blob, blockstart + 0x12 + 4 + 4);

            int namestart = blockstart + 0x12 + 4 + 4 + 4;

            byte[] name = blob.Skip(namestart).Take(nameSize).ToArray();
            byte[] unrolName = new byte[name.Length];
            for (int i = 0; i < name.Length; i++)
            {
                unrolName[i] = Rol(name[i], (int)storage.Key);
            }
            string chunkName = Encoding.UTF8.GetString(unrolName);
            if (string.IsNullOrWhiteSpace(chunkName))
            {
                chunkName = "un_" + Guid.NewGuid().ToString();
            }
            int compressedDataStart = namestart + nameSize;
            SwStorageChunkInfo chunk = new SwStorageChunkInfo();

            Debug.WriteLine("stream name {0} : start : {1:x4}, compressedSize:{2} 0x{2:x8}, uncompressedSize : {3} 0x{3:x8}",
                new object[] { chunkName, compressedDataStart + 8, compressedSize, uncompressedSize });

            SwStorageChunkInfo chunkInfo = new SwStorageChunkInfo();
            chunkInfo.ChunkOffset = (uint)blockstart;
            chunkInfo.CompressedSize = compressedSize;
            chunkInfo.StartCompressedBlock = compressedDataStart;
            chunkInfo.ChunkName = chunkName;
            if (uncompressedSize > 0)
            {
                byte[] compressedData = blob.Skip(compressedDataStart).Take((int)compressedSize).ToArray();
                byte[] uncompressedData = Inflate(compressedData, (int)uncompressedSize);
                chunkInfo.Chunk = uncompressedData;
            }
            else
            {
                chunkInfo.Chunk = new byte[0];
            }
            return chunkInfo;
        }
        static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }
        public static int GetInt(byte[] data, int offset)
        {
            int i = offset;
            int b0 = data[i++] & 0xFF;
            int b1 = data[i++] & 0xFF;
            int b2 = data[i++] & 0xFF;
            int b3 = data[i++] & 0xFF;
            return (b3 << 24) + (b2 << 16) + (b1 << 8) + (b0 << 0);
        }

        public static uint GetUInt(byte[] data, int offset)
        {
            int retNum = GetInt(data, offset);
            return (uint)(retNum & 0x00FFFFFFFFL);
        }

        public static byte[] Inflate(byte[] data, int outputSize)
        {
            byte[] output = new Byte[outputSize];
            using (MemoryStream ms = new MemoryStream())
            {
                ZlibCodec compressor = new ZlibCodec();
                compressor.InitializeInflate(false);

                compressor.InputBuffer = data;
                compressor.AvailableBytesIn = data.Length;
                compressor.NextIn = 0;
                compressor.OutputBuffer = output;

                foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish })
                {
                    int bytesToWrite = 0;
                    do
                    {
                        compressor.AvailableBytesOut = outputSize;
                        compressor.NextOut = 0;
                        compressor.Inflate(f);

                        bytesToWrite = outputSize - compressor.AvailableBytesOut;
                        if (bytesToWrite > 0)
                            ms.Write(output, 0, bytesToWrite);
                    }
                    while ((f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
                        (f == FlushType.Finish && bytesToWrite != 0));
                }
                compressor.EndInflate();
                return ms.ToArray();
            }
        }
        private void DecodeHeader(SwStorage storage)
        {
            var header = new byte[4];
            this.m_stream.Read(header, 0, 4);
            storage.Header = GetUInt(header,0);

            var key = new byte[4];
            this.m_stream.Read(key, 0, 4);
            storage.Key = key[3];
        }

        public SwFileReaderResult GetStream(string streamName,out byte[] streamData)
        {
            streamData = m_storage.GetChunks().Where(chunk => streamName.Equals(chunk.ChunkName)).Select(chunk => chunk.Chunk).First();
            return SwFileReaderResult.Ok;
        }

        public SwFileReaderResult GetAvailableStreamNames(out string[] names)
        {

            names = m_storage.GetChunks().Select(chunk => chunk.ChunkName).ToArray();
            return SwFileReaderResult.Ok;
        }

        public static byte Rol(byte bits, int shift)
        {
            // Unlike the RotateLeft( uint, int ) and RotateLeft( ulong, int ) 
            // overloads, we need to mask out the required bits of count 
            // manually, as the shift operaters will promote a byte to uint, 
            // and will not mask out the correct number of count bits.
            shift &= 0x07;
            return (byte)((bits << shift) | (bits >> (8 - shift)));
        }

        public class StreamHelper
        {
            public static byte[] ReadToEnd(System.IO.Stream stream)
            {

                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;

            }


        }


        public void Dispose()
        {
            this.m_stream.Dispose();
        }
    }
}
