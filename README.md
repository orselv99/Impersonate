# Impersonate
---
- Avoiding User Access Control (UAC) but launching an elevated process.

## Implementation
---
- When a process is created using a Windows Service, the process runs with service privileges. 
- But if you create a token of a non-elevated process running with privileges belonging to the Administrators group, clone and make it an impersonated token to create a process, it will run as an elevated process.

## References
---
- Windows Service
    - https://learn.microsoft.com/en-us/dotnet/framework/windows-services/how-to-install-and-uninstall-services
- Impersonated token
    - https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-processidtosessionid
    - https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtsqueryusertoken
    - https://learn.microsoft.com/en-us/windows/win32/api/securitybaseapi/nf-securitybaseapi-gettokeninformation
    - https://learn.microsoft.com/en-us/windows/win32/api/securitybaseapi/nf-securitybaseapi-duplicatetokenex
    - https://learn.microsoft.com/en-us/windows/win32/api/userenv/nf-userenv-createenvironmentblock
    - https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessasusera
    - https://learn.microsoft.com/en-us/windows/win32/secgloss/i-gly
