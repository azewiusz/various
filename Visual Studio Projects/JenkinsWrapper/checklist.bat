@ECHO OFF
ECHO.
ECHO Checking environment variables...
ECHO.
SET ERR=
SET ERR2=
IF "%JAVA_HOME%"=="" (
   ECHO JAVA_HOME	[FAILED] the environment variable is not defined.
   SET ERR=1
) ELSE (
   ECHO JAVA_HOME	[OK]	"%JAVA_HOME%"
)

IF "%JENKINS_HOME%"=="" (
   ECHO JENKINS_HOME	[FAILED] the environment variable is not defined.
   SET ERR=1
   SET ERR2=1
) ELSE (
   ECHO JENKINS_HOME	[OK]	"%JENKINS_HOME%"
)

IF "%ERR2%"=="1" (
   ECHO.
   ECHO Basic Environment Variables Missing, re-run this script after providing above variables.
   goto finito
   )


IF not EXIST "%JENKINS_HOME%\service.bat" (
   ECHO runbinary	[FAILED] missing file "%JENKINS_HOME%\service.bat"
   SET ERR=1
) ELSE (
   ECHO runbinary files	[OK]	"%JENKINS_HOME%\service.bat"
)

IF not EXIST "%JENKINS_HOME%\start.bat" (
   ECHO runbinary	[FAILED] missing file "%JENKINS_HOME%\start.bat"
   SET ERR=1
) ELSE (
   ECHO runbinary files	[OK]	"%JENKINS_HOME%\start.bat"
)
:finito
ECHO.
pause Press any key to close this window...




