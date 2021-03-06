@ECHO OFF

IF NOT "%~1"==""  (
	CALL:_VER_%~1
)

::We want the TargetVisualStudioVersion to be available outside this batch
::file, so establish it before calling SETLOCAL
SETLOCAL
IF "%ProgramFiles(X86)%"=="" (
	SET WOWRegistryAdjust=
) ELSE (
	CALL:SET6432
)
SET UseToolsVersion=
CALL:_TOOLS_%TargetVisualStudioVersion%
IF "%UseToolsVersion%"=="" (
	@ECHO Visual studio version not recognized.
	GOTO:EOF
)

SET HackToolsVersion=12.34

REG DELETE "HKLM\Software%WOWRegistryAdjust%\Microsoft\MSBuild\ToolsVersions\%HackToolsVersion%" /f 1>NUL 2>&1
::Ignore error state, delete fails if the key is not there.
REG ADD "HKLM\Software%WOWRegistryAdjust%\Microsoft\MSBuild\ToolsVersions\%HackToolsVersion%" 1>NUL 2>&1
IF ERRORLEVEL 1 (
	@ECHO Registry write permissions are required for this file.
	@ECHO Run from a Visual Studio 20xx Command Prompt opened as an Administrator.
	@PAUSE
	GOTO:EOF
)

REG COPY "HKLM\Software%WOWRegistryAdjust%\Microsoft\MSBuild\ToolsVersions\%UseToolsVersion%" "HKLM\Software%WOWRegistryAdjust%\Microsoft\MSBuild\ToolsVersions\%HackToolsVersion%" /s /f 1>NUL 2>&1
::VS Doesn't like the empty state on the default value, which is how this ends up. Delete the default value.
REG DELETE "HKLM\Software%WOWRegistryAdjust%\Microsoft\MSBuild\ToolsVersions\%HackToolsVersion%" /ve /f 1>NUL 2>&1
GOTO:EOF


:SET6432
::If this batch file is already running under a 32 bit process, then the
::reg utility will choose the appropriate registry keys without our help.
::This also means that this file should not be called to pre-set environment
::variables before invoking 32-bit processes that use these variables.
IF DEFINED PROCESSOR_ARCHITEW6432 (
	SET WOWRegistryAdjust=
) ELSE (
	SET WOWRegistryAdjust=\Wow6432Node
)
GOTO:EOF

:CLEAR64
SET WOWRegistryAdjust=
GOTO:EOF

:SETVAR
SET %~1=%~2
GOTO:EOF

:_VER_2005
:_VER_8.0
:_VER_v8.0
:_VER_8
CALL:SETVAR "TargetVisualStudioVersion" "v8.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "8.0"
GOTO:EOF

:_VER_2008
:_VER_9.0
:_VER_v9.0
:_VER_9
CALL:SETVAR "TargetVisualStudioVersion" "v9.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "9.0"
GOTO:EOF

:_VER_2010
:_VER_10.0
:_VER_v10.0
:_VER_10
CALL:SETVAR "TargetVisualStudioVersion" "v10.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "10.0"
GOTO:EOF

:_VER_2012
:_VER_11.0
:_VER_v11.0
:_VER_11
CALL:SETVAR "TargetVisualStudioVersion" "v11.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "11.0"
GOTO:EOF

:_VER_2013
:_VER_12.0
:_VER_v12.0
:_VER_12
CALL:SETVAR "TargetVisualStudioVersion" "v12.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "12.0"
GOTO:EOF

:_VER_2015
:_VER_14.0
:_VER_v14.0
:_VER_14
CALL:SETVAR "TargetVisualStudioVersion" "v14.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "14.0"
GOTO:EOF

:_VER_2017
:_VER_15.0
:_VER_v15.0
:_VER_15
CALL:SETVAR "TargetVisualStudioVersion" "v15.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "15.0"
CALL:SETVAR "TargetVisualStudioMajorMinorNextVersion" "16.0"
GOTO:EOF

:_VER_2019
:_VER_16.0
:_VER_v16.0
:_VER_16
CALL:SETVAR "TargetVisualStudioVersion" "v16.0"
CALL:SETVAR "TargetVisualStudioMajorMinorVersion" "16.0"
CALL:SETVAR "TargetVisualStudioMajorMinorNextVersion" "17.0"
GOTO:EOF

:_TOOLS_v8.0
CALL:SETVAR "UseToolsVersion" "2.0"
GOTO:EOF

:_TOOLS_v9.0
CALL:SETVAR "UseToolsVersion" "3.5"
GOTO:EOF

:_TOOLS_v10.0
:_TOOLS_v11.0
CALL:SETVAR "UseToolsVersion" "4.0"
GOTO:EOF

:_TOOLS_v12.0
CALL:SETVAR "UseToolsVersion" "12.0"
GOTO:EOF

:_TOOLS_v14.0
CALL:SETVAR "UseToolsVersion" "14.0"
GOTO:EOF

:_TOOLS_v15.0
CALL:SETVAR "UseToolsVersion" "15.0"
GOTO:EOF

:_TOOLS_v16.0
CALL:SETVAR "UseToolsVersion" "%ProjectToolsVersion%"
GOTO:EOF