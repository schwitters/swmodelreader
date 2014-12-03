package io.github.swmodelreader;

import java.util.Collections;
import java.util.List;

import com.google.common.collect.Lists;

public class SwStorage {
	private final List<SwStorageChunkInfo> chunks = Lists.newArrayList();

	private final long header;
	private final long key;

	public SwStorage(long header, long key) {
		super();
		this.header = header;
		this.key = key;
	}

	public void addChunk(SwStorageChunkInfo chunk) {
		this.chunks.add(chunk);
	}

	public List<SwStorageChunkInfo> getChunks() {
		return Collections.unmodifiableList(this.chunks);
	}

	public long getHeader() {
		return header;
	}

	public long getKey() {
		return key;
	}

}