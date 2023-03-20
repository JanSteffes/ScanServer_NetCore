# ScanServer_NetCore


## Overview
Enables old non-network-able scanners to be reachable using this net core backend as an endpoint, running on a linux system. 
For me, it's running on a raspberrpi with Raspbian GNU/Linux 10 (buster). 

Can provide overview over already scanned files and enbales scanning of new files in predefined qualities.
Uses [scanimage](https://linux.die.net/man/1/scanimage) to scan files and further [tiff2pd](https://linux.die.net/man/1/tiff2pdf), [pdftops](https://linux.die.net/man/1/pdftops) and [ps2pdf](https://linux.die.net/man/1/ps2pdf) chained to minimize filesize used

## Requirements
Requires GhostScript to be installed for creating thumbnails, see https://ghostscript.com/doc/9.55.0/Install.htm
* On Windows, add path to <installdir>/bin to path variables and copy gswin64c.exe to gs.exe


## Other

### Interactions
Can be used via swagger on any device with a mobile browser. 
Besides that, there's a flutter app also created by me to interact with this backend: [scan_client](https://github.com/JanSteffes/scan_client)

### Intention
This started as a home project and will be handled as such. 
There might be updates in the future. As long as it's working for me and i don't need any new features, it may stays as it is.
