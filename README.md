# HelloRift
VR Experiment implementation for Paper "Depth Discrimination in 3D Clutter"

This project/experiment is designed to test and understand how does human vision perceive depth in a cluttered 3D scene, the motivation and design is detailed in an upcoming paper named in the title.

System requirements:
Requires Unity 5.0.4f1 (NOT with above 5.1 as Unity revamped VR support and I can't find a way to disable binocular camera)
Requires Oculus Rift DK2 (Only tested device, maybe with future commercial OR release version, but surely not compatible with any VR without position tracking, i.e. DK1 etc.)
Compatible with Windows and Mac OS X

Quick starting steps:
1, install Unity 5.0.4f1
2, install Oculus Runtime for Windows or Mac V0.5.0.1-beta (only this version, higher version requires higher Unity version which is not compatible)
3, using Unity to open this project
4, File -> Build Settings -> PC & Mac Standalone -> Build
5, Find the file named XXX_DirectToRift.exe or app on mac, run it
6, out on your VR and let the experiment start

Instruction while doing Experiment:
Left Ctrl key to recenter the scene.
If you see two red short bars side by side, using left or right arrow key to indicate which one appears closer to you (the viewer), if two long bars up and down, use up or down arrow key to indicate which one appears closer.
When prompt to take a test, do it! When you feel like continuing, press space key.
Esc key to exit any time.
When it reads "This experiment requires motion", you have to move left and right before you make your decision, if it reads "This experiment does not require motion", you have to stay still when making decision.
After the experiment is complete, there will be three data files in the \output\ folder, "result" and "staircases" are excel files, both quite self explainary. All units are in meters.
