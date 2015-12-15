# Backup
Program to control [DriveSnapshot](http://www.drivesnapshot.de/en/down.htm "Download"), giving multiple months daily 
backups in minimal space.

It takes a full image backup of one or more disk drives at the start of the month, and 
a differential image every day thereafter.

Next month, it does the same, in a different folder.

The following month it uses a third folder. After that, it goes back to the first one.

That way you can restore your full system to any point in the last 2-3 months, or can also access 
any individual file or folder as it was on any day in the past 2-3 months.

Because the backups are compressed, the whole caboodle only takes about 4 times the size of the drive.

##Usage

Backup  [-&lt;generations&gt;] [-s(hutdown)] &lt;folder&gt; &lt;disk&gt; [&lt;disk&gt; ...]

&lt;generations&gt;: Number of month folders to create before reusing the first one. Default 3.

-s : Shuts down computer when backup is finished.

&lt;folder&gt;: Destination parent folder (folders named "0", "1", "2" will be created for the month folders below this one).

&lt;disk&gt;: Drive letter - e.g. C

#This program requires [Drive Snapshot](http://www.drivesnapshot.de/en/down.htm "Download") to function
