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
using System.Collections.Generic;

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

        List<string> processedEntities = new List<string>();

        public void info(string options)
        {
            // bring options
            Debug.WriteLine("raw options for info: " + options);
            ZipOptions zipOptions = JSON.JsonHelper.Deserialize<ZipOptions>(options);

            string source = zipOptions.source;
            Debug.WriteLine("selected source for info:" + source);

            // ZipFile.Count not working for me, larger process needed
            long count = 0;
            count = this.getTotalOfEntries(source);

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

            string lastMsg;

            // get total of entries
            long count = 0;
            count = this.getTotalOfEntries(zipOptions.source);

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

                    this.processedEntities.Add(filePath);

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

                    lastMsg = this.publish(filePath, count);
                }
            }
        }

        private string publish(String file, long totalEntities)
	    {
            string jsonObj;

            float progress = (float) this.processedEntities.Count / totalEntities;
            progress = (int)(progress * 100);

            // construct 
            jsonObj = "{\"progress\": \"" + progress + "\", ";
            jsonObj += "\"entries\": \"" + (this.processedEntities.Count + 1) + "\", ";

            bool completed = totalEntities == this.processedEntities.Count;

            if (totalEntities == this.processedEntities.Count)
		    {
                jsonObj += "\"completed\": \"true\", ";
		    }
		    else
		    {
                jsonObj += "\"completed\": \"false\", ";
            }

            if (file[file.Length - 1] == '/')
            {
                jsonObj += "\"isFile\": \"false\", ";
                jsonObj += "\"isDirectory\": \"true\", ";
            }
            else
            {
                jsonObj += "\"isFile\": \"true\", ";
                jsonObj += "\"isDirectory\": \"false\", ";
            }

            jsonObj += "\"name\": \"" + file.Substring(file.LastIndexOf('/'), (file.Length - file.LastIndexOf('/'))) + "\", ";
            jsonObj += "\"fullPath\": \"" + file + "\"}";

		    PluginResult result = new PluginResult(PluginResult.Status.OK, jsonObj);
            result.KeepCallback = true;

            Debug.WriteLine(jsonObj);

            // Avoid to send the message "uncompress completed" twice.
            // This message is sended in the execute method.
            if (!completed) {
                this.DispatchCommandResult(result);
            }

            System.Threading.Thread.Sleep(100);

		    return jsonObj;
        }

        private long getTotalOfEntries(String source)
        {
            long count = 0;

            IsolatedStorageFile infile = IsolatedStorageFile.GetUserStoreForApplication();

            // get total count of entries
            using (ZipInputStream s = new ZipInputStream(infile.OpenFile(source, FileMode.Open)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
