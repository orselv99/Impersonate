# Impersonate
- User Access Control (UAC) 가 필요한 프로세스 실행시 UAC 창을 회피하기 위함.

## Implementation
- 서비스를 이용하여 실행하면, 프로세스는 서비스 권한으로 실행된다.
- 관리자 그룹에 속한 권한을 갖고 실행된 프로세스 (일반적인 로그온 유저) 의 토큰을 생성하여 가장토큰으로 복제하여 이를 가지고 프로세스를 실행한다.
- 서비스 권한이 아닌 관리자 권한으로 승격하여 프로세스를 실행할 수 있다.

## References
- Named Pipe
    - https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication
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
