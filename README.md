# DAD project 2016-2017 #

Group 24 - Campus Alameda

Márcio Santos 76338

Diogo Ferreira 79018

Francisco Santos 79719

-------------------------------------------------------------------------------

## Setup

### Input files
Either copy the files where the Slave binary is, or change the config file by giving the full path to the file.</br>
### Custom dll
You need to copy the mylib.dll to the Slave execution folder.</br>
### PuppetMaster IP Override
Sometimes when we have multiple Network Cards, for instance with VirtualBox installed, the URL that the PuppetCard advertises is wrong.</br>By changing the file "Inputs/ip_tba_ppm.txt", you will override the default IP and be able to select a proper IP.</br>Be aware that:</br> 1) Anything that you put in this file will be considered an IP, ie no IP format checking;</br>2) It is only considered the file's first line;</br>3) In Windows a end of the line is different from other systems.
