@echo off
echo Installing required Python packages...

:: Ensure pip is up to date
python -m pip install --upgrade pip

:: Install required packages
pip install python-osc spotipy psutil ttkthemes

echo.
echo All required packages have been installed.
pause
