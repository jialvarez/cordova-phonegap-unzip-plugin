cordova-phonegap-unzip-plugin
=============================

UnZip Plugin for Phonegap / Windows Phone (SL4). Allows:

* Download a zip file from external URL
* Unzip a gzip file from SDCard
* Get info of the zip file

In order to check uncompress was sucessful, use Isolated Storage Explorer Tool.

Usage:

<pre>ISETool.exe <ts|rs|EnumerateDevices|dir[:device-folder]> <xd|de|deviceindex[:n]> <Product GUID> [<desktop-path>]</pre>

Example of usage:

<pre>"c:\Program Files (x86)\Microsoft SDKs\Windows Phone\v7.1\Tools\IsolatedStorageExplorerTool\ISETool.exe" dir:"Shared\test" xd {1637b4d1-a924-4384-b40b-401cc09668a9}</pre>

More info about this tool: 

http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh286408(v=vs.92).aspx
