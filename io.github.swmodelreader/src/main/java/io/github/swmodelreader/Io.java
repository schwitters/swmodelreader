package io.github.swmodelreader;

import static com.google.common.base.Preconditions.checkNotNull;

import java.io.Closeable;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.io.Reader;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardOpenOption;
import java.nio.file.attribute.FileTime;

import com.google.common.base.Charsets;
import com.google.common.base.Throwables;

public class Io {
	public static final int IO_BUFFER_SIZE = 4 * 1024;
	private static final String KEY = Io.class.getPackage().getName().toLowerCase();

	public static Reader newUtf8FileReader(Path path) throws IOException {
		return Files.newBufferedReader(path, Charsets.UTF_8);
	}

	public static Path newTempDir() throws IOException {
		return Files.createTempDirectory(KEY);
	}

	public static Path mkdir(Path parent, String name) {
		Path dir = parent.resolve(name);
		try {
			Files.createDirectories(dir);
		} catch (IOException e) {
			throw new RuntimeException("mkdir", e);
		}
		return dir;
	}

	public static void touch(Path file) throws IOException {
		checkNotNull(file);
		if (Files.exists(file)) {
			Files.setLastModifiedTime(file, FileTime.fromMillis(System.currentTimeMillis()));
		} else {
			Files.createFile(file);
		}
	}

	public static void deleteDirectory(Path directory) throws IOException {
		DirectoryDeleter.delete(directory);

	}

	public static String readUtf8File(Path log) throws IOException {
		return new String(Files.readAllBytes(log), StandardCharsets.UTF_8);
	}

	public static Reader newUtf16FileReader(Path path) throws IOException {
		return Files.newBufferedReader(path, StandardCharsets.UTF_16);

	}

	public static Writer newUtf8FileWriter(Path path) throws IOException {
		return Files.newBufferedWriter(path, Charsets.UTF_8, StandardOpenOption.CREATE);
	}

	public static PrintWriter newUtf8PrintWriter(Path path) throws IOException {
		return new PrintWriter(Files.newBufferedWriter(path, StandardCharsets.UTF_8));
	}

	public static void closeQuietly(Closeable closeable) {
		if (closeable != null) {
			try {
				closeable.close();
			} catch (IOException ignored) {
				// empty
			}
		}

	}

	public static long copy(InputStream in, OutputStream out) throws IOException {
		return copy(in, out, Long.MAX_VALUE);
	}

	public static long copy(InputStream in, OutputStream out, long length) throws IOException {
		try {
			long copied = 0;
			int len = (int) Math.min(length, IO_BUFFER_SIZE);
			byte[] buffer = new byte[len];
			while (length > 0) {
				len = in.read(buffer, 0, len);
				if (len < 0) {
					break;
				}
				if (out != null) {
					out.write(buffer, 0, len);
				}
				copied += len;
				length -= len;
				len = (int) Math.min(length, IO_BUFFER_SIZE);
			}
			return copied;
		} catch (Exception e) {
			Throwables.propagateIfInstanceOf(e, IOException.class);
		}
		return 0;
	}

}
