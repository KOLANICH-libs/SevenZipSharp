using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using SevenZip;

namespace SevenZipTest2013
{
	/// <summary>
	/// Сводное описание для SevenZipTestPack
	/// </summary>
	[TestClass]
	public class SevenZipTestPack : SevenZipTest
	{
		[ClassInitialize]
		public static void startUp(TestContext ctx){
			initTempDir(ctx);
			testFold1 = createTempFolder();
			testFile1 = createTempFile(testFold1);

			testFold2 = createTempFolder();
			testFile2 = createTempFile(testFold2);
			archivePath = createTempFileName() + ".archive";
		}

		[ClassCleanup]
		public static void tearDown(){
			removeTempDir();
		}

		public static string testFold1, testFold2, testFile1, testFile2;

		[TestMethod]
		public void SerializationDemo(){
			ArgumentException ex = new ArgumentException("blahblah");
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
				new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			using (MemoryStream ms = new MemoryStream()) {
				bf.Serialize(ms, ex);
				SevenZipCompressor cmpr = new SevenZipCompressor();
				cmpr.CompressStream(ms, File.Create(createTempFileName()));
			}
		}

		/*[TestMethod]
		public void LzmaEncodeAndDecodeStreamTest() {
			using (var output = new FileStream(@"d:\Temp\arch.lzma", FileMode.Create))
			{
				var encoder = new LzmaEncodeStream(output);
				using (var inputSample = new FileStream(@"d:\Temp\tolstoi_lev_voina_i_mir_kniga_1.rtf", FileMode.Open))
				{
					int bufSize = 24576, count;
					byte[] buf = new byte[bufSize];
					while ((count = inputSample.Read(buf, 0, bufSize)) > 0)
					{
						encoder.Write(buf, 0, count);
					}
				}
				encoder.Close();
			}
			using (var input = new FileStream(@"d:\Temp\arch.lzma", FileMode.Open))
			{
				var decoder = new LzmaDecodeStream(input);
				using (var output = new FileStream(@"d:\Temp\res.rtf", FileMode.Create))
				{
					int bufSize = 24576, count;
					byte[] buf = new byte[bufSize];
					while ((count = decoder.Read(buf, 0, bufSize)) > 0)
					{
						output.Write(buf, 0, count);
					}
				}
			}
		}*/

		[TestMethod]
		public void CompressionTestsVerySimple(){
			var tmp = new SevenZipCompressor();
			//tmp.ScanOnlyWritable = true;
			//tmp.CompressFiles(@"d:\Temp\arch.7z", @"d:\Temp\log.txt");
			//tmp.CompressDirectory(@"c:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE\1033", @"D:\Temp\arch.7z");
			tmp.CompressDirectory(testFold1, Path.Combine(tempFolder, TestContext.TestName + ".7z"));
		}

		[TestMethod]
		public void CompressionTestFeaturesAppendMode(){
			var tmp = new SevenZipCompressor();
			var arch = Path.Combine(tempFolder, TestContext.TestName + ".7z");
			tmp.CompressDirectory(testFold2, arch);
			tmp.CompressionMode = CompressionMode.Append;
			tmp.CompressDirectory(testFold1, arch);
			tmp = null;
		}

		[TestMethod]
		public void CompressionTestFeaturesModifyMode(){
			var arch = Path.Combine(tempFolder, TestContext.TestName + ".7z");
			var tmp = new SevenZipCompressor();
			tmp.CompressDirectory(testFold2, arch);
			tmp.ModifyArchive(arch, new Dictionary<int, string>(){{0, testFile1}});
			//Delete
			//tmp.ModifyArchive(@"d:\Temp\test.7z", new Dictionary<int, string>() { { 19, null }, { 1, null } });
		}

		[TestMethod]
		public void CompressionTestMultiVolumes(){
			var tmp = new SevenZipCompressor();
			tmp.VolumeSize = 10000;
			tmp.CompressDirectory(testFold1, Path.Combine(tempFolder, TestContext.TestName + ".7z"));
		}

		[TestMethod]
		public void CompressionTestLotsOfFeatures(){
			var tmp = new SevenZipCompressor();
			tmp.ArchiveFormat = OutArchiveFormat.SevenZip;
			tmp.CompressionLevel = CompressionLevel.High;
			tmp.CompressionMethod = CompressionMethod.Ppmd;
			tmp.FileCompressionStarted += (s, e) =>{
				if (e.PercentDone > 50) e.Cancel = true;
				else {
					//TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileName));
				}
			};

			tmp.FilesFound += (se, ea) => { TestContext.WriteLine("Number of files: " + ea.Value.ToString()); };

			var arch = Path.Combine(tempFolder, TestContext.TestName + ".ppmd.7z");
			tmp.CompressFiles(arch, testFile1, testFile2);
			tmp.CompressDirectory(testFold1, arch);
		}

