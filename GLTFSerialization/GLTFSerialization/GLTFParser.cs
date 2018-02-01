using GLTF.Schema;
using System;
using System.Collections.Generic;
using System.IO;

namespace GLTF
{
	public enum ChunkFormat : uint
	{
		JSON = 0x4e4f534a,
		BIN = 0x004e4942
	}
	
	public struct GLBHeader
	{
		public uint Version { get; set; }
		public uint FileLength { get; set; }
	}

	public struct ChunkInfo
	{
		public long StartPosition;
		public uint Length;
		public ChunkFormat Type;
	}
	
	public class GLTFParser
	{
		public static readonly uint HEADER_SIZE = 12;
		public static readonly uint CHUNK_HEADER_SIZE = 8;
		public static readonly uint MAGIC_NUMBER = 0x46546c67;

		public static GLTFRoot ParseJson(Stream stream, long startPosition = 0)
		{
			stream.Position = startPosition;
			bool isGLB = IsGLB(stream);
			
			// Check for binary format magic bytes
			if (isGLB)
			{
				ParseJsonChunk(stream, startPosition);
			}
			else
			{
				stream.Position = startPosition;
			}

			GLTFRoot root = GLTFRoot.Deserialize(new StreamReader(stream));
			root.IsGLB = isGLB;

			return root;
		}

		// todo: this needs reimplemented. There is no such thing as a binary chunk index, and the chunk may not be in 0, 1, 2 order
		// Moves stream position to binary chunk location
		public static ChunkInfo SeekToBinaryChunk(Stream stream, int binaryChunkIndex, long startPosition = 0)
		{
			stream.Position = startPosition + 4;	 // start after magic number chunk
			GLBHeader header = ParseGLBHeader(stream);
			uint chunkOffset = 12;   // sizeof(GLBHeader) + magic number
			uint chunkLength = 0;
			for (int i = 0; i < binaryChunkIndex + 2; ++i)
			{
				chunkOffset += chunkLength;
				stream.Position = chunkOffset;
				chunkLength = GetUInt32(stream);
				chunkOffset += 8;   // to account for chunk length (4 bytes) and type (4 bytes)
			}

			// Load Binary Chunk
			if (chunkOffset + chunkLength <= header.FileLength)
			{
				ChunkFormat chunkType = (ChunkFormat)GetUInt32(stream);
				if (chunkType != ChunkFormat.BIN)
				{
					throw new GLTFHeaderInvalidException("Second chunk must be of type BIN if present");
				}

				return new ChunkInfo
				{
					StartPosition = stream.Position - CHUNK_HEADER_SIZE,
					Length = chunkLength,
					Type = chunkType
				};
			}

			throw new GLTFHeaderInvalidException("File length does not match chunk header.");
		}

		public static GLBHeader ParseGLBHeader(Stream stream)
		{
			uint version = GetUInt32(stream);   // 4
			uint length = GetUInt32(stream); // 8

			return new GLBHeader
			{
				Version = version,
				FileLength = length
			};
		}

		public static bool IsGLB(Stream stream)
		{
			return GetUInt32(stream) == 0x46546c67;  // 0
		}

		public static ChunkInfo ParseChunkInfo(Stream stream)
		{
			ChunkInfo chunkInfo = new ChunkInfo
			{
				StartPosition = stream.Position
			};

			chunkInfo.Length = GetUInt32(stream);					// 12
			chunkInfo.Type = (ChunkFormat)GetUInt32(stream);		// 16
			return chunkInfo;
		}

		public static List<ChunkInfo> FindChunks(Stream stream, long startPosition = 0)
		{
			stream.Position = startPosition + 4;     // start after magic number chunk
			ParseGLBHeader(stream);
			List<ChunkInfo> allChunks = new List<ChunkInfo>();
			while (stream.Position != stream.Length)
			{
				ChunkInfo chunkInfo = ParseChunkInfo(stream);
				allChunks.Add(chunkInfo);
				stream.Position += chunkInfo.Length;
			}

			return allChunks;
		}

		private static void ParseJsonChunk(Stream stream, long startPosition)
		{
			GLBHeader header = ParseGLBHeader(stream);  // 4, 8
			if (header.Version != 2)
			{
				throw new GLTFHeaderInvalidException("Unsupported glTF version");
			};

			if (header.FileLength != (stream.Length - startPosition))
			{
				throw new GLTFHeaderInvalidException("File length does not match header.");
			}

			ChunkInfo chunkInfo = ParseChunkInfo(stream);
			if (chunkInfo.Type != ChunkFormat.JSON)
			{
				throw new GLTFHeaderInvalidException("First chunk must be of type JSON");
			}
		}

		private static uint GetUInt32(Stream stream)
		{
			var uintSize = sizeof(uint);
			byte[] headerBuffer = new byte[uintSize];
			stream.Read(headerBuffer, 0, uintSize);
			return BitConverter.ToUInt32(headerBuffer, 0);
		}
	}
}

