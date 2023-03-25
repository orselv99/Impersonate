using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Impersonate.Util
{
    static class ProcessUtil
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool ProcessIdToSessionId(int dwProcessId, ref int pSessionId);

        [DllImport("Wtsapi32.dll", SetLastError = true)]
        private static extern int WTSQueryUserToken(int SessionId, ref IntPtr phToken);

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_LINKED_TOKEN
        {
            public IntPtr LinkedToken;
        }

        private enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern int GetTokenInformation(
            IntPtr TokenHandle, 
            TokenInformationClass TokenInformationClass,
            ref TOKEN_LINKED_TOKEN buffer, 
            int bufferLength,
            ref int returnLength
        );

        private enum SecurityImpersonationLevel
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        private enum TokenType
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern int DuplicateTokenEx(
            IntPtr hExistingToken, 
            int dwDesiredAccess, 
            IntPtr lpTokenAttributes,
            SecurityImpersonationLevel ImpersonationLevel, 
            TokenType TokenType,
            ref IntPtr phNewToken
        );

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr handle);

        [Flags]
        public enum StartupInfoFlags
        {
            ForceOnFeedback = 0x40,
            ForceOffFeedback = 0x80,
            PreventPinning = 0x2000,
            RunFullScreen = 0x20,
            TitleIsAppId = 0x1000,
            TitleIsLinkName = 0x800,
            UseCountChars = 0x8,
            UseFillAttribute = 0x10,
            UseHotKey = 0x200,
            UsePosition = 0x4,
            UseShowWindow = 0x1,
            UseSize = 0x2,
            UseStdHandles = 0x100,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StartupInfo
        {
            public int Cb;
            public readonly string Reserved;
            public readonly string Desktop;
            public readonly string Title;
            public readonly int X;
            public readonly int Y;
            public readonly int XSize;
            public readonly int YSize;
            public readonly int XCountChars;
            public readonly int YCountChars;
            public readonly int FillAttribute;
            public readonly StartupInfoFlags Flags;
            public readonly short ShowWindow;
            public readonly short Reserved2;
            public readonly IntPtr Reserved3;
            public readonly IntPtr StandardInput;
            public readonly IntPtr StandardOutput;
            public readonly IntPtr StandardError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public readonly IntPtr Process;
            public readonly IntPtr Thread;
            public readonly int ProcessId;
            public readonly int ThreadId;
        }

        [DllImport("Userenv.dll", SetLastError = true)]
        private static extern int CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [Flags]
        public enum CreationFlags
        {
            Default = DefaultErrorMode | NewConsole | NewProcessGroup,
            DefaultErrorMode = 0x4000000,
            NewConsole = 0x10,
            NewProcessGroup = 0x200,
            SeparateWowVdm = 0x800,
            Suspended = 0x4,
            UnicodeEnvironment = 0x400,
            ExtendedStartupInfoPresent = 0x80000,
        }

        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int CreateProcessAsUser(
            IntPtr hToken, 
            string lpApplicationName, 
            StringBuilder lpCommandLine,
            IntPtr lpProcessAttributes, 
            IntPtr lpThreadAttributes,
            bool bInheritHandles, 
            CreationFlags dwCreationFlags,
            IntPtr lpEnvironment, 
            string lpCurrentDirectory,
            ref StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation
        );

        [DllImport("Userenv.dll", SetLastError = true)]
        private static extern int DestroyEnvironmentBlock(IntPtr lpEnvironment);

        public static Process Start(string application_name, string command_line)
        {
            // user session_id 를 확인하기위해 service 와 통신하는 process 를 찾아서 process_id 를 확인
            // ** 반드시 administrator group 에 속한 프로세스여야 함
            var processes = Process.GetProcessesByName("{NON_ELEVATED_PROCESS_BUT_BELONGING_TO_ADMINGROUP}");
            if (processes.Length == 0)
            {
                throw new Win32Exception();
            }

            int session_id = 0;
            if (ProcessIdToSessionId(processes[0].Id, ref session_id) == false)
            {
                throw new Win32Exception();
            }

            // 토큰 확인
            var token = IntPtr.Zero;
            if (WTSQueryUserToken(session_id, ref token) == 0)
            {
                throw new Win32Exception();
            }

            // windows vista 이상부터 uac 적용
            var admin_token = token;
            if (Environment.OSVersion.Version >= new Version(6, 0))
            {
                // admin 권한의 token 을 얻기위해 linked_token 을 확인하여 복제
                var linked = new TOKEN_LINKED_TOKEN();
                int returned = 0;
                if (GetTokenInformation(
                    token, 
                    TokenInformationClass.TokenLinkedToken, 
                    ref linked,
                    Marshal.SizeOf(linked), 
                    ref returned) == 0)
                {
                    throw new Win32Exception();
                }

                var linked_token = linked.LinkedToken;
                if (DuplicateTokenEx(
                    linked_token, 
                    0, 
                    IntPtr.Zero,
                    SecurityImpersonationLevel.SecurityImpersonation,
                    TokenType.TokenPrimary,
                    ref admin_token) == 0)
                {
                    throw new Win32Exception();
                }

                // 더이상 사용하지 않는 토큰 release
                CloseHandle(linked_token);
                CloseHandle(token);
            }

            try
            {
                // admin 권한에 적용되는 환경변수를 확인
                var environment = IntPtr.Zero;
                if (CreateEnvironmentBlock(ref environment, admin_token, true) == 0)
                {
                    throw new Win32Exception();
                }

                // 프로세스 실행
                ProcessInformation process_info;
                var command_line_text = new StringBuilder(string.Format("\"{0}\" {1}", application_name, command_line));
                var startup_info = new StartupInfo();
                startup_info.Cb = Marshal.SizeOf(startup_info);

                try
                {
                    if (CreateProcessAsUser(
                        admin_token, 
                        null,
                        command_line_text, 
                        IntPtr.Zero, 
                        IntPtr.Zero, 
                        false,
                        CreationFlags.UnicodeEnvironment,
                        environment,
                        null,
                        ref startup_info,
                        out process_info) == 0)
                    {
                        throw new Win32Exception();
                    }
                }
                finally
                {
                    // 환경변수 객체 release
                    DestroyEnvironmentBlock(environment);
                }

                // release
                CloseHandle(process_info.Process);
                CloseHandle(process_info.Thread);

                return Process.GetProcessById(process_info.ProcessId);
            }
            finally
            {
                CloseHandle(admin_token);
            }
        }
    }
}
