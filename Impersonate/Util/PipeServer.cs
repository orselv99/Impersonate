using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;

namespace Impersonate.Util
{
    public class PipeServerUtil
    {
        private string pipe_name = "";
        private NamedPipeServerStream server = null;
        private PipeSecurity pipe_security = null;
        private Thread worker = null;
        private bool is_terminated = false;
        public delegate Process external_method(string application_name, string command_line);
        private external_method method = null;

        public PipeServerUtil(external_method method, string pipe_name) 
        {
            // impersonate 실행 메서드
            this.method = method;

            // pipe 서버이름
            this.pipe_name = pipe_name;

            // 다른 권한의 프로세스로와 연결을 위한 설정
            var pipe_access_rule = new PipeAccessRule(
                "Everyone", 
                PipeAccessRights.ReadWrite, 
                System.Security.AccessControl.AccessControlType.Allow
            );
            this.pipe_security = new PipeSecurity();
            this.pipe_security.AddAccessRule(pipe_access_rule);

            this.worker = new Thread(this.ThreadWorker);
            this.worker.Start();
        }

        private void ThreadWorker()
        {
            while (this.is_terminated == false)
            {
                try
                {
                    // 하나의 접속만 허용
                    this.server = new NamedPipeServerStream(
                        this.pipe_name, 
                        PipeDirection.In, 
                        1, 
                        PipeTransmissionMode.Byte, 
                        PipeOptions.Asynchronous, 
                        1, 
                        1, 
                        this.pipe_security
                    );
                
                    // 연결대기
                    this.server.WaitForConnection();

                    while (this.server.IsConnected == true)
                    {
                        byte[] read = new byte[byte.MaxValue];
                        if (server.Read(read, 0, read.Length) > 0)
                        {
                            /* TODO: 프로토콜을 만들어 처리 */

                            // 실행할 process 와 parameter 전달
                            var process = this.method("{PROCESS_YOU_WANT_TO_ELEVATE}", "{PARAMETERS}");
                        }

                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    this.server.Close();
                    this.server = null;
                }
            }
        }
    }
}
