package io.github.swmodelreader;

public class SwStorageChunkInfo {
	private byte[] chunk;
	private long uncompressedSize;
	private long compressedSize;
	private long chunkOffset;
	private long headerLength;
	private long startCompressedBlock;
	private String chunkName;

	public SwStorageChunkInfo(String chunkName, byte[] chunk,
			long uncompressedSize, long compressedSize, long chunkOffset,
			long startCompressedBlock, long headerLength) {
		super();
		this.chunkName = chunkName;
		this.chunk = chunk;
		this.uncompressedSize = uncompressedSize;
		this.compressedSize = compressedSize;
		this.chunkOffset = chunkOffset;
		this.startCompressedBlock = startCompressedBlock;
		this.headerLength = headerLength;
	}

	public byte[] getChunk() {
		return chunk;
	}

	public long getUncompressedSize() {
		return uncompressedSize;
	}

	public long getCompressedSize() {
		return compressedSize;
	}

	public long getChunkOffset() {
		return chunkOffset;
	}

	public long getStartCompressedBlock() {
		return startCompressedBlock;
	}

	public String getChunkName() {
		return chunkName;
	}

	public long getLength() {
		return headerLength + compressedSize;
	}

}