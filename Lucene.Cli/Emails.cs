using System;
using System.IO;
using System.Collections.Generic;
using MimeKit;
using NachoCore.Index;

namespace Lucene.Cli
{
    public partial class LuceneCli
    {
        private static List<string> _EmailPaths;

        private static List<string> EmailPaths {
            get {
                if (null == _EmailPaths) {
                    _EmailPaths = new List<string> ();
                }
                return _EmailPaths;
            }
        }

        private static void GetAllFiles (string[] args)
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

        private static void AddEmails (string[] paths)
        {
            long bytesIndexed = 0;
            GetAllFiles (paths);
            Log.Info ("Adding {0} emails", EmailPaths.Count);
            using (var index = new Index (IndexDirectory)) {
                index.BeginAddTransaction ();
                foreach (var emailPath in EmailPaths) {
                    Log.Debug ("Add {0}", emailPath);
                    bytesIndexed += index.BatchAdd (emailPath, "message", emailPath);
                }
                index.EndAddTransaction ();
            }
            Log.Info ("{0} bytes indexed", bytesIndexed);
        }

        private static void RemoveEmails (string[] paths)
        {
            GetAllFiles (paths);
            Log.Info ("Removing {0} emails", EmailPaths.Count);
            int count = 0;
            using (var index = new Index (IndexDirectory)) {
                index.BeginRemoveTransaction ();
                foreach (var emailPath in EmailPaths) {
                    if (!index.Remove ("message", emailPath)) {
                        count += 1;
                    }
                }
                index.EndRemoveTransaction ();
            }
            Log.Info ("{0} emails removed", count);
        }

        private static void SearchEmails (string search)
        {
            using (var index = new Index (IndexDirectory)) {
                int n = 1;
                foreach (var match in index.Search(search)) {
                    Log.Info ("{0} {1}", n, match.Id);
                    n += 1;
                }
            }
        }
    }
}
