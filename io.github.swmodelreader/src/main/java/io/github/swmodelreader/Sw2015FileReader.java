package io.github.swmodelreader;

import java.io.BufferedInputStream;
import java.io.Closeable;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.List;
import java.util.UUID;
import java.util.zip.DataFormatException;
import java.util.zip.Inflater;

import org.apache.commons.codec.binary.Hex;

import com.google.common.base.Function;
import com.google.common.base.Joiner;
import com.google.common.base.Optional;
import com.google.common.base.Predicate;
import com.google.common.base.Splitter;
import com.google.common.base.Strings;
import com.google.common.collect.Iterables;
import com.google.common.collect.Lists;
import com.google.common.io.ByteStreams;

public class Sw2015FileReader implements Closeable {

	private final InputStream in;
	private SwStorage storage = null;

	public Sw2015FileReader(InputStream in) {
		super();
		this.in = in instanceof BufferedInputStream ? in : new BufferedInputStream(in);

	}

	SwStorage process(InputStream stream) throws IOException {
		// TODO: lazy loading
		byte[] blob = ByteStreams.toByteArray(stream);
		int index = 0;

		// yet unknown the first bytes
		long header = getInt(blob, index);
		index += 4;

		index += 3; // skip 3 bytes
		// there seems to be an key for scrambling strings
		long key = blob[index];
		index += 1;
		SwStorage storage = new SwStorage(header, key);
		for (; index < blob.length; index++) {
			// TODO: find a way to recognize if the table of contents starts
			SwStorageChunkInfo chunk = readChunk(storage, blob, index);
			if (chunk == null) {
				readContentTable(blob, index);
				break;
			}
			storage.addChunk(chunk);
			int nextOffset = index + (int) chunk.getLength();
			index = nextOffset - 1; // for !
		}
		return storage;
	}

	private void readContentTable(byte[] data, int index) {
		// yet no ide about the bytes
	}

	String dump(byte[] data, int index, int length, String comment) {
		return String.format("%08x: %s (%s)", index, toHex(data, index, length), comment);
	}

	SwStorageChunkInfo readChunk(SwStorage storage, byte[] blob, int startIndex) {
		// within a block before offset 0x12 the bytes are yet unknown
		int index = startIndex + 0x12;

		long compressedSize = getUInt(blob, index);
		index += 4;

		long uncompressedSize = getUInt(blob, index);
		index += 4;

		int nameSize = (int) getUInt(blob, index);
		index += 4;
		int namestart = index;
		if (namestart + nameSize > blob.length) {
			// happens if we try to read the content table
			return null;
		}
		// the stream names are scrambled ;-)
		byte[] unrolName = new byte[nameSize];
		for (; index < namestart + nameSize; index++) {
			byte unroledByte = rol(blob[index], (int) storage.getKey());
			unrolName[index - namestart] = unroledByte;
		}
		String chunkName = new String(unrolName, StandardCharsets.UTF_8);
		if (Strings.isNullOrEmpty(chunkName)) {
			chunkName = "un_" + UUID.randomUUID().toString();
		}

		int compressedDataStart = namestart + nameSize;
		byte[] uncompressedChunk = new byte[(int) uncompressedSize];
		if (uncompressedSize > 0) {
			Inflater inflater = new Inflater(true);
			inflater.setInput(blob, compressedDataStart, (int) compressedSize);
			try {
				inflater.inflate(uncompressedChunk);

			} catch (DataFormatException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
		// System.out.println(dump(blob, startIndex, 0x12,
		// String.format("Block header:%s", chunkName)));
		SwStorageChunkInfo chunkInfo = new SwStorageChunkInfo(chunkName, uncompressedChunk, uncompressedSize, compressedSize,
				startIndex, compressedDataStart, compressedDataStart - startIndex);
		return chunkInfo;
	}

	private String toHex(byte[] blob, int offset, int length) {
		String asHey = Hex.encodeHexString(Arrays.copyOfRange(blob, offset, offset + length));
		return Joiner.on(' ').join(Splitter.fixedLength(2).split(asHey));
	}

	public static byte rol(byte bits, int shift) {
		return (byte) (((bits & 0xff) << shift) | ((bits & 0xff) >>> (8 - shift)));
	}

	public static int getInt(byte[] data, int offset) {
		int i = offset;
		int b0 = data[i++] & 0xFF;
		int b1 = data[i++] & 0xFF;
		int b2 = data[i++] & 0xFF;
		int b3 = data[i++] & 0xFF;
		return (b3 << 24) + (b2 << 16) + (b1 << 8) + (b0 << 0);
	}

	public static long getUInt(byte[] data, int offset) {
		long retNum = getInt(data, offset);
		return retNum & 0x00FFFFFFFFl;
	}

	boolean startsWith(byte[] array, byte[] prefix, long offset) {
		boolean startsWith = true;
		if (prefix.length > array.length) {
			return false;
		}
		int len = Math.min(array.length, prefix.length);

		for (long i = offset; i < offset + len; i++) {
			if (prefix[(int) (i - offset)] != array[(int) i]) {
				startsWith = false;
			}
		}
		return startsWith;
	}

	@Override
	public void close() throws IOException {
		in.close();
	}

	public List<String> getAvailableStreamNames() throws IOException {
		ensureProcessed();
		return Lists.transform(storage.getChunks(), new Function<SwStorageChunkInfo, String>() {

			@Override
			public String apply(SwStorageChunkInfo input) {
				return input.getChunkName();
			}
		});
	}

	private void ensureProcessed() throws IOException {
		if (this.storage == null) {
			storage = process(in);
		}
	}

	public byte[] getStream(final String name) throws IOException {
		ensureProcessed();
		Optional<SwStorageChunkInfo> result = Iterables.tryFind(this.storage.getChunks(), new Predicate<SwStorageChunkInfo>() {
			@Override
			public boolean apply(SwStorageChunkInfo input) {

				return input.getChunkName().equalsIgnoreCase(name);
			}
		});
		return result.isPresent() ? result.get().getChunk() : null;
	}
}
