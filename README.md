# ChemViewAR
A molecular viewer augmented reality android app. Designed using Unity and ARCore.

# Known Issues
* Load time is a lot longer than it should be, issue arised after dropdown box implementation.
* Rotation functionality needs work, as conversion of user touch input to game world rotation could be better.
* Molecule information sheet will only be populated if the spawned molecule name matches an existing wikipedia page, so for now only the Acetone test molecule has a populated information sheet.

# To Do
* Unclutter UI by creating custom UI icons.
* ~~Implement Molecule Info Sheet with wikipedia RESTful API implementation~~ Added as of 23/08/2018
* Improve molecule info sheet placement and increase RESTful API call efficiency so less calls are made.
