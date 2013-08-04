@ECHO OFF
ECHO.
ECHO Checking environment variables...
ECHO.
IF "%JAVA_HOME%"=="" (
   ECHO JAVA_HOME	[FAILED] the environment variable is not defined.
   SET ERR=1
) ELSE (
   ECHO JAVA_HOME	[OK]	"%JAVA_HOME%"
)

IF "%JENKINS_HOME%"=="" (
   ECHO JENKINS_HOME	[FAILED] the environment variable is not defined.
   SET ERR=1
) ELSE (
   ECHO JENKINS_HOME	[OK]	"%JENKINS_HOME%"
)
ECHO.
IF "%ERR%"=="1" (
   ECHO Installation terminated.  
   exit /B 1
) ELSE ( 
   ECHO Installing service...
)

SET ERR=

@ECHO ON
installutil JenkinsWrapper.exe /servicename="ProcessWrapper" /servicedisplayname="Jenkins IE" /runbinary="%JENKINS_HOME%\service.bat" /workdir="%JENKINS_HOME%"
echo installutil /u JenkinsWrapper.exe /servicename="ProcessWrapper" /servicedisplayname="Jenkins IE" > uniinstall.bat
echo del uniinstall.bat >> uniinstall.bat
