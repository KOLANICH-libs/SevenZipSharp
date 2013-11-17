using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace SevenZipTest2013
{
	public class SevenZipTest
	{
		/*
			 You may specify the custom path to 7-zip dll at SevenZipLibraryManager.LibraryFileName 
				or call SevenZipExtractor.SetLibraryPath(@"c:\Program Files\7-Zip\7z.dll");
				or call SevenZipCompressor.SetLibraryPath(@"c:\Program Files\7-Zip\7z.dll");
			 You may check if your library fits your goals with
				(SevenZipExtractor/Compressor.CurrentLibraryFeatures & LibraryFeature.<name>) != 0
			 Internal benchmark:
				var features = SevenZip.SevenZipExtractor.CurrentLibraryFeatures;
				TestContext.WriteLine(((uint)features).ToString("X6"));
			*/
		private TestContext testContextInstance;

		/// <summary>
		///Получает или устанавливает контекст теста, в котором предоставляются
		///сведения о текущем тестовом запуске и обеспечивается его функциональность.
		///</summary>
		public TestContext TestContext{
			get { return testContextInstance; }
			set { testContextInstance = value; }
		}

		public static string tempFolder, archivePath;

		public static string createTempFolder(){
			var path = createTempFileName();
			Directory.CreateDirectory(path);
			return path;
		}

		public static string createTempFileName(string fold = null){
			if (fold == null) fold = tempFolder;
			return Path.Combine(fold, Path.GetRandomFileName());
		}

		public static string createTempFile(string fold = null, int len = 1024*1024){
			var fileName = createTempFileName(fold);
			using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None)) fs.SetLength(len);
			return fileName;
		}

		public static string createTempCompressableFolder(){
			var fold = createTempFolder();
			createTempFile(fold);
			return fold;
		}

		public static void initTempDir(TestContext ctx){
			tempFolder = Path.Combine(ctx.TestRunDirectory, "TestTemp");
			Directory.CreateDirectory(tempFolder);
		}

		public static void removeTempDir(){
			Directory.Delete(tempFolder, true);
		}
	}
}