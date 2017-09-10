cd /d %~dp0
mkdocs serve

:: Run on port 8001, accessible over the local network.(http://127.0.0.1:8001/) , if 8000 port is used by other.
mkdocs serve --dev-addr=0.0.0.0:8001

pause