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
   exit /B 2
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

IF "%ERR%"=="1" (
   ECHO.
   ECHO Missing prerequsities, see above list.
   exit /B 3
 ) 
@ECHO ON
installutil JenkinsWrapper.exe /servicename="ProcessWrapper" /servicedisplayname="Jenkins IE" /runbinary="%JENKINS_HOME%\service.bat" /workdir="%JENKINS_HOME%"
@ECHO OFF
ECHO installutil /u JenkinsWrapper.exe /servicename="ProcessWrapper" /servicedisplayname="Jenkins IE" > uniinstall.bat
ECHO del uniinstall.bat >> uniinstall.bat
@ECHO ON
