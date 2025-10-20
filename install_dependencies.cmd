@echo off
echo Installing required Python packages for VRChat OSC Pro...
echo This may take a moment.

python -m pip install --upgrade pip

pip install python-osc spotipy psutil pypresence winsdk requests packaging customtkinter

echo.
echo All required packages have been installed successfully.
pause
