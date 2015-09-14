# cellVIEW


How to:

The HIV + Blood plasma dataset has already been baked and can be loaded by simply loading the scene located in the scenes folder.
It is also possible to bake a scene based on cellPACK results files, however, a powerful GPU is prefered for this option.

Prerequisite to build a scene:

In order to be able to load a scene it is necessary to add/modify the TdrDelay key from the windows registery.
This value correspond to the timeout value after which the GPU driver will restart when the GPU is busy.
By default this value is set to 2 seconds on Windows, and should be changed to order to allow more computing time for the loading, a value of 20/30 seconds is enough on most decents graphics hardware.


