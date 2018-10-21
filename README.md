# ChemViewAR
A molecular viewer augmented reality android app. Designed using Unity and ARCore.

## Known Issues
* ~~Load time is a lot longer than it should be, issue arised after dropdown box implementation.~~ Resolved as of 10/09/2018
* Rotation functionality needs work, as conversion of user touch input to game world rotation could be better.
* Molecule information sheet will only be populated if the spawned molecule name matches an existing wikipedia page, so for now only the Acetone test molecule has a populated information sheet.

## To Do
* ~~Implement Molecule Info Sheet with wikipedia RESTful API implementation~~ Added as of 23/08/2018.
* ~~Unclutter UI by creating custom UI icons~~ Added as of 30/08/2018.
* ~~Improve molecule info sheet placement and increase RESTful API call efficiency so less calls are made.~~ Added as of 30/08/2018.
* ~~Add molecular diagram to molinfo sheet.~~ Added as of 10/09/2018
* ~~Improve scaling functionality so molecule doesn't 'rubber band' to the position of the finger on the screen.~~ Added as of 18/09/2018
* ~~Further improve molecule info sheet to account for space taken up by parent molecule~~ Added as of 22/09/2018
* ~~Further improve molecule info sheet  to stay in place when molecule is spinning or being rotated, but still draggable.~~ Added as of 02/10/2018
* ~~Incorporate ARCoreUtils to provide collision detection to generated tracked planes.~~ Custom collision detection added as of 11/10/2018

* Further improve rotation functionality.
* Further improve custom icon designs and make them more 'chemistry specific'. 
* Expand molecule info list to pull spectrum from either an API or database.

## Possible Future Implementations
* Add functionality to turn on the camera flash to be used as a torch to light up low light environments. Currently not supported in ARCore.
* Begin implementation of 'image recognition' augmented reality to spawn in 3D molecules from their skeletal diagram (possibly as separate branch).
