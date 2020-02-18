# Neuralia.Blockchains.Tools

##### Version:  Release Candidate III

This library contains the essential low level tools used by the blockchain.


## Build Instructions

##### First, ensure dotnet core 3.1 SDK is installed

#### The first step is to ensure that the dependencies have been built and copied into the local-source folder.

 - Neuralia.Data.HashFunction.xxHash

the best way to include it into other projects is to build it as a nuget package. 
To do so, simply invoke pack.sh
> ./pack.sh
this will produce a package name Neuralia.Blockchains.Tools.*[version]*.nupkg
