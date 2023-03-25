using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;

namespace Impersonate.Util
{
    public class PipeServerUtil
    {
        private NamedPipeServerStream server = null;
        private Thread worker = null;
        private bool is_terminated = false;
        public delegate Process external_method(string application_name, string command_line);
        private external_method method = null;

        public PipeServerUtil(external_method method, string pipe_name) 
        {
            // impersonate 실행 메서드
            this.method = method;

            // 다른 권한의 프로세스로와 연결을 위한 설정
            var psRule = new PipeAccessRule(@"Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            var ps = new PipeSecurity();
            ps.AddAccessRule(psRule);

            // 하나의 접속만 허용
            this.server = new NamedPipeServerStream(pipe_name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1, 1, ps);
            this.worker = new Thread(this.ThreadWorker);
            this.worker.Start();
        }

        private void ThreadWorker()
        {
            while (this.is_terminated == false)
            {
                try
                {
                    this.server.WaitForConnection();

                    while (this.server.IsConnected == true)
                    {
                        byte[] read = new byte[byte.MaxValue];
                        int result = server.Read(read, 0, read.Length);
                        if (result > 0)
                        {
                            /* TODO: 프로토콜을 만들어 처리 */

                            // 실행할 process 와 parameter 전달
                            var process = this.method("{PROCESS}", "{PARAMETERS}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
