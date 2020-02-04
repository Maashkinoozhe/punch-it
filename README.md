# punch-it
Punch It is a working hour recording tool. Its designed with minimal user interaction overhead in mind.

Purpose
-------

State of Development
----------------------
This project is in an early state. There are not a lot of features but this version is working right now.

Usage
-----

PunchItClient.exe
        -h      --help                   Print this help.
        -e      --export                 Run in export mode, export saved records into csv formatted file.
        -s      --start-work             Start working on a package in the currently)
                                         active project if the day has started(no RecordEntries created so far)
                usage    -s <PackageKey>
                usage    --start-work <PackageKey>
