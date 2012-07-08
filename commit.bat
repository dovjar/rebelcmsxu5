@ECHO OFF
ECHO ------------------------------------------
ECHO Updating Sources...
ECHO ------------------------------------------
git pull
ECHO ------------------------------------------
ECHO Reticulating Splines...
ECHO ------------------------------------------
cd ..
git add .
git commit 
git push
ECHO ------------------------------------------
ECHO All done!
ECHO ------------------------------------------
@ECHO ON