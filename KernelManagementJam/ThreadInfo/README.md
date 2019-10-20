## Crossplatform CPU Usage metrics
It receives the amount of time that the **thread/process** has executed in _**kernel**_ and _**user**_ mode.

Works everywhere: Linux, OSX and Windows.

## Coverage
Minimum OS requirements: Linux Kernel 2.6.26, Mac OS 10.9, Windows XP/2003

Autotests using .NET Core cover:  
- Linux on x64 using plenty linux distributions 
- Linux on ARM 64-bit using Debian
- Mac OS x64 10.13 & 10.14

Windows x86/x64 and ARM 32-bit are manually tested only. 

It should support Linux x86 and BSD-like system with linux compatibility layer using mono, but was never tested. 
 
## Implementation
The implementation utilizes platform invocation of the corresponding system libraries depending on the OS:  

| OS       | per thread function      | per process function   | library         |
|----------|--------------------------|------------------------|-----------------|
| Linux    | getrusage(RUSAGE_THREAD) | getrusage(RUSAGE_SELF) | libc.so         |
| Windows  | GetThreadTimes()         | GetProcessTimes()      | kernel32.dll    |
| Mac OS X | thread_info()            | getrusage(RUSAGE_SELF) | libSystem.dylib |
