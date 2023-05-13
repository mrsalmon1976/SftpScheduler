1. Run "build.ps1", which will
	a. Prompt for the new version number (enter in format 1.2.3) 
	b. Automatically update the relevant source files
	c. Build a release version of the solution
	d. Create the zip file
2. Go to GitHub and add these as new release files
	* The tag name MUST be in the format v<Version>, e.g. "v2.3.0" - this is used by the AutoUpdater
