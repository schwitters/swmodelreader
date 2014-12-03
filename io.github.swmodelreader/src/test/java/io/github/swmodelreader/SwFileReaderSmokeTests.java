package io.github.swmodelreader;

import static java.nio.file.Files.createTempDirectory;
import static java.nio.file.Files.write;

import java.awt.image.BufferedImage;
import java.nio.file.Path;
import java.util.List;

import javax.imageio.ImageIO;

import org.junit.Assert;
import org.junit.Test;

import com.google.common.io.ByteSource;
import com.google.common.io.Resources;

public class SwFileReaderSmokeTests {

	private static final String ASM_2 = "carrier.slddrw";
	private static final String ASM_1 = "_incontrolvalve.sldasm";
	private static final String PART_2 = "actuator_casing.sldprt";
	private static final String PART_1 = "_in_ec_valve.sldprt";

	@Test
	public void partListStreamsTest() throws Exception {
		ByteSource source = Resources.asByteSource(Resources.getResource(PART_1));
		System.out.println(PART_1);
		try (Sw2015FileReader reader = new Sw2015FileReader(source.openBufferedStream())) {
			List<String> streamNames = reader.getAvailableStreamNames();
			for (String name : streamNames) {
				byte[] data = reader.getStream(name);
				Assert.assertNotNull(data);
			}
		}
	}

	@Test
	public void part2ListStreamsTest() throws Exception {
		ByteSource source = Resources.asByteSource(Resources.getResource(PART_2));
		System.out.println(PART_2);
		try (Sw2015FileReader reader = new Sw2015FileReader(source.openBufferedStream())) {
			List<String> streamNames = reader.getAvailableStreamNames();
			for (String name : streamNames) {
				byte[] data = reader.getStream(name);
				Assert.assertNotNull(data);
			}
		}
	}

	@Test
	public void assemblyListStreamsTest() throws Exception {
		ByteSource source = Resources.asByteSource(Resources.getResource(ASM_1));
		try (Sw2015FileReader reader = new Sw2015FileReader(source.openBufferedStream())) {
			List<String> streamNames = reader.getAvailableStreamNames();
			for (String name : streamNames) {
				byte[] data = reader.getStream(name);
				Assert.assertNotNull(data);
			}
		}
	}

	@Test
	public void drawingListStreamsTest() throws Exception {
		ByteSource source = Resources.asByteSource(Resources.getResource(ASM_2));
		try (Sw2015FileReader reader = new Sw2015FileReader(source.openBufferedStream())) {
			List<String> streamNames = reader.getAvailableStreamNames();
			for (String name : streamNames) {
				byte[] data = reader.getStream(name);
				Assert.assertNotNull(data);
			}
		}
	}

	@Test
	public void testExtractPreview() throws Exception {
		ByteSource source = Resources.asByteSource(Resources.getResource(PART_1));
		Path tmp = createTempDirectory("swmodelreader");
		try (Sw2015FileReader reader = new Sw2015FileReader(source.openBufferedStream())) {
			byte[] data = reader.getStream("PreviewPNG");
			Path png = tmp.resolve("preview.png");
			write(png, data);
			BufferedImage img = ImageIO.read(png.toFile());
			Assert.assertNotNull(img);
		}
		Assert.assertTrue(DirectoryDeleter.tryDelete(tmp));
	}
}