		[TestMethod]
		public void MultiThreadedCompressionTest(){
			var t1 = new Thread(() =>{
				var tmp = new SevenZipCompressor();
				tmp.FileCompressionStarted +=
					(s, e) => TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileName));
				tmp.CompressDirectory(testFold1, Path.Combine(tempFolder, TestContext.TestName + "1.7z"));
			});
			var t2 = new Thread(() =>{
				var tmp = new SevenZipCompressor();
				tmp.FileCompressionStarted +=
					(s, e) => TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileName));
				tmp.CompressDirectory(testFold2, Path.Combine(tempFolder, TestContext.TestName + "2.7z"));
			});
			t1.Start();
			t2.Start();
			t1.Join();
			t2.Join();
		}

		[TestMethod]
		public async Task MultiTaskedCompressionTest(){
			var t1 = Task.Factory.StartNew(() =>{
				var tmp = new SevenZipCompressor();
				tmp.FileCompressionStarted +=
					(s, e) => TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileName));
				tmp.CompressDirectory(testFold1, Path.Combine(tempFolder, TestContext.TestName + "1.7z"));
			});
			var t2 = Task.Factory.StartNew(() =>{
				var tmp = new SevenZipCompressor();
				tmp.FileCompressionStarted +=
					(s, e) => TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileName));
				tmp.CompressDirectory(testFold2, Path.Combine(tempFolder, TestContext.TestName + "2.7z"));
			});
			await Task.WhenAll(t1, t2);
		}

		[TestMethod]
		public void StreamingCompressionTest(){
			var tmp = new SevenZipCompressor();
			tmp.FileCompressionStarted += (s, e) =>{
				//TestContext.WriteLine(String.Format("[{0}%] {1}",e.PercentDone, e.FileName));
			};
			tmp.CompressDirectory(testFold1,
				File.Create(Path.Combine(tempFolder, TestContext.TestName + ".7z")));
		}

		[TestMethod]
		public void CompressStreamManagedTest(){
			SevenZipCompressor.CompressStream(File.OpenRead(testFile1),
				File.Create(Path.Combine(tempFolder, TestContext.TestName + ".7z")), null, (o, e) =>{
					if (e.PercentDelta > 0) {
						//TestContext.Clear();
						//TestContext.WriteLine(e.PercentDone.ToString() + "%");
					}
				});
		}

		[TestMethod]
		public void CompressFilesZipTest(){
			var tmp = new SevenZipCompressor();
			tmp.ArchiveFormat = OutArchiveFormat.Zip;
			tmp.CompressFiles(Path.Combine(tempFolder, TestContext.TestName + ".zip"), testFile1, testFile2);
		}

		[TestMethod]
		public void CompressStreamExternalTest(){
			var tmp = new SevenZipCompressor();
			tmp.CompressStream(
				File.OpenRead(testFile1),
				File.Create(Path.Combine(tempFolder, TestContext.TestName + ".7z"))
				);
		}

		[TestMethod]
		public void CompressFileDictionaryTest(){
			var tmp = new SevenZipCompressor();
			Dictionary<string, string> fileDict = new Dictionary<string, string>();
			var arch = Path.Combine(tempFolder, TestContext.TestName + ".7z");
			fileDict.Add("ololol.bin", testFile1);
			tmp.FileCompressionStarted += (o, e) =>{
				//TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileName));
			};
			tmp.CompressFileDictionary(fileDict, arch);
		}

		[TestMethod]
		public void CompressWithCustomParametersDemo(){
			var tmp = new SevenZipCompressor();
			tmp.ArchiveFormat = OutArchiveFormat.Zip;
			tmp.CompressionMethod = CompressionMethod.Deflate;
			tmp.CompressionLevel = CompressionLevel.Ultra;
			//Number of fast bytes
			tmp.CustomParameters.Add("fb", "256");
			//Number of deflate passes
			tmp.CustomParameters.Add("pass", "4");
			//Multi-threading on
			tmp.CustomParameters.Add("mt", "on");
			tmp.ZipEncryptionMethod = ZipEncryptionMethod.Aes256;
			/*tmp.Compressing += (s, e) => {
				TestContext.Clear();
				TestContext.WriteLine(String.Format("{0}%", e.PercentDone));
			};*/
			tmp.CompressDirectory(testFold1, Path.Combine(tempFolder, TestContext.TestName + ".7z"), "test");


			/*SevenZipCompressor tmp = new SevenZipCompressor();
			tmp.CompressionMethod = CompressionMethod.Ppmd;
			tmp.CompressionLevel = CompressionLevel.Ultra;
			tmp.EncryptHeadersSevenZip = true;
			tmp.ScanOnlyWritable = true;
			tmp.CompressDirectory(@"d:\Temp\!Пусто", @"d:\Temp\arch.7z", "test");  
			//*/
		}

		/*[TestMethod]
		public void SfxDemo() {
			var sfx = new SevenZipSfx();
			SevenZipCompressor tmp = new SevenZipCompressor();
			using (MemoryStream ms = new MemoryStream())
			{
				tmp.CompressDirectory(testFold1, ms);
				sfx.MakeSfx(ms, createTempFileName()+".exe");
			}
		}*/
	}
}