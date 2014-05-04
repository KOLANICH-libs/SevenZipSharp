using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Reflection;
using SevenZip;

namespace SevenZipTest2013
{
	/// <summary>
	/// Tests unpacking
	/// </summary>
	[TestClass]
	public class SevenZipTestUnpack : SevenZipTest
	{
		[ClassInitialize]
		public static void startUp(TestContext ctx){
			initTempDir(ctx);
			archivePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(tempFolder))), @"arch");
		}

		[ClassCleanup]
		public static void tearDown(){
			removeTempDir();
		}

		[TestMethod]
		public void TemporaryTest(){
			var features = SevenZipExtractor.CurrentLibraryFeatures;
			TestContext.WriteLine(((uint) features).ToString("X6"));
		}

		public void ExtractionTestExtractFiles(string format){
			using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test."+format)))
				for (int i = 0; i < tmp.ArchiveFileData.Count; i++) tmp.ExtractFiles(tempFolder, tmp.ArchiveFileData[i].Index);
		}

		[TestMethod]
		public void ExtractionTestBZIP27ZExtractFiles(){
			ExtractionTestExtractFiles("bzip2.7z");
		}

		[TestMethod]
		public void ExtractionTestLZMA7ZExtractFiles(){
			ExtractionTestExtractFiles("lzma.7z");
		}
		
		[TestMethod]
		public void ExtractionTestLZMA27ZExtractFiles(){
			ExtractionTestExtractFiles("lzma2.7z");
		}
		
		[TestMethod]
		public void ExtractionTestLZMA27ZSFXExtractFiles(){
			ExtractionTestExtractFiles("7Zip.LZMA2.sfx.exe");
		}

		[TestMethod]
		public void ExtractionTestPPMD7ZExtractFiles(){
			ExtractionTestExtractFiles("ppmd.7z");
		}

		[TestMethod]
		public void ExtractionTestRARExtractFiles(){
			ExtractionTestExtractFiles("rar");
		}

		[TestMethod]
		public void ExtractionTestTGZExtractFiles(){
			ExtractionTestExtractFiles("txt.gz");
		}
		
		[TestMethod]
		public void ExtractionTestTBZ2ExtractFiles(){
			ExtractionTestExtractFiles("txt.bz2");
		}

		[TestMethod]
		public void ExtractionTestTXZExtractFiles(){
			ExtractionTestExtractFiles("txt.xz");
		}
		
		[TestMethod]
		public void ExtractionTestTARExtractFiles(){
			ExtractionTestExtractFiles("tar");
		}

		[TestMethod]
		public void ExtractionTestMultiVolumes(){
			using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.part1.rar"))) tmp.ExtractArchive(tempFolder);
		}

		[TestMethod]
		public void ExtractionTestCancelFeature(){
			using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
				tmp.FileExtractionStarted += (s, e) =>{
					if (e.FileInfo.Index == 10) {
						e.Cancel = true;
						TestContext.WriteLine("Cancelled");
					}
					else
						TestContext.WriteLine(String.Format("[{0}%] {1}",
							e.PercentDone, e.FileInfo.FileName));
				};
				tmp.FileExists += (o, e) =>
					TestContext.WriteLine("Warning: file \"" + e.FileName + "\" already exists.");
				tmp.ExtractionFinished += (s, e) =>
					TestContext.WriteLine("Finished!");
				tmp.ExtractArchive(tempFolder);
			}
		}

		[TestMethod]
		public void MultiThreadedExtractionTest(){
			var t1 = new Thread(() =>{
				using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
					tmp.FileExtractionStarted += (s, e) =>
						TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
					tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
					tmp.ExtractArchive(Path.Combine(tempFolder, "MultiThreadedExtractionTest1"));
				}
			});
			var t2 = new Thread(() =>{
				using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
					tmp.FileExtractionStarted += (s, e) =>
						TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
					tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
					tmp.ExtractArchive(Path.Combine(tempFolder, "MultiThreadedExtractionTest2"));
				}
			});
			t1.Start();
			t2.Start();
			t1.Join();
			t2.Join();
		}

		[TestMethod]
		public async Task TaskedExtractionTest(){
			throw new NotImplementedException("Read code comments carefully, please");
			await Task.Factory.StartNew(() =>{
				using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
					/*
						bug: here is heisenbug, throws exception, nobody knows why
						looks like heap corruption
						if you stil sure that the bug is in WriteLine, try to comment it out and you will se bug in another place...
						also MultiThreadedExtractionTest works fine
						YOU NEED RATHER BIG FILE TO REPRODUCE THIS BUG, SMALL FILES AS Test.lzma.7z don't cause the crash
					*/
					tmp.FileExtractionStarted += (s, e) =>
						TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
					tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
					tmp.ExtractArchive(Path.Combine(tempFolder, "MultiThreadedExtractionTest1"));
				}
			});
		}

		[TestMethod]
		public async Task MultiTaskedExtractionTest(){
			throw new NotImplementedException("See TaskedExtractionTest, please");
			var tasks = new[]{
				Task.Factory.StartNew(() =>{
					using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
						tmp.FileExtractionStarted += (s, e) =>
							TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
						tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
						tmp.ExtractArchive(Path.Combine(tempFolder, "MultiThreadedExtractionTest1"));
					}
				}),
				Task.Factory.StartNew(() =>{
					using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
						tmp.FileExtractionStarted += (s, e) =>
							TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
						tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
						tmp.ExtractArchive(Path.Combine(tempFolder, "MultiThreadedExtractionTest2"));
					}
				})
			};
			await Task.WhenAll(tasks);
		}

		[TestMethod]
		public void StreamingExtractionTest(){
			using (
				var tmp = new SevenZipExtractor(
					File.OpenRead(Path.Combine(archivePath, "Test.lzma.7z"))
					)
				) {
				tmp.FileExtractionStarted += (s, e) =>
					TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
				tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
				tmp.ExtractArchive(tempFolder);
			}
		}

		[TestMethod]
		public void StreamingSFXExtractionTest() {
			using (
				var tmp = new SevenZipExtractor(
					File.OpenRead(Path.Combine(archivePath, "Test.7Zip.LZMA2.sfx.exe"))
					)
				) {
				tmp.FileExtractionStarted += (s, e) =>
					TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
				tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
				tmp.ExtractArchive(tempFolder);
			}
		}

		[TestMethod]
		public void ExtractFileStreamTest(){
			using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
				tmp.FileExtractionStarted += (s, e) => TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
				tmp.FileExists += (o, e) =>{
					TestContext.WriteLine("Warning: file \"" + e.FileName + "\" already exists.");
					//e.Overwrite = false;
				};
				tmp.ExtractionFinished += (s, e) => TestContext.WriteLine("Finished!");
				var rand = new Random();
				tmp.ExtractFile(rand.Next((int) tmp.FilesCount), File.Create(createTempFileName()));
			}
		}

		[TestMethod]
		public void ExtractFileDiskTest(){
			using (var tmp = new SevenZipExtractor(Path.Combine(archivePath, "Test.lzma.7z"))) {
				tmp.FileExtractionStarted += (s, e) => TestContext.WriteLine(String.Format("[{0}%] {1}", e.PercentDone, e.FileInfo.FileName));
				tmp.FileExists += (o, e) =>{
					TestContext.WriteLine("Warning: file \"" + e.FileName + "\" already exists.");
					//e.Overwrite = false;
				};
				tmp.ExtractionFinished += (s, e) => { TestContext.WriteLine("Finished!"); };
				var rand = new Random();
				tmp.ExtractFile(rand.Next((int) tmp.FilesCount), File.Create(createTempFileName()));
			}
		}

		/*[TestMethod]
		public void ToughnessTestThrowsNoExceptionsAndNoLeaks() {
			Console.ReadKey();
			string exeAssembly = Assembly.GetAssembly(typeof(SevenZipExtractor)).FullName;
			AppDomain dom = AppDomain.CreateDomain("Extract");
			for (int i = 0; i < 1000; i++) {
				using (SevenZipExtractor tmp =
					(SevenZipExtractor)dom.CreateInstance(
						exeAssembly, typeof(SevenZipExtractor).FullName,
						false, BindingFlags.CreateInstance, null,
						new object[] { Path.Combine(archivePath, "Test.lzma.7z") },
						System.Globalization.CultureInfo.CurrentCulture, null, null
					).Unwrap()
				) {
					tmp.ExtractArchive(tempFolder);
				}
				Console.Clear();
				Console.WriteLine(i);
			}
			AppDomain.Unload(dom);
		}*/
	}
}