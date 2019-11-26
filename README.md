# ProcessManager
#### Lightweight, easy-to-use command-line task manager. It's time to ditch taskkill and tasklist.

PRM is the main project. It is the command-line task manager, written in C#. Needs many more improvements.  
SUDO is a standalone that summons PRM, to execute something with administrator priviledge, mimicking Linux's SUDO.

SYS (abandoned) is side project, which meant to simplify calling shell commands from prm. SYS will might be removed in near future.

## Purpose
This project demonstrates abilities of .NET Framework (and possibly .NET Compatible) in managing processes.  
Managing processes means starting and terminating processes with rules. System.Diagnostics provides powerful yet simple way for mostly all requirements.

At this point, we can provide our own commands that can be run through shells (such as cmd, etc):
- Starting processes
- Starting processes as administrator
- Terminating processes
- Locating processes with File Explorer
- Locating processes in command prompt
- Summarizing resource usage

By providing all those capabilities in PRM, I would say that PRM has all minimum functionalities as Task Manager, excluding uncommonly needed things, but also extends the shell capabilities.

## Issues
This project is developed in Windows 10 machine, where console windows can be maximized and even full screen. PRM may require more console width for now, especially when monitoring resource usage, which will cause issues in older version of windows where maximum width of console is limited to half of the screen size.

Also, relying fully on .NET for this purposes, at some point, is not really a good idea. Getting file location using .NET Framework would oftenly get us 'Access Denied'. Using services and Windows' DLLs will work for this.


I myself prefer using PRM instead of taskkill, tasklist, and Windows Task Manger. It's reliable and easy to use anyway.
I hope this project can give good examples, showing how to manage (start, terminate) processes on your own application.
