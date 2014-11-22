using System;
using System.IO;
using System.Collections.Generic;
using MimeKit;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Lucene.Cli
{
	public partial class LuceneCli
	{
		private IndexWriter _Writer;
		private IndexWriter Writer {
			get {
				if (null == _Writer) {
					_Writer = new IndexWriter (Index, Analyzer, IndexWriter.MaxFieldLength.LIMITED);
				}
				return _Writer;
			}
		}

		private IndexReader _Reader;
		private IndexReader Reader {
			get {
				if (null == _Reader) {
					_Reader = IndexReader.Open (Index, true);
				}
				return _Reader;
			}
		}

		private IndexSearcher _Searcher;
		private IndexSearcher Searcher {
			get {
				if (null == _Searcher) {
					_Searcher = new IndexSearcher (Reader);
				}
				return _Searcher;
			}
		}

		private List<string> _EmailPaths;
		private List<string> EmailPaths {
			get {
				if (null == _EmailPaths) {
					_EmailPaths = new List<string> ();
				}
				return _EmailPaths;
			}
		}

		private void CleanupWriter ()
		{
			if (null == _Writer) {
				return;
			}
			_Writer.Commit ();
			_Writer.Dispose ();
			_Writer = null;
		}

		private void CleanIndexReader ()
		{
			if (null == _Reader) {
				return;
			}
			_Reader.Commit ();
			_Reader.Dispose ();
			_Reader = null;
		}

		private void GetAllFiles (string[] args)
		{
			foreach (var path in args) {
				// Check if the path exist
				if (File.Exists (path)) {
					EmailPaths.Add (path);
					continue;
				}

				if (Directory.Exists (path)) {
					var files = Directory.GetFiles (path);
					if (0 < files.Length) {
						GetAllFiles (files);
					}
					var dirs = Directory.GetDirectories (path);
					if (0 < dirs.Length) {
						GetAllFiles (dirs);
					}
					continue;
				}

				Log.Warn ("{0} is neither a file nor a directory", path);
			}
		}
		private void AddEmails (string[] paths)
		{
			long bytesIndexed = 0;
			GetAllFiles (paths);
			Log.Info ("Adding {0} emails", EmailPaths.Count);
			foreach (var emailPath in EmailPaths) {
				bytesIndexed += AddEmail (emailPath);
			}
			CleanupWriter ();
			Log.Info ("{0} bytes indexed", bytesIndexed);
		}

		private void RemoveEmails (string[] paths)
		{
			GetAllFiles (paths);
			Log.Info ("Removing {0} emails", EmailPaths.Count);
			foreach (var emailPath in EmailPaths) {
				RemoveEmail (emailPath);
			}
			CleanupWriter ();
			Log.Info ("{0} emails removed", EmailPaths.Count);
		}

		private long AddEmail (string emailPath)
		{
			Log.Debug ("Add {0}", emailPath);
			// MIME parse the message 
			MimeMessage message;
			using (var fileStream = new FileStream (emailPath, FileMode.Open, FileAccess.Read)) {
				message = MimeMessage.Load (fileStream);
			}

			// Index the body
			long bytesIndexed = 0;
			var doc = new Document ();
			foreach (var part in message.BodyParts) {
				var textPart = part as TextPart;
				if (null == textPart) {
					continue;
				}
				var body = textPart.Text;
				Log.Debug ("body = {0}", body);
				var bodyField = new Field ("body", new StringReader (body));
				doc.Add (bodyField);
				Writer.AddDocument (doc);
				bytesIndexed += body.Length;
			}
			return bytesIndexed;
		}

		private void RemoveEmail (string emailPath)
		{
			Log.Debug ("Remove {0}", emailPath);
		}

		private void SearchEmails (string search)
		{
			var query = new QueryParser (Lucene.Net.Util.Version.LUCENE_30, "body", Analyzer).Parse (search);

			var matches = Searcher.Search (query, 1000);
			Log.Info ("{0} hits", matches.TotalHits);
			Log.Info ("{0} max score", matches.MaxScore);
			foreach (var doc in matches.ScoreDocs) {
				Log.Info ("{0}", doc.ToString ());
			}
		}
	}
}
