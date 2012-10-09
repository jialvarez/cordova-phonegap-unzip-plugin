/*
 * Copyright (C) 2012 by Emergya
 *
 * Author: Jose I. Alvarez Ruiz <jialvarez@emergya.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/


using System;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Net;

namespace WP7CordovaClassLib.Cordova.Commands
{
    public class Zip : BaseCommand
    {
        [DataContract]
        public class ZipOptions
        {
            [DataMember]
            public string source;

            [DataMember]
            public string target;
        }

        public void info(string options)
        {
            // bring options
            Debug.WriteLine("raw options for info: " + options);
            ZipOptions zipOptions = JSON.JsonHelper.Deserialize<ZipOptions>(options);

            string source = zipOptions.source;
            Debug.WriteLine("selected source for info:" + source);

            // ZipFile.Count not working for me, larger process needed
            int count = 0;
            IsolatedStorageFile infile = IsolatedStorageFile.GetUserStoreForApplication();

            using (ZipInputStream s = new ZipInputStream(infile.OpenFile(source, FileMode.Open)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    count++;
                }
            }

            Debug.WriteLine("Count: " + count);

            // extract last item from path (filename)
            string[] fileParts = source.Split('/');
            Debug.WriteLine("length: " + fileParts.Length);
            string name = fileParts[fileParts.Length - 1];

            // prepare return for plugin to js, communication using JSON format
            string zipInfo = "{\"entries\":\"" + count + "\", \"fullPath\":\"" + source + "\", \"name\":\"" + name + "\"}";
            PluginResult result = new PluginResult(PluginResult.Status.OK, zipInfo);
            result.KeepCallback = true;
            this.DispatchCommandResult(result);
        }

        public void uncompress(string options)
        {
            // bring options
            Debug.WriteLine("raw options for uncompress: " + options);
            ZipOptions zipOptions = JSON.JsonHelper.Deserialize<ZipOptions>(options);
            Debug.WriteLine("selected source for uncompress:" + zipOptions.source);
            Debug.WriteLine("selected target for uncompress:" + zipOptions.target);

            // prepare file handlers for SL 4 
            IsolatedStorageFile infile = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFile outfile = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFile path = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFile directory = IsolatedStorageFile.GetUserStoreForApplication();

            // direct access to targetPath
            string targetPath = zipOptions.target;

            // open zip file
            using (ZipInputStream decompressor = new ZipInputStream(infile.OpenFile(zipOptions.source, FileMode.Open)))
            {
                ZipEntry entry;
 
                // iterate through entries of the zip file
                while ((entry = decompressor.GetNextEntry()) != null)
                {
                    string filePath = Path.Combine(targetPath, entry.Name); 
                    string directoryPath = Path.GetDirectoryName(filePath);
 
                    // create directory if not exists
                    if (!string.IsNullOrEmpty(directoryPath) && !directory.FileExists(directoryPath))
                    {
                        directory.CreateDirectory(directoryPath);
                    }
 
                    // don't consume cycles to write files if it's a directory
                    if (entry.IsDirectory)
                    {
                        continue;
                    }
 
                    // unzip and create file
                    byte[] data = new byte[2048];
                    using (FileStream streamWriter = outfile.CreateFile(filePath))
                    {
                        int bytesRead;
                        while ((bytesRead = decompressor.Read(data, 0, data.Length)) > 0)
                        {
                            streamWriter.Write(data, 0, bytesRead);
                        }
                    }
                }
            }
        }
    }
}
