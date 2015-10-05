Warning : The component is broken. Feel free to send a fix

swmodelreader
=============

Prototype for Reading SolidWorks 2015 Files. Since sw2015 you can not use 
Apache POI (Java) or OpenMCDF (.NET) for reading Solidworks files.

This Project attempts to read data from SolidWorks 2015 Files.

There are two implementations available :  .NET and Java.

Current Status :

Java implementation allows you to extract streams from SLDXXX files.

```Java
Path swModelFile = ...;
try (Sw2015FileReader reader = new Sw2015FileReader(Files.newInputStream(swModelFile))) {
	byte[] data = reader.getStream("PreviewPNG");
	/* do something with png data */
}
```
The same in C#
```C#
 string swModelFile = ...;
 using (var reader = SwModelReader.Open(swModelFile))
 {
    byte[] pngData;
    reader.GetStream("PreviewPNG", out pngData);
    // do something with your png data 
 }
```

