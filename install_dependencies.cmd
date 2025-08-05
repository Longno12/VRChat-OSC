@echo off
echo Installing required Python packages...
python -m pip install --upgrade pip
pip install python-osc spotipy psutil ttkthemes
echo.
echo All required packages have been installed.
pause

