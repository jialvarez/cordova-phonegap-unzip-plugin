/*
       Licensed to the Apache Software Foundation (ASF) under one
       or more contributor license agreements.  See the NOTICE file
       distributed with this work for additional information
       regarding copyright ownership.  The ASF licenses this file
       to you under the Apache License, Version 2.0 (the
       "License"); you may not use this file except in compliance
       with the License.  You may obtain a copy of the License at

         http://www.apache.org/licenses/LICENSE-2.0

       Unless required by applicable law or agreed to in writing,
       software distributed under the License is distributed on an
       "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
       KIND, either express or implied.  See the License for the
       specific language governing permissions and limitations
       under the License.
*/

function fail(error) {
	console.log(error.source);
	console.log(error.target);
	console.log(error.code);
}

function getEntityFail(fileError) {
	console.log("FileError!");
	console.log("code: "+fileError.code);
}

var deviceInfo = function() {
    document.getElementById("platform").innerHTML = device.platform;
    document.getElementById("version").innerHTML = device.version;
    document.getElementById("uuid").innerHTML = device.uuid;
    document.getElementById("name").innerHTML = device.name;
    document.getElementById("width").innerHTML = screen.width;
    document.getElementById("height").innerHTML = screen.height;
    document.getElementById("colorDepth").innerHTML = screen.colorDepth;
};

function displayZipInfo (zipEntry) {
    document.getElementById("zip_name").innerHTML = zipEntry.name;
    document.getElementById("zip_fullpath").innerHTML = zipEntry.fullPath;
    document.getElementById("zip_entries").innerHTML = zipEntry.entries;
}

function zipInfo() {

	var source = "test.zip";

	window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, function (fileSystem) {
                fileSystem.root.getFile(source, null, function (fileEntry) {
					window.plugins.Zip.info(fileEntry.fullPath, displayZipInfo, function(){});
                }, getEntityFail);
        }, fail);
}

function successListener (msg) {
	console.log("isFile: " + msg.isFile);
	console.log("isDirectory: " + msg.isDirectory);
	console.log("name: " + msg.name);
	console.log("fullPath: " + msg.fullPath);
	console.log("completed: " + msg.completed);
	console.log("progress: " + msg.progress);
}

function uncompressFromSDCard() {
	console.log("uncompressFromSDCard");
	var source = "test.zip";
	var target = "/Shared";

	window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, function (fileSystem) {
		console.log("request file system");
		fileSystem.root.getFile(source, {create: false, exclusive: false}, function (fileEntry) {
			console.log("request file:" + source);
			fileSystem.root.getDirectory(target, {create: true, exclusive: false}, function (directoryEntry) {
				console.log("request dir:" + target);
				window.plugins.Zip.uncompress(fileEntry.fullPath, directoryEntry.fullPath, successListener, function(){});
			}, getEntityFail);
		}, getEntityFail);
	}, fail);
}

function uncompressFromURL() {

	var url = "http://www.litio.org/tmp/test.zip";
	var targetName = "test.zip";

	window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, function (fileSystem) {

		fileSystem.root.getFile(targetName, {create: true, exclusive: true}, function (fileEntry) {

			var localPath = fileEntry.fullPath;
			console.log("++++ localPath: "+localPath);
			
			/*
			if (device.platform === "Android" && localPath.indexof("file://") === 0) {
				localPath = localPath.substring(7);
			}
			*/
	
			var fileTransfer = new FileTransfer();
	
			fileTransfer.download(
				url,
				localPath,
				function (entry) {
					console.log("download complete: " + entry.fullPath);
					console.log("+ info: " + entry);
				},
				function (error) {
					console.log("download error source " + error.source);
					console.log("download error target " + error.target);
					console.log("upload error code" + error.code);
				}
			);
		}, fail);
	}, fail);

}


function uncompress(source, target) {

	console.log("uncompress("+source+","+target+")");
	window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, function (fileSystem) {

		fileSystem.root.getFile(target, null, function (fileEntry) {

			var localPath = fileEntry.fullPath;
			var zip = window.plugins.Zip;
			zip.uncompress(source, target, successListener, fail);

		}, fail);
	}, fail);
}

function compress() {

}

function init() {
    // the next line makes it impossible to see Contacts on the HTC Evo since it
    // doesn't have a scroll button
    // document.addEventListener("touchmove", preventBehavior, false);
    document.addEventListener("deviceready", deviceInfo, true);
}

