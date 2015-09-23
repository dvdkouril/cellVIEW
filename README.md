# cellVIEW

This software is the result of the effort the Illvisation research project at TU Wien, Austria [1] and
the Molecular Graphics Laboratory at Scripps [2] to enable real-time visualization of large molecular scenes.
The authors are not responsible for any consequences of the software usage. The software is intended to 
be used for non-commercial purposes only. In case of interest in commercial license, please contact
Mathieu Le Muzic <mathieu@cg.tuwien.ac.at>. 

Details about the techniques and also the internals of the tool can be found in a EG VCBM 2015 paper [3]
also available at Eurographics Digital Library [4]. Please use this work as reference in scientific
publications that use existing functionality or build on the top of the cellVIEW code-base. 

How to:

The HIV + Blood plasma dataset has already been baked and can be loaded by simply loading the scene
located in the scenes folder. It is also possible to bake a scene based on cellPACK results files,
however, a powerful GPU is prefered for this option.

Prerequisite to build a scene:

In order to be able to load a scene it is necessary to add/modify the TdrDelay key from the windows registery.
This value correspond to the timeout value after which the GPU driver will restart when the GPU is busy.
By default this value is set to 2 seconds on Windows, and should be changed to order to allow more computing 
time for the loading, a value of 20/30 seconds is enough on most decents graphics hardware.


[1] https://www.cg.tuwien.ac.at/research/projects/illvisation/
[2] http://mgl.scripps.edu/
[3] http://onlinelibrary.wiley.com/doi/10.1111/cgf.12370/pdf
[4] http://diglib.eg.org/handle/10.2312/vcbm20151209
